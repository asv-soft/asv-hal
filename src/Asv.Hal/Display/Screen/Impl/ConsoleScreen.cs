using System.Collections.Concurrent;
using System.Text;

namespace Asv.Hal.Impl;

public class ConsoleScreen(Size size) : BufferScreen(size)
{
    private int _renderCount;
    private ConcurrentDictionary<string, string> _debug = new();
    private StringBuilder _sb = new();

    protected override void InternalRender(char[,] buffer)
    {
        Console.Clear();
        Console.Write("╔");
        for (int i = 0; i < Size.Width; i++)
        {
            Console.Write("═");
        }
        Console.WriteLine("╗");
        for (var x = 0; x < Size.Height; x++)
        {
            Console.Write("║");
            for (var y = 0; y < Size.Width; y++)
            {
                Console.Write(buffer[x,y]);
            }
            Console.Write("║");
            Console.WriteLine();
        }  
        Console.Write("╚");
        for (int i = 0; i < Size.Width; i++)
        {
            Console.Write("═");
        }
        Console.WriteLine("╝");
        Console.Write($"Rendered: {_renderCount++}");
        Console.WriteLine();
        foreach (var item in _debug)
        {
            Console.WriteLine($"{item.Key,-20} = {item.Value,-20}");
        }
        Console.Write(_sb);
        _sb.Clear();

    }

    public override void Debug(string key, string value)
    {
        _debug.AddOrUpdate(key, value,(k,v)=>value);
    }

    public override void DebugWrite(string message)
    {
        _sb.Append(message);
    }

    public override void DebugWriteLine(string message)
    {
        _sb.AppendLine(message);
    }
}