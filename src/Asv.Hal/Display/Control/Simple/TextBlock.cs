namespace Asv.Hal;

public enum HorizontalPosition
{
    Left,
    Center,
    Right,
}

public class TextBlock(string id) : Control(id)
{
    private HorizontalPosition _align;
    private string? _text;

    public HorizontalPosition Align
    {
        get => _align;
        set
        {
            if (_align == value) return;
            _align = value;
            RiseRenderRequestEvent();
        }
    }
    
    public string? Text
    {
        get => _text;
        set
        {
            if (_text == value) return;
            _text = value;
            RiseRenderRequestEvent();
        }
    }
    
    public override Size Measure(Size availableSize)
    {
        return IsVisible == false 
            ? new Size(0,0) 
            : new Size(Text?.Length??0,1);
    }

    public override void Render(IRenderContext context)
    {
        if (IsVisible == false) return;
        if (string.IsNullOrWhiteSpace(_text)) return;
        switch (_align)
        {
            case HorizontalPosition.Left:
                context.WriteString(0,0,Text); 
                break;
            case HorizontalPosition.Center:
                context.WriteString((context.Size.Width - _text.Length)/2,0,Text);
                break;
            case HorizontalPosition.Right:
                context.WriteString(context.Size.Width - _text.Length,0,Text);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}