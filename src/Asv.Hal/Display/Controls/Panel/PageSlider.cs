using Asv.Common;

namespace Asv.Hal;

public class PageSlider:Panel
{
    private int _pageIndex;
    private bool _isEditingProcess;

    public PageSlider()
    {
        Events.Catch<ValueEditingProcessEvent>(OnValueEditingProcessEvent).DisposeItWith(Disposable);
        Events.Catch<ValueEditedEvent>(OnValueEditedEvent).DisposeItWith(Disposable);
        Events.Catch<EnumValueEditedEvent>(OnEnumValueEditedEvent).DisposeItWith(Disposable);
        Events.Catch<AttachEvent>(OnAttachEvent).DisposeItWith(Disposable);
        Events.Catch<KeyDownEvent>(OnKeyDownEvent).DisposeItWith(Disposable);
    }

    public int PageIndex
    {
        get => _pageIndex;
        set
        {
            if (_pageIndex == value) return;
            if (value <= 0) value = 0;
            var cnt = Items.Count;
            if (value >= cnt) value = cnt - 1;
            if (SelectedPage != null) SelectedPage.IsFocused = false;
            _pageIndex = value;
            if (SelectedPage != null) SelectedPage.IsFocused = true;
            RiseRenderRequestEvent();
        }
    }
    
    public Control? SelectedPage => Items[_pageIndex];

    public override int Width => SelectedPage?.Width ?? 0;
    public override int Height => SelectedPage?.Height ?? 0;

    
    public override void Render(IRenderContext context)
    {
        SelectedPage?.Render(context);
    }

    private void OnValueEditingProcessEvent(ValueEditingProcessEvent e)
    {
        _isEditingProcess = true;
    }

    private void OnValueEditedEvent(ValueEditedEvent e)
    {
        _isEditingProcess = false;
    }

    private void OnEnumValueEditedEvent(EnumValueEditedEvent e)
    {
        _isEditingProcess = false;
    }

    private void OnAttachEvent(AttachEvent e)
    {
        IsFocused = false;
        if (SelectedPage != null) SelectedPage.IsFocused = true;
    }

    private void OnKeyDownEvent(KeyDownEvent e)
    {
        if (IsFocused)
        {
            if (SelectedPage != null) SelectedPage.IsFocused = true;
        }
        if (e.Key.Type == KeyType.RightArrow && !_isEditingProcess)
        {
            PageIndex++;
            e.IsHandled = true;
        }

        if (e.Key.Type == KeyType.LeftArrow && !_isEditingProcess)
        {
            PageIndex--;
            e.IsHandled = true;
        }

        var copy = e.Clone();
        e.IsHandled = true;
        SelectedPage?.Events.Rise(copy);
    }
}
