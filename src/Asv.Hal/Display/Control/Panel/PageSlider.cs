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

    public override int Width => SelectedPage?.Width ?? 0;
    public override int Height => SelectedPage?.Height ?? 0;

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
                e.IsHandled = true;
            }

            if (key.Key.Type == KeyType.LeftArrow)
            {
                PageIndex--;
                e.IsHandled = true;
            }
        }
    }
}