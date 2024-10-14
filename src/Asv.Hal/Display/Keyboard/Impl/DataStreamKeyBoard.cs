using System.Text;
using Asv.Common;
using Asv.IO;

namespace Asv.Hal;

public class TelnetKeyboard:KeyboardBase
{
    enum State
    {
        ESC,
        
    }
    private State _state;

    public TelnetKeyboard(IDataStream port)
    {
        port.Subscribe(OnData).DisposeItWith(Disposable);
    }

    private void OnData(byte[] data)
    {
        foreach (var b in data)
        {
            switch (_state)
            {
                 
            }
        }
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