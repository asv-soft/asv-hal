namespace Asv.Hal;

public abstract class GroupBox:Panel
{
    private Control? _header;
    protected GroupBox(string? header, HorizontalPosition headerAlign = HorizontalPosition.Left)
    {
        if (header != null)
        {
            Header = new TextBlock(header, headerAlign);
        }
    }
    public Control? Header
    {
        get => _header;
        set
        {
            if (_header == value) return;
            RemoveVisualChild(_header);
            _header = value;
            AddVisualChild(_header);
            RiseRenderRequestEvent();
        } 
    }
    public override int Height => Header is { IsVisible: true } ? Items.Sum(x => x.Height) + Header.Height : Items.Sum(x => x.Height);
    public override int Width => Items.Where(x=>x.IsVisible).Max(x => x.Width);
    public override void Render(IRenderContext ctx)
    {
        if (Header is { IsVisible: true })
        {
            Header.Render(ctx.Crop(0,0,ctx.Size.Width,Header.Height));
            ctx = ctx.Crop(0,Header.Height,ctx.Size.Width,ctx.Size.Height-Header.Height);
        }
        InternalRenderChildren(ctx);
    }
    protected abstract void InternalRenderChildren(IRenderContext ctx);
}