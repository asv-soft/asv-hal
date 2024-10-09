using System.Reactive.Linq;

namespace Asv.Hal;

public class Button: Control
{
    private Control? _content;
    private char? _left = '[';
    private char? _right = ']';
    private readonly Action<ButtonClickEvent>? _onClick;

    public Button(string? text = null, Action<ButtonClickEvent>? onClick = null)
    {
        _onClick = onClick;
        if (text != null)
        {
            Content = new TextBlock(text);
        }
    }

    protected override void InternalOnEvent(RoutedEvent e)
    {
        if (e is KeyDownEvent { Key.Type: KeyType.Enter } && IsFocused)
        {
            var click = new ButtonClickEvent(this);
            _onClick?.Invoke(click);
            if (click.IsHandled == false) Event(click);
            e.IsHandled = true;
        }

        base.InternalOnEvent(e);
    }
    
    public Control? Content
    {
        get => _content;
        set
        {
            if (_content == value) return;
            RemoveVisualChild(_content);
            _content = value;
            AddVisualChild(_content);
            RiseRenderRequestEvent();
        }
    }

    public char? Left
    {
        get => _left;
        set
        {
            if (_left == value) return;
            _left = value;
            RiseRenderRequestEvent();
        }
    }

    public char? Right
    {
        get => _right;
        set
        {
            if (_right == value) return;
            _right = value;
            RiseRenderRequestEvent();
        }
    }

    public override Size Measure(Size availableSize)
    {
        var leftRightWidth = (_left.HasValue ? 1 : 0) + (_right.HasValue ? 1 : 0);
        var origin = Content?.Measure(new Size(availableSize.Width - leftRightWidth, 1));
        if (origin.HasValue)
        {
            return new Size(origin.Value.Width + leftRightWidth, origin.Value.Height);
        }

        return new Size(leftRightWidth, 0);


    }

    public override void Render(IRenderContext ctx)
    {
        if (Content == null) return;
        if (Left.HasValue) ctx.Write(0, 0, Left.Value);
        Content.Render(ctx.Crop(Left.HasValue ? 1 : 0, 0, ctx.Size.Width - (Right.HasValue ? 1 : 0), 1));
        if (Right.HasValue) ctx.Write(ctx.Size.Width - 1, 0, Right.Value);
    }
}