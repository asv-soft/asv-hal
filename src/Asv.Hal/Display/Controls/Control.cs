using System.Globalization;
using Asv.Common;
using R3;

namespace Asv.Hal;


public abstract class Control:DisposableOnceWithCancel, ISupportRoutedEvents<Control>, ISupportParentChange<Control>
{
    // (Avoid zero-length array allocations.) The identity of these arrays matters, so we can't use the shared Array.Empty<T>() instance
    private static readonly Control[] Disposed = [];
    private Control[] _visualChildren = [];
    private readonly Subject<Control?> _parentChanged;

    public static implicit operator Control(string text) => new TextBlock(text);

    protected Control()
    {
        _parentChanged = new Subject<Control?>().DisposeItWith(Disposable);
        Events = new RoutedEventController<Control>(this).DisposeItWith(Disposable);
        Disposable.AddAction(()=>Volatile.Write(ref _visualChildren, Disposed));
        Events.Catch<DetachEvent>(OnDetachEvent).DisposeItWith(Disposable);
        Events.Catch<FocusUpdatedEvent>(OnFocusUpdatedEvent).DisposeItWith(Disposable);
        
    }

    private void OnFocusUpdatedEvent(FocusUpdatedEvent e)
    {
        if (e.NewFocus != this)
        {
            IsFocused = false;
        }
    }

    private void OnDetachEvent(DetachEvent obj)
    {
        IsFocused = false;
    }

    public object? Tag { get; set; }

    public bool IsVisible
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            RiseRenderRequestEvent();
        }
    } = true;

    #region Focus

    public bool IsFocused
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            if (field)
            {
                OnGotFocus();
                if (field == false) return;
                Events.Rise(new GotFocusEvent(this)).SafeFireAndForget();
            }
            else
            {
                OnLostFocus();
                if (field) return;
                Events.Rise(new LostFocusEvent(this)).SafeFireAndForget();
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

    public Control? Parent { get; set; }
    public IReadOnlyList<Control> VisualChildren => Volatile.Read(ref _visualChildren);
    public IRoutedEventController<Control> Events { get; }

    IEnumerable<Control> ISupportChildren<Control>.GetChildren() => VisualChildren;

    public ValueTask EventAsync(RoutedEvent e, CancellationToken cancel = default)
    {
        return this.Rise(e, cancel);
    }

    protected internal void AddVisualChild(Control? child)
    {
        if (child == null) return;
        var children = Volatile.Read(ref _visualChildren);
        child.Parent = this;
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
            children[i].Parent = null;
            var newChildren = new Control[children.Length - 1];
            Array.Copy(children, 0, newChildren, 0, i);
            Array.Copy(children, i + 1, newChildren, i, children.Length - i - 1);
            Volatile.Write(ref _visualChildren, newChildren);
            break;
        }
    }

    #endregion
    
    public abstract int Width { get; }
    public abstract int Height { get; }
    
    public abstract void Render(IRenderContext ctx);
    protected void RiseRenderRequestEvent()
    {
       Events.Rise(new RenderRequestEvent(this));
    }
    
    public override string ToString()
    {
        return GetType().Name;
    }

    public void SetParent(Control? parent)
    {
        if (ReferenceEquals(Parent, parent)) return;
        var oldParent = Parent;
        Parent = parent;
        _parentChanged.OnNext(oldParent);
    }

    public Observable<Control?> ParentChanged => _parentChanged;
}
