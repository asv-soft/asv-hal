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
    }
}