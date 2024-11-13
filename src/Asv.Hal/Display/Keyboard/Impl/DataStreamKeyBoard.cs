using System.Diagnostics;
using System.Reactive.Subjects;
using System.Text;
using Asv.Common;
using Asv.IO;

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
    private readonly IDataStream _port;
    private readonly StringBuilder _vtSequence = new();

    enum State
    {
        Idle,
        Iac,
        IacDo,
        Esc,
        IacWill,
        EscBracket,
        EndLine,
        EscBracketVtSequence
    }
    private State _state;
    

    public TelnetKeyboard(IDataStream port)
    {
        _port = port;
        port.Subscribe(OnData).DisposeItWith(Disposable);
    }

    private void OnData(byte[] data)
    {
        foreach (var b in Encoding.ASCII.GetChars(data))
        {
            switch (_state)
            {
                case State.Idle:
                    _state = IdleMode(b);
                    break;
                case State.EndLine:
                    _state = EndLine(b);
                    break;
                case State.Iac:
                    _state = Iac(b);
                    break;
                case State.IacDo:
                    _state = IacDo(b);
                    return;
                case State.IacWill:
                    _state = IacWill(b);
                    return;
                case State.Esc:
                    _state = Esc(b);
                    break;
                case State.EscBracket:
                    _state = EscBracket(b);
                    break;
                case State.EscBracketVtSequence:
                    _state = EscBracketVtSequence(b);
                    return;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    private State EndLine(char b)
    {
        if (b == 0x0D)
        {
            RiseEnterEvent();
        }
        return State.Idle;
    }

    private State EscBracket(char b)
    {
        switch (b)
        {
            case (char)0x41:
                RiseUpArrowEvent();
                break;
            case (char)0x42:
                RiseDownArrowEvent();
                break;
            case (char)0x43:
                RiseRightArrowEvent();
                break;
            case (char)0x44:
                RiseLeftArrowEvent();
                break;
        }

        if (char.IsDigit((char)b))
        {
            return EscBracketVtSequence(b);
        }
        return State.Idle;
    }

    private State EscBracketVtSequence(char b)
    {
        var c = (char)b;
        if (c == 0x7E)
        {
            switch (_vtSequence.ToString())
            {
                case "11":
                    RiseFunctionEvent();
                    break;
            }
            _vtSequence.Clear();
            return State.Idle;
        }
        if (char.IsDigit((char)b))
        {
            _vtSequence.Append(c);
            return State.EscBracketVtSequence;
        }
        return State.Idle;
    }

    private State IacWill(char b)
    {   
        //Debug.WriteLine($"IAC WILL {b:X}");
        
        _port.Send([0xFF,0xFB,0x01], 3, default).Wait(); // [IAC WILL ECHO] Suppress Echo
        _port.Send([0xFF,0xFD,0x03], 3, default).Wait(); // [IAC DO SUPPRESS-GOAHEAD] Character Mode / Line Mode
        _port.Send([0xFF,0xFB,0x03], 3, default).Wait(); // [IAC DO SUPPRESS-GOAHEAD] Character Mode / Line Mode
        return State.Idle;
    }

    private static State IacDo(char b)
    {
        //Debug.WriteLine($"IAC DO {b:X}");
        return State.Idle;
    }

    private static State Esc(char b)
    {
        return b switch
        {
            (char)0x5B => State.EscBracket,
            _ => State.Idle
        };
    }

    private static State Iac(char b)
    {
        return b switch
        {
            (char)0xFD => State.IacDo,
            (char)0xFB => State.IacWill,
            _ => State.Idle
        };
    }

    private State IdleMode(char b)
    {
        if (char.IsDigit(b))
        {
            RiseDigitEvent(b);
            return State.Idle;
        }
        return State.Idle;
        /*switch (b)
        {
            case 0x7F :
                RiseEscapeEvent();
                return State.Idle;
            case 0xFF:
                return State.Iac;
            case 0x1B:
                return State.Esc;
            case 0x0A:
                return State.EndLine;
            default:
                return State.Idle;
        }*/
    }
}

 

public class DataStreamKeyBoard:KeyboardBase
{
    public DataStreamKeyBoard(string cs)
    {
        var port = PortFactory.Create(cs);
        port.Enable();
        port.Subscribe(OnData).DisposeItWith(Disposable);
    }
    
    public DataStreamKeyBoard(IDataStream port)
    {
        port.Subscribe(OnData).DisposeItWith(Disposable);
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