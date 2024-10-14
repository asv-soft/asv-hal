using System.Text;
using Asv.IO;

namespace Asv.Hal.Impl;

public class TelnetScreen : BufferScreen
{
    private readonly IPort _port;

    public TelnetScreen(IPort port, Size size) : base(size)
    {
        _port = port;
    }

    protected override unsafe void InternalRender(char[,] buffer)
    {
        
        
        var length = Size.Width * Size.Height;
        var data = new byte[length];
        fixed (char* src = buffer)
        fixed (byte* dst = data)
        {
            Encoding.ASCII.GetBytes(src, length, dst, length);    
        }
        
        _port.Send([0xFF,0xFB,0x01], 3, default).Wait(); // [IAC WILL ECHO] Suppress Echo
        _port.Send([0xFF,0xFD,0x03], 3, default).Wait(); // [IAC DO SUPPRESS-GOAHEAD] Character Mode / Line Mode
        _port.Send([0xFF,0xFB,0x03], 3, default).Wait(); // [IAC DO SUPPRESS-GOAHEAD] Character Mode / Line Mode
        
        _port.Send([27,99], 2, default).Wait(); // clr screen
        for (var y = 0; y < Size.Height; y++)
        {
            var mem = new ReadOnlyMemory<byte>(data, y * Size.Width, Size.Width);
            _port.Send(mem,default).Wait();
            _port.Send([10,13], 2 ,default).Wait();
        }
    }
}