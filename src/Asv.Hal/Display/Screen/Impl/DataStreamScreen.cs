using System.Text;
using Asv.IO;

namespace Asv.Hal.Impl;

public class DataStreamScreen(IPort port, Size size) : BufferScreen(size)
{
    protected override unsafe void InternalRender(char[,] buffer)
    {

        var length = size.Width * size.Height;
        var data = new byte[length];
        fixed (char* src = buffer)
        fixed (byte* dst = data)
        {
            Encoding.ASCII.GetBytes(src, length, dst, length);    
        }
            
        for (var i = 0; i < size.Width; i++)
        {
            for (var j = 0; j < size.Height; j++)
            {
                Console.Write(buffer[i, j]);
            }
        }
        port.Send([0x0A],1,default).Wait();
        port.Send(data,length,default).Wait();
        port.Send([0x0D],1,default).Wait();
    }
}