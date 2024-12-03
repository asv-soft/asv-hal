using Asv.Common;
using R3;

namespace Asv.Hal;


public abstract class Control:DisposableOnceWithCancel
{
    // (Avoid zero-length array allocations.) The identity of these arrays matters, so we can't use the shared Array.Empty<T>() instance
    private static readonly Control[] Disposed = [];
    private Control[] _visualChildren = [];
    private bool _isVisible = true;
    private Subject<RoutedEvent>? _onEvent;
    private bool _isFocused;

    public static implicit operator Control(string text) => new TextBlock(text);

    protected Control()
    {
        Disposable.AddAction(()=>Volatile.Write(ref _visualChildren, Disposed));
    }

    public object? Tag { get; set; }
    
    public bool IsVisible
    {
        get => _isVisible;
        set
        {
            if (_isVisible == value) return;
            _isVisible = value;
            RiseRenderRequestEvent();
        }
    }

    #region Focus

    public bool IsFocused
    {
        get => _isFocused;
        set
        {
            if (_isFocused == value) return;
            _isFocused = value;
            if (_isFocused)
            {
                OnGotFocus();
                Event(new GotFocusEvent(this));
            }
            else
            {
                OnLostFocus();
                Event(new LostFocusEvent(this));
            }
        }
    }

    protected virtual void OnGotFocus()
    {
        
    }
    
    protected virtual void OnLostFocus()
    {
        
    }

    #endregion
    
    #region Visual tree

    private Control? VisualParent { get; set; }
    public IReadOnlyList<Control> VisualChildren => Volatile.Read(ref _visualChildren);

    public void Event(RoutedEvent e)
    {
        InternalOnEvent(e);
        if (e.IsHandled) return;
        _onEvent?.OnNext(e);
        if (e.Strategy == RoutingStrategy.Bubble && VisualParent != null)
        {
            VisualParent.Event(e);
            return;
        }
        if (e.Strategy == RoutingStrategy.Tunnel && VisualChildren.Count > 0)
        {
            foreach (var child in VisualChildren)
            {
                child.Event(e);
                if (e.IsHandled) return;
            }
        }
        if (e is FocusUpdatedEvent focus && focus.NewFocus != this)
        {
            IsFocused = false;
        }
    }

    

    public Observable<RoutedEvent> OnEvent => _onEvent ??= new Subject<RoutedEvent>().DisposeItWith(Disposable);

    protected internal void AddVisualChild(Control? child)
    {
        if (child == null) return;
        var children = Volatile.Read(ref _visualChildren);
        child.VisualParent = this;
        var newChildren = new Control[children.Length + 1];
        children.CopyTo(newChildren, 0);
        newChildren[children.Length] = child;
        Volatile.Write(ref _visualChildren, newChildren);
    }
    protected internal void RemoveVisualChild(Control? child)
    {
        if (child == null) return;
        var children = Volatile.Read(ref _visualChildren);
        for (var i = 0; i < children.Length; i++)
        {
            if (!ReferenceEquals(children[i], child)) continue;
            children[i].VisualParent = null;
            var newChildren = new Control[children.Length - 1];
            Array.Copy(children, 0, newChildren, 0, i);
            Array.Copy(children, i + 1, newChildren, i, children.Length - i - 1);
            Volatile.Write(ref _visualChildren, newChildren);
            break;
        }
    }

    #endregion
    
    protected virtual void InternalOnEvent(RoutedEvent e)
    {
        if (e is DetachEvent det)
        {
            IsFocused = false;
        }
    }

    public abstract int Width { get; }
    public abstract int Height { get; }
    
    public abstract void Render(IRenderContext ctx);
    protected void RiseRenderRequestEvent()
    {
        Event(new RenderRequestEvent(this));
    }
    
    public override string ToString()
    {
        return GetType().Name;
    }
}