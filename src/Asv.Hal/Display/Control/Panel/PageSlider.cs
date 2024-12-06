namespace Asv.Hal;

public class PageSlider:Panel
{
    private int _pageIndex;
    private bool _isEditingProcess;

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

    protected override void InternalOnEvent(RoutedEvent e)
    {
        switch (e)
        {
            case ValueEditingProcessEvent { Sender: TextBox }:
                _isEditingProcess = true;
                break;
            case ValueEditedEvent:
                _isEditingProcess = false;
                break;
            case AttachEvent:
            {
                IsFocused = false;
                if (SelectedPage != null) SelectedPage.IsFocused = true;
                break;
            }
            case KeyDownEvent key:
            {
                if (IsFocused)
                {
                    if (SelectedPage != null) SelectedPage.IsFocused = true;
                }
                if (key.Key.Type == KeyType.RightArrow)
                {
                    if (!_isEditingProcess)
                    {
                        PageIndex++;
                        e.IsHandled = true;
                    }
                }

                if (key.Key.Type == KeyType.LeftArrow)
                {
                    if (!_isEditingProcess)
                    {
                        PageIndex--;
                        e.IsHandled = true;
                    }
                }
            
                var copy = e.Clone();
                e.IsHandled = true;
                SelectedPage?.Event(copy);
                break;
            }
        }
    }
}