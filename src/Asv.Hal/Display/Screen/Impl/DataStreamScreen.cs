using System.Text;
using Asv.IO;

namespace Asv.Hal.Impl;

public class DataStreamScreen : BufferScreen
{
    private readonly IPort _port;

    public DataStreamScreen(IPort port, Size size) : base(size)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        _port = port;
    }

    protected override unsafe void InternalRender(char[,] buffer)
    {

        var length = Size.Width * Size.Height;
        var data = new byte[length];
        fixed (char* src = buffer)
        fixed (byte* dst = data)
        {
            // Encoding.ASCII.GetBytes(src, length, dst, length);    
            Encoding.GetEncoding("windows-1251").GetBytes(src, length, dst, length);    
        }
            
        _port.Send([0x0A],1,default).Wait();
        _port.Send(data,length,default).Wait();
        _port.Send([0x0D],1,default).Wait();
    }
}