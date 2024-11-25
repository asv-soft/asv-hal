using System.Reactive.Disposables;

namespace Asv.Hal;

public class CompositeScreen : IScreen
{
    public IScreen[] Screens { get; }

    public CompositeScreen(params IScreen[] screens)
    {
        //check all screens same size
        Size = screens[0].Size;
        if (screens.Any(screen => screen.Size != Size))
        {
            throw new ArgumentException("All screens must have same size");
        }
        Screens = screens;
    }
    public Size Size { get; }
    public void Write(int x, int y, char value)
    {
        foreach (var screen in Screens)
        {
            screen.Write(x, y, value);
        }
    }

    public IDisposable BeginRenderLoop()
    {
        return new CompositeDisposable(Screens.Select(screen => screen.BeginRenderLoop()));
    }

    public void Debug(string key, string value)
    {
        foreach (var screen in Screens)
        {
            screen.Debug(key, value);
        }
    }

    public void DebugWrite(string message)
    {
        foreach (var screen in Screens)
        {
            screen.DebugWrite(message);
        }
    }

    public void DebugWriteLine(string message)
    {
        foreach (var screen in Screens)
        {
            screen.DebugWriteLine(message);
        }
    }
}