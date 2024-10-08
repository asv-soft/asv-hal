using Asv.Common;

namespace Asv.Hal;


public abstract class Control :DisposableOnceWithCancel
{
    // (Avoid zero-length array allocations.) The identity of these arrays matters, so we can't use the shared Array.Empty<T>() instance
    private static readonly Control[] Disposed = [];
    private Control[] _visualChildren = [];
    private bool _isVisible = true;

    protected Control(string id)
    {
        Id = id;
        Disposable.AddAction(()=>Volatile.Write(ref _visualChildren, Disposed));
    }

    public string Id { get;}
    
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

    #region Visual tree

    private Control? VisualParent { get; set; }
    private IReadOnlyList<Control> VisualChildren => Volatile.Read(ref _visualChildren);
    public void OnEvent(RoutedEvent e)
    {
        InternalOnEvent(e);
        if (e.IsHandled) return;
        if (e.Strategy == RoutingStrategy.Bubble && VisualParent != null)
        {
            VisualParent.OnEvent(e);
            return;
        }
        if (e.Strategy == RoutingStrategy.Tunnel && VisualChildren.Count > 0)
        {
            foreach (var child in VisualChildren)
            {
                child.OnEvent(e);
                if (e.IsHandled) return;
            }
        }
    }
    

    protected void AddVisualChild(Control? child)
    {
        if (child == null) return;
        var children = Volatile.Read(ref _visualChildren);
        child.VisualParent = this;
        var newChildren = new Control[children.Length + 1];
        children.CopyTo(newChildren, 0);
        newChildren[children.Length] = child;
        Volatile.Write(ref _visualChildren, newChildren);
    }
    protected void RemoveVisualChild(Control? child)
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
        
    }

    public abstract Size Measure(Size availableSize);
    public abstract void Render(IRenderContext context);
    protected void RiseRenderRequestEvent()
    {
        OnEvent(new RenderRequestEvent(this));
    }
}