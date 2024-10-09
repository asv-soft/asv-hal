using System.Reactive.Linq;
using Asv.Common;

namespace Asv.Hal;

public class Window: Control
{
    private int _tickInProgress;
    private int _renderRequested;
    private readonly TaskFactory _uiTaskFactory;
    private Control? _current;
    private readonly TimeProvider _timeProvider;
    private readonly IScreen _screen;


    public Window(string id, TimeProvider timeProvider, TimeSpan animationTick, IKeyboard keyboard,IScreen screen)
        :base(id)
    {
        var uiTaskScheduler = new SingleThreadTaskScheduler("UI Thread").DisposeItWith(Disposable);
        _uiTaskFactory = new TaskFactory(uiTaskScheduler);
        _timeProvider = timeProvider;
        _screen = screen;
        timeProvider.CreateTimer(Tick, null, animationTick, animationTick)
                .DisposeItWith(Disposable);
        keyboard.OnKeyPress.Subscribe(OnKeyDown).DisposeItWith(Disposable);
    }

    private void OnKeyDown(KeyValue keyValue)
    {
        _uiTaskFactory.StartNew(_ => OnRoutedEvent(new KeyDownEvent(this, keyValue)), null, DisposeCancel);
    }

    private async void Tick(object? state)
    {
        if (Interlocked.Exchange(ref _tickInProgress,1) != 0) return;
        try
        {
            await _uiTaskFactory.StartNew(_=>OnRoutedEvent(new AnimationTickEvent(this,_timeProvider)), null, DisposeCancel);
            if (Interlocked.CompareExchange(ref _renderRequested, 0, 1) != 0)
            {
                using var loop = _screen.BeginRenderLoop();
                await _uiTaskFactory.StartNew(_=>Render(_screen), null, DisposeCancel);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        finally
        {
            Interlocked.Exchange(ref _tickInProgress,0);
        }
    }
    
    public override Size Measure(Size availableSize) => availableSize;

    public override void Render(IRenderContext context)
    {
        Current?.Render(context);
    }

    public Control? Current
    {
        get => _current;
        set
        {
            if (_current == value) return;
            RemoveVisualChild(_current);
            _current = value;
            AddVisualChild(value);
            OnRoutedEvent(new RenderRequestEvent(this));
        }
    }

    protected override void InternalOnEvent(RoutedEvent e)
    {
        if (e is RenderRequestEvent)
        {
            e.IsHandled = true;
            Interlocked.Exchange(ref _renderRequested, 1);
        }
    }
}