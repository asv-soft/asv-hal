namespace Asv.Hal;

public enum HorizontalPosition
{
    Left,
    Center,
    Right,
}

public class TextBlock(string? text = null, HorizontalPosition align = HorizontalPosition.Left ) : Control
{
    private char _background = ScreenHelper.Empty;
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

    public char Background
    {
        get => _background;
        set
        {
            if (_background == value) return;
            _background = value;
            RiseRenderRequestEvent();
        }
    }

    public override int Height => 1;
    public override int Width => text?.Length ?? 0;

    public override void Render(IRenderContext ctx)
    {
        if (IsVisible == false) return;
        if (Text == null)
        {
            ctx.FillChar(0,0,ctx.Size.Width,_background);
            return;
        }
        switch (align)
        {
            case HorizontalPosition.Left:
                ctx.WriteString(0,0,Text);
                ctx.FillChar(Text.Length,0,ctx.Size.Width - Text.Length,_background);
                break;
            case HorizontalPosition.Center:
                var startX = (ctx.Size.Width - Text.Length) / 2;
                ctx.FillChar(0,0,startX,_background);
                ctx.WriteString(startX,0,Text);
                ctx.FillChar(startX + Text.Length,0,ctx.Size.Width - startX - Text.Length,_background);
                break;
            case HorizontalPosition.Right:
                ctx.FillChar(0,0,ctx.Size.Width - Text.Length,_background);
                ctx.WriteString(ctx.Size.Width - Text.Length,0,Text);
                ctx.FillChar(ctx.Size.Width,0,Text.Length,_background);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public override string ToString()
    {
        return $"TextBlock[{Text}]";
    }
}