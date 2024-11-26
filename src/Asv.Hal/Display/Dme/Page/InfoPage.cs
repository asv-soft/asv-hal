namespace Asv.Hal;

public class InfoPage : GroupBox
{
    public InfoPage(string? header, HorizontalPosition headerAlign = HorizontalPosition.Left) : base(header, headerAlign)
    {
        
    }

    protected override void InternalRenderChildren(IRenderContext ctx)
    {
        var stringNum = 0;
        foreach (var item in Items)
        {
            item.Render(ctx.Crop(0, stringNum, new Size(ctx.Width, 1)));
            stringNum++;
        }
    }
}