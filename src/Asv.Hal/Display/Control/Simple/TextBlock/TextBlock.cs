namespace Asv.Hal;

public enum HorizontalPosition
{
    Left,
    Center,
    Right,
}

public class TextBlock(string? text = null, HorizontalPosition align = HorizontalPosition.Left ) : Control
{
    public static explicit operator TextBlock(string text) => new(text);
    public HorizontalPosition Align
    {
        get => align;
        set
        {
            if (align == value) return;
            align = value;
            RiseRenderRequestEvent();
        }
    }
    
    public string? Text
    {
        get => text;
        set
        {
            if (text == value) return;
            text = value;
            RiseRenderRequestEvent();
        }
    }
    
    public override Size Measure(Size availableSize)
    {
        return IsVisible == false 
            ? new Size(0,0) 
            : new Size(Text?.Length??0,1);
    }

    public override void Render(IRenderContext ctx)
    {
        if (IsVisible == false) return;
        if (string.IsNullOrWhiteSpace(text)) return;
        switch (align)
        {
            case HorizontalPosition.Left:
                ctx.WriteString(0,0,Text); 
                break;
            case HorizontalPosition.Center:
                ctx.WriteString((ctx.Size.Width - text.Length)/2,0,Text);
                break;
            case HorizontalPosition.Right:
                ctx.WriteString(ctx.Size.Width - text.Length,0,Text);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}