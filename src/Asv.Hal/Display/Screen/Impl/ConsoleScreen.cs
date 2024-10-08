namespace Asv.Hal.Impl;

public class ConsoleScreen(Size size) : BufferScreen(size)
{
    private int _renderCount;

    protected override void InternalRender(char[,] buffer)
    {
        Console.Clear();
        Console.Write("╔");
        for (int i = 0; i < Size.Width; i++)
        {
            Console.Write("═");
        }
        Console.WriteLine("╗");
        for (var i = 0; i < Size.Height; i++)
        {
            Console.Write("║");
            for (var j = 0; j < Size.Width; j++)
            {
                Console.Write(buffer[j,i]);
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
    }
}