using Asv.Common;

namespace Asv.Hal;

public class TextBox:Control
{
    private bool _isCaretVisible;
    private TimeSpan _blinkTime = TimeSpan.FromMilliseconds(500);
    private long _lastBlink;
    private string? _lastValue;
    private string? _text;

    public TextBox(string? header = null, string? units = null)
    {
        Events.Catch<ValueChangedEvent>(OnValueChangedEvent).DisposeItWith(Disposable);
        Events.Catch<KeyDownEvent>(OnKeyDownEvent).DisposeItWith(Disposable);
        Events.Catch<AnimationTickEvent>(OnAnimationTickEvent).DisposeItWith(Disposable);

        if (header != null)
        {
            AddVisualChild(Header = new TextBlock{Text = header});
        }
        else
        {
            AddVisualChild(Header = new TextBlock());
        }
        if (units != null)
        {
            AddVisualChild(Units = new TextBlock{Text = units});
        }
        else
        {
            AddVisualChild(Units = new TextBlock());
        }
    }
   
    public override int Width => Header.Width + Units.Width + (Text?.Length ?? 0);
    public override int Height => 1;
    public TextBlock Header { get; }
    public TextBlock Units { get; }

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

    public char Cursor { get; set; } = '_';

    protected override void OnGotFocus()
    {
        _lastValue = Text;
        Text = "";
        _isCaretVisible = true;
        RiseRenderRequestEvent();
    }

    protected override void OnLostFocus()
    {
        _isCaretVisible = false;
        RiseRenderRequestEvent();
    }

    public TimeSpan BlinkTime
    {
        get => _blinkTime;
        set
        {
            if (_blinkTime == value) return;
            _blinkTime = value;
            RiseRenderRequestEvent();
        }
    }

    public override void Render(IRenderContext ctx)
    {
        if (IsFocused)
        {
            var valueWidth = Text?.Length ?? 0;
            Header.Render(ctx.Crop(0,0,ctx.Width - valueWidth,1));
            ctx.WriteString(ctx.Width - valueWidth,0,Text);
            if (_isCaretVisible)
            {
                ctx.Write(ctx.Width - 1,0,Cursor);
            }
        }
        else
        {
            var valueWidth = (Text?.Length??0) + Units.Width;
            Header.Render(ctx.Crop(0,0,ctx.Width - valueWidth,1));
            ctx.WriteString(ctx.Width - valueWidth,0,Text);
            Units.Render(ctx.Crop(ctx.Width-Units.Width,0,Units.Width,1));
        }
    }

    private void OnValueChangedEvent(ValueChangedEvent e)
    {
        if (e.Target == this)
        {
            Text = e.Text;
        }
    }

    private void OnKeyDownEvent(KeyDownEvent e)
    {
        if (IsFocused == false) return;
        switch (e.Key.Type)
        {
            case KeyType.Enter:
                IsFocused = false;
                Events.Rise(new ValueEditedEvent(this, Text));
                break;
            case KeyType.Digit or KeyType.Dot:
                Text += e.Key.Value.ToString();
                RiseRenderRequestEvent();
                break;
            case KeyType.LeftArrow:
                if (Text?.Length > 0) Text = Text[..^1];
                break;
            case KeyType.Escape:
                Text = _lastValue;
                IsFocused = false;
                Events.Rise(new ValueEditedEvent(this, Text));
                break;
            case KeyType.Function:
                if (Text?.Length > 0)
                {
                    Text = Text[0] == '-' ? Text.Substring(1, Text.Length - 1) : $"-{Text}";
                }
                else
                {
                    Text = "-";
                }
                break;
        }
        e.IsHandled = true;
    }

    private void OnAnimationTickEvent(AnimationTickEvent e)
    {
        if (IsFocused)
        {
            if (e.TimeProvider.GetElapsedTime(_lastBlink) > BlinkTime)
            {
                _isCaretVisible = !_isCaretVisible;
                _lastBlink = e.TimeProvider.GetTimestamp();
                RiseRenderRequestEvent();
            }
        }
    }
}
