using System.Text;
using Asv.Common;
using Asv.IO;
using R3;

namespace Asv.Hal;

public enum TelnetOptions:byte
{
    Se = 0xF0,
    Nop = 0xF1,
    DataMark = 0xF2,
    Break = 0xF3,
    InterruptProcess = 0xF4,
    AbortOutput = 0xF5,
    AreYouThere = 0xF6,
    EraseCharacter = 0xF7,
    EraseLine = 0xF8,
    GoAhead = 0xF9,
    Sb = 0xFA,
    Will = 0xFB,
    Wont = 0xFC,
    Do = 0xFD,
    Dont = 0xFE,
    Iac = 0xFF
}

public class TelnetCommand(TelnetOptions code)
{
    public TelnetOptions Code { get; } = code;
}

public class TelnetOptionsCommand(TelnetOptions code, byte option)
    :TelnetCommand(code)
{
    public byte Option { get; } = option;
}

public class TelnetSubNegotiationCommand(TelnetOptions code, byte[] data)
    :TelnetCommand(code)
{
    public byte[] Data { get; } = data;
}

public class TelnetParser:DisposableOnceWithCancel
{
    private enum State
    {
        Normal,
        Iac,
        IacDoDontWillWont,
        IacSb,
        IacSe
    }
    private State _state;
    private TelnetOptions _command;
    private readonly Subject<TelnetCommand> _cmd;
    private readonly List<byte> _data = new(8);

    public TelnetParser()
    {
        _cmd = new Subject<TelnetCommand>().DisposeItWith(Disposable);
    }

    public void Parse(byte data)
    {
        switch (_state)
        {
            case State.Normal:
                StateNormal(data);
                break;
            case State.Iac:
                IacState(data);
                break;
            case State.IacDoDontWillWont:
                _cmd.OnNext(new TelnetOptionsCommand(_command,data));
                break;
            case State.IacSb:
                if (data == 0xFF)
                {
                    _state = State.IacSe;
                    return;
                }
                _data.Add(data);
                break;
            case State.IacSe:
                if (data == 0xF0)
                {
                    _cmd.OnNext(new TelnetSubNegotiationCommand(TelnetOptions.Sb, _data.ToArray()));
                }
                _data.Clear();
                _state = State.Normal;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void StateNormal(byte data)
    {
        switch (data)
        {
            case 0xFF:
                _state = State.Iac;
                break;
            default:
                ProcessNormalData(data);
                break;
        }
    }

    private void IacState(byte data)
    {
        switch (data)
        {
            case 0xF0: // SE - End of subnegotiation parameters
                // strange data byte here
                _state = State.Normal;
                break;
            case 0xF1: // NOP
                _state = State.Normal;
                _cmd.OnNext(new TelnetCommand(TelnetOptions.Nop));
                break;
            case 0xF2: // Data Mark
                _state = State.Normal;
                _cmd.OnNext(new TelnetCommand(TelnetOptions.DataMark));
                break;
            case 0xF3: // Break
                _state = State.Normal;
                _cmd.OnNext(new TelnetCommand(TelnetOptions.Break));
                break;
            case 0xF4: // Interrupt Process
                _state = State.Normal;
                _cmd.OnNext(new TelnetCommand(TelnetOptions.InterruptProcess));
                break;
            case 0xF5: // Abort Output
                _state = State.Normal;
                _cmd.OnNext(new TelnetCommand(TelnetOptions.AbortOutput));
                break;
            case 0xF6: // Are You There
                _state = State.Normal;
                _cmd.OnNext(new TelnetCommand(TelnetOptions.AreYouThere));
                break;
            case 0xF7: // Erase Character
                _state = State.Normal;
                _cmd.OnNext(new TelnetCommand(TelnetOptions.EraseCharacter));
                break;
            case 0xF8: // Erase Line
                _state = State.Normal;
                _cmd.OnNext(new TelnetCommand(TelnetOptions.EraseLine));
                break;
            case 0xF9: // Go Ahead
                _state = State.Normal;
                _cmd.OnNext(new TelnetCommand(TelnetOptions.GoAhead));
                break;
            case 0xFA: // IAC SB
                _state = State.IacSb;
                _command = TelnetOptions.Sb;
                break;
            case 0xFB: // IAC DO
                _command = TelnetOptions.Do;
                _state = State.IacDoDontWillWont;
                break;
            case 0xFC: // IAC DONT
                _command = TelnetOptions.Dont;
                _state = State.IacDoDontWillWont;
                break;
            case 0xFD: // IAC WILL
                _command = TelnetOptions.Will;
                _state = State.IacDoDontWillWont;
                break;
            case 0xFE: // IAC WONT
                _command = TelnetOptions.Wont;
                _state = State.IacDoDontWillWont;
                break;
            case 0xFF: // double IAC => just data
                ProcessNormalData(data);
                _state = State.Normal;
                break;
            default:
                // unknown command
                _state = State.Normal;
                break;
        }
    }

    protected virtual void ProcessNormalData(byte data)
    {
        
    }
}



public class TelnetKeyboard:KeyboardBase
{
    private const byte Null = 0x00;
    private const byte Backspace = 0x08;
    private const byte LineFeed = 0x0A;
    private const byte CarriageReturn = 0x0D;
    private const byte Escape = 0x1B;
    private const byte Delete = 0x7F;

    private const byte TelnetEcho = 0x01;
    private const byte TelnetSuppressGoAhead = 0x03;
    private const byte TelnetLineMode = 0x22;

    private readonly IDataStream _port;
    private readonly StringBuilder _vtSequence = new();
    private TelnetOptions _optionCommand;
    private bool _characterModeRequested;
    private bool _ignoreLineFeedOrNullAfterCarriageReturn;

    enum State
    {
        Idle,
        Iac,
        Esc,
        IacOption,
        SubNegotiation,
        SubNegotiationIac,
        Csi,
        CsiBracket,
        Ss3
    }
    private State _state;

    public TelnetKeyboard(IDataStream port)
    {
        _port = port;
        port.OnReceive.Subscribe(OnData).DisposeItWith(Disposable);
    }

    private void OnData(byte[] data)
    {
        if (data.Length == 0) return;
        RequestCharacterMode();

        foreach (var b in data)
        {
            if (_ignoreLineFeedOrNullAfterCarriageReturn)
            {
                if (b is LineFeed or Null)
                {
                    _ignoreLineFeedOrNullAfterCarriageReturn = false;
                    continue;
                }

                _ignoreLineFeedOrNullAfterCarriageReturn = false;
            }

            switch (_state)
            {
                case State.Idle:
                    _state = IdleMode(b);
                    break;
                case State.Iac:
                    _state = Iac(b);
                    break;
                case State.IacOption:
                    _state = IacOption(b);
                    break;
                case State.SubNegotiation:
                    _state = SubNegotiation(b);
                    break;
                case State.SubNegotiationIac:
                    _state = SubNegotiationIac(b);
                    break;
                case State.Esc:
                    _state = Esc(b);
                    break;
                case State.Csi:
                    _state = Csi(b);
                    break;
                case State.CsiBracket:
                    _state = CsiBracket(b);
                    break;
                case State.Ss3:
                    _state = Ss3(b);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    private State IdleMode(byte b)
    {
        switch (b)
        {
            case (byte)TelnetOptions.Iac:
                return State.Iac;
            case Escape:
                return State.Esc;
            case CarriageReturn:
                RiseEnterEvent();
                _ignoreLineFeedOrNullAfterCarriageReturn = true;
                return State.Idle;
            case LineFeed:
                RiseEnterEvent();
                return State.Idle;
            case Backspace:
            case Delete:
                RiseEscapeEvent();
                return State.Idle;
            case (byte)'.':
            case (byte)',':
                RiseDotEvent();
                return State.Idle;
            default:
                if (IsAsciiDigit(b))
                {
                    RiseDigitEvent((char)b);
                }
                return State.Idle;
        }
    }

    private State Iac(byte b)
    {
        switch ((TelnetOptions)b)
        {
            case TelnetOptions.Iac:
                return State.Idle;
            case TelnetOptions.Do:
            case TelnetOptions.Dont:
            case TelnetOptions.Will:
            case TelnetOptions.Wont:
                _optionCommand = (TelnetOptions)b;
                return State.IacOption;
            case TelnetOptions.Sb:
                return State.SubNegotiation;
            case TelnetOptions.EraseCharacter:
            case TelnetOptions.EraseLine:
            case TelnetOptions.Break:
            case TelnetOptions.InterruptProcess:
                RiseEscapeEvent();
                return State.Idle;
            default:
                return State.Idle;
        }
    }

    private State IacOption(byte option)
    {
        ReplyToTelnetOption(_optionCommand, option);
        return State.Idle;
    }

    private static State SubNegotiation(byte b)
    {
        return b == (byte)TelnetOptions.Iac ? State.SubNegotiationIac : State.SubNegotiation;
    }

    private static State SubNegotiationIac(byte b)
    {
        return b == (byte)TelnetOptions.Se ? State.Idle : State.SubNegotiation;
    }

    private State Esc(byte b)
    {
        switch (b)
        {
            case (byte)'[':
                _vtSequence.Clear();
                return State.Csi;
            case (byte)'O':
                return State.Ss3;
            case Escape:
                RiseEscapeEvent();
                return State.Esc;
            default:
                RiseEscapeEvent();
                return IdleMode(b);
        }
    }

    private State Csi(byte b)
    {
        if (b == (byte)'[' && _vtSequence.Length == 0)
        {
            return State.CsiBracket;
        }

        if (IsCsiParameterByte(b))
        {
            _vtSequence.Append((char)b);
            return State.Csi;
        }

        if (IsCsiFinalByte(b))
        {
            ProcessCsiFinal(b);
            _vtSequence.Clear();
            return State.Idle;
        }

        _vtSequence.Clear();
        return State.Idle;
    }

    private State CsiBracket(byte b)
    {
        if (b == (byte)'A')
        {
            RiseFunctionEvent();
        }

        return State.Idle;
    }

    private State Ss3(byte b)
    {
        switch (b)
        {
            case (byte)'A':
                RiseUpArrowEvent();
                break;
            case (byte)'B':
                RiseDownArrowEvent();
                break;
            case (byte)'C':
                RiseRightArrowEvent();
                break;
            case (byte)'D':
                RiseLeftArrowEvent();
                break;
            case (byte)'P':
                RiseFunctionEvent();
                break;
        }

        return State.Idle;
    }

    private void ProcessCsiFinal(byte b)
    {
        switch (b)
        {
            case (byte)'A':
                RiseUpArrowEvent();
                break;
            case (byte)'B':
                RiseDownArrowEvent();
                break;
            case (byte)'C':
                RiseRightArrowEvent();
                break;
            case (byte)'D':
                RiseLeftArrowEvent();
                break;
            case (byte)'~':
                ProcessCsiTilde(_vtSequence.ToString());
                break;
        }
    }

    private void ProcessCsiTilde(string sequence)
    {
        var keyCode = sequence.Split(';')[0];
        switch (keyCode)
        {
            case "3":
                RiseEscapeEvent();
                break;
            case "11":
                RiseFunctionEvent();
                break;
        }
    }

    private void RequestCharacterMode()
    {
        if (_characterModeRequested) return;
        _characterModeRequested = true;

        _ = SendTelnetCommand(TelnetOptions.Will, TelnetEcho);
        _ = SendTelnetCommand(TelnetOptions.Will, TelnetSuppressGoAhead);
        _ = SendTelnetCommand(TelnetOptions.Do, TelnetSuppressGoAhead);
        _ = SendTelnetCommand(TelnetOptions.Dont, TelnetLineMode);
    }

    private void ReplyToTelnetOption(TelnetOptions command, byte option)
    {
        switch (command)
        {
            case TelnetOptions.Do:
                _ = SendTelnetCommand(option is TelnetEcho or TelnetSuppressGoAhead
                    ? TelnetOptions.Will
                    : TelnetOptions.Wont, option);
                break;
            case TelnetOptions.Will:
                _ = SendTelnetCommand(option == TelnetSuppressGoAhead
                    ? TelnetOptions.Do
                    : TelnetOptions.Dont, option);
                break;
            case TelnetOptions.Dont:
            case TelnetOptions.Wont:
                break;
        }
    }

    private async Task SendTelnetCommand(TelnetOptions command, byte option)
    {
        try
        {
            byte[] data = [(byte)TelnetOptions.Iac, (byte)command, option];
            await _port.Send(data, data.Length, default).ConfigureAwait(false);
        }
        catch
        {
            // Negotiation is best effort; input parsing must survive disconnected TCP clients.
        }
    }

    private static bool IsAsciiDigit(byte b)
    {
        return b is >= (byte)'0' and <= (byte)'9';
    }

    private static bool IsCsiParameterByte(byte b)
    {
        return b is >= 0x30 and <= 0x3F;
    }

    private static bool IsCsiFinalByte(byte b)
    {
        return b is >= 0x40 and <= 0x7E;
    }
}

 

public class DataStreamKeyBoard:KeyboardBase
{
    public DataStreamKeyBoard(string cs)
    {
        var port = PortFactory.Create(cs);
        port.Enable();
        port.OnReceive.Subscribe(OnData).DisposeItWith(Disposable);
    }
    
    public DataStreamKeyBoard(IDataStream port)
    {
        port.OnReceive.Subscribe(OnData).DisposeItWith(Disposable);
    }
    
    private void OnData(byte[] data)
    {
        foreach (var c in Encoding.ASCII.GetChars(data))
        {
            if (char.IsDigit(c))
            {
                RiseDigitEvent(c);
                continue;
            }

            if (!char.IsLetter(c)) continue;
            switch (c)
            {
                case 'a':
                    RiseDotEvent();
                    break;
                case 'b':
                    RiseDownArrowEvent();
                    break;
                case 'c':
                    RiseEnterEvent();
                    break;
                case 'd':
                    RiseEscapeEvent();
                    break;
                case 'e':
                    RiseFunctionEvent();
                    break;
                case 'f':
                    RiseLeftArrowEvent();
                    break;
                case 'g':
                    RiseRightArrowEvent();
                    break;
                case 'h':
                    RiseUpArrowEvent();
                    break;
            }
        }
    }
}
