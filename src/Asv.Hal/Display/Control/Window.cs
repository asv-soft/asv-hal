using System.Diagnostics;
using Asv.Common;
using R3;

namespace Asv.Hal;

public class Window: Control
{
    private int _tickInProgress;
    private int _renderRequested;
    private readonly TaskFactory _uiTaskFactory;
    private Control? _current;
    private readonly TimeProvider _timeProvider;
    private readonly IScreen _screen;
    private Control _focused;


    public Window(TimeProvider timeProvider, TimeSpan animationTick, IKeyboard keyboard,IScreen screen)
    {
        var uiTaskScheduler = new SingleThreadTaskScheduler("UI Thread").DisposeItWith(Disposable);
        uiTaskScheduler.OnTaskUnhandledException += (sender, exception) =>
        {
            Console.WriteLine(exception);
        };
        _uiTaskFactory = new TaskFactory(uiTaskScheduler);
        _timeProvider = timeProvider;
        _screen = screen;
        _focused = this;
        timeProvider.CreateTimer(Tick, null, animationTick, animationTick)
                .DisposeItWith(Disposable);
        keyboard.OnKeyPress.Subscribe(OnKeyDown).DisposeItWith(Disposable);
    }

    private void OnKeyDown(KeyValue keyValue)
    {
        _uiTaskFactory.StartNew(_ => Event(new KeyDownEvent(this, keyValue)), null, DisposeCancel);
    }

    private async void Tick(object? state)
    {
        if (Interlocked.Exchange(ref _tickInProgress,1) != 0) return;
        try
        {
            await _uiTaskFactory.StartNew(_=>Event(new AnimationTickEvent(this,_timeProvider)), null, DisposeCancel);
            if (Interlocked.CompareExchange(ref _renderRequested, 0, 1) != 0)
            {
                //DebugRenderTree(this,0);
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

    private void DebugRenderTree(Control? control, int level)
    {
        if (control == null) return;
        _screen.DebugWrite("|");
        for (int i = 0; i < level; i++)
        {
            _screen.DebugWrite("-");    
        }
        _screen.DebugWriteLine($"{control} {(control.IsFocused? "<=" : String.Empty)}");
        foreach (var child in control.VisualChildren)
        {
            DebugRenderTree(child, level +1);
        }
        
    }

    public override int Height => Current?.Height ?? 0;
    public override int Width => Current?.Width ?? 0;

    public override void Render(IRenderContext ctx)
    {
        Current?.Render(ctx);
    }

    public Control? Current
    {
        get => _current;
        private set
        {
            if (_current == value) return;
            if (_current != null)
            {
                _current.Event(new DetachEvent(this));
            }
            RemoveVisualChild(_current);
            _current = value;
            AddVisualChild(value);
            if (_current != null)
            {
                _current.IsFocused = true;
                _current.Event(new AttachEvent(this));
            }
            Event(new RenderRequestEvent(this));
        }
    }

    public Control Focused
    {
        get => _focused;
        private set
        {
            if (_focused == value) return;
            var old = _focused;
            _focused = value;
            _screen.Debug("FOCUS",$"{old}=>{_focused}");
            Debug.WriteLine($"==>FOCUS:{old}=>{_focused}");
            _uiTaskFactory.StartNew(() =>
                Event(new FocusUpdatedEvent(this, old, _focused)));
        }
    }

    public void GoTo(Control? control)
    {
        _uiTaskFactory.StartNew(() => Current = control);
    }
    
    public TaskFactory UiTaskFactory => _uiTaskFactory;

    protected override void InternalOnEvent(RoutedEvent e)
    {
        if (e is RenderRequestEvent)
        {
            e.IsHandled = true;
            Interlocked.Exchange(ref _renderRequested, 1);
        }

        if (e is GotFocusEvent focus)
        {
            Focused = focus.Sender;
        }
    }

    
}