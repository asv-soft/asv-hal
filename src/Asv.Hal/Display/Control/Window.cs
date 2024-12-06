using System.Diagnostics;
using System.Globalization;
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
    private readonly SingleThreadTaskScheduler _uiTaskScheduler;
    private bool _isEditingProcess;
    private bool _isCalibrationProcess;


    public Window(TimeProvider timeProvider, TimeSpan animationTick, IKeyboard keyboard,IScreen screen, CultureInfo culture)
    {
        _uiTaskScheduler = new SingleThreadTaskScheduler("UI Thread").DisposeItWith(Disposable);
        _uiTaskScheduler.OnTaskUnhandledException += (sender, exception) =>
        {
            Console.WriteLine(exception);
        };
        _uiTaskFactory = new TaskFactory(_uiTaskScheduler);
        _timeProvider = timeProvider;
        _screen = screen;
        _focused = this;
        timeProvider.CreateTimer(Tick, null, animationTick, animationTick)
                .DisposeItWith(Disposable);

        _uiTaskFactory.StartNew(_ =>
        {
            _uiTaskScheduler.SchedulerThread.CurrentUICulture = culture;
            _uiTaskScheduler.SchedulerThread.CurrentCulture = culture;
        }, null, DisposeCancel);
        
        
        
        keyboard.OnKeyPress.Subscribe(OnKeyDown).DisposeItWith(Disposable);
    }

    public void SetCulture(CultureInfo culture)
    {
        _uiTaskFactory.StartNew(_ =>
        {
            _uiTaskScheduler.SchedulerThread.CurrentUICulture = culture;
            _uiTaskScheduler.SchedulerThread.CurrentCulture = culture;
        }, null, DisposeCancel);
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
            _current?.Event(new DetachEvent(this));
            RemoveVisualChild(_current);
            _current = value;
            AddVisualChild(value);
            if (_current != null)
            {
                _current.Event(new AttachEvent(this));
                _current.IsFocused = true;
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
        if (e is KeyDownEvent && _isCalibrationProcess)
        {
            e.IsHandled = true;
            return;
        }
        
        switch (e)
        {
            case KeyDownEvent { Key.Type: KeyType.Function }:
                if (!_isEditingProcess)
                {
                    _isCalibrationProcess = true;
                    _timeProvider.CreateTimer(_ => {
                        {
                            _isCalibrationProcess = false;
                            Event(new CalibrationStopEvent(this));
                        } }, 
                        null, TimeSpan.FromSeconds(3.5),
                        Timeout.InfiniteTimeSpan).DisposeItWith(Disposable);
                    e.IsHandled = true;
                    Event(new CalibrationStartEvent(this));
                }
                break;
            case KeyDownEvent { Key.Type: KeyType.Escape }:
                if (_isEditingProcess)
                {
                    var newEv = e.Clone();
                    e.IsHandled = true;
                    Current?.Event(newEv);
                }
                break;
            case ValueEditingProcessEvent { Sender: TextBox }:
                _isEditingProcess = true;
                break;
            case ValueEditedEvent:
                _isEditingProcess = false;
                break;
            case RenderRequestEvent:
                e.IsHandled = true;
                Interlocked.Exchange(ref _renderRequested, 1);
                break;
            case GotFocusEvent focus:
                Focused = focus.Sender;
                break;
        }
    }
}