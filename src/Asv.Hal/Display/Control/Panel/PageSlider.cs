namespace Asv.Hal;

public class PageSlider:Panel
{
    private int _pageIndex;

    public int PageIndex
    {
        get => _pageIndex;
        set
        {
            if (_pageIndex == value) return;
            if (value <= 0) value = 0;
            var cnt = Items.Count;
            if (value >= cnt) value = cnt - 1;
            _pageIndex = value;
            RiseRenderRequestEvent();
        }
    }
    
    public Control? SelectedPage => Items[_pageIndex];

    public override Size Measure(Size availableSize)
    {
        return SelectedPage?.Measure(availableSize) ?? availableSize;
    }

    public override void Render(IRenderContext context)
    {
        SelectedPage?.Render(context);
    }

    protected override void InternalOnEvent(RoutedEvent e)
    {
        if (e is KeyDownEvent key)
        {
            if (key.Key.Type == KeyType.RightArrow)
            {
                PageIndex++;
            }

            if (key.Key.Type == KeyType.LeftArrow)
            {
                PageIndex--;
            }
        }
    }
}