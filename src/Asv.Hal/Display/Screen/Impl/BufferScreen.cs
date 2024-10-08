namespace Asv.Hal;

public abstract class BufferScreen : IScreen
{
    
    
    private readonly char[,] _buffer;
    private readonly object _lock = new();

    protected BufferScreen(Size size)
    {
        Size = size;
        _buffer = new char[size.Width, size.Height];
        InternalClear();
    }
   

    private void InternalClear()
    {
        lock (_lock)
        {
            for (var i = 0; i < Size.Width; i++)
            {
                for (var j = 0; j < Size.Height; j++)
                {
                    _buffer[j, i] = ScreenHelper.Empty;
                }
            }
        }
    }

    public Size Size { get; }

    public void Write(int x, int y, char value)
    {
        if (x < 0 || x >= Size.Width || y < 0 || y >= Size.Height)
        {
            return;
        }
        lock (_lock)
        {
            _buffer[y, x] = value;
        }
    }

    public IDisposable BeginRenderLoop()
    {
        InternalClear();
        return System.Reactive.Disposables.Disposable.Create(EndRenderLoop);
    }

    private void EndRenderLoop()
    {
        lock (_lock)
        {
            InternalRender(_buffer);
        }
    }

    protected abstract void InternalRender(char[,] buffer);

}