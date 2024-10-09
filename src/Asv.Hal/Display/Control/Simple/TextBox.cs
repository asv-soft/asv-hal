namespace Asv.Hal;

public class TextBox:Control
{
    private bool _isInEditMode;
    private bool _isCaretVisible;
    private TimeSpan _blinkTime = TimeSpan.FromMilliseconds(500);
    private long _lastBlink;
    private string _lastValue;
    private string? _text;

    public TextBox(string? header = null, string? units = null)
    {
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

    public bool IsInEditMode
    {
        get => _isInEditMode;
        set
        {
            if (_isInEditMode == value) return;
            _isInEditMode = value;
            if (_isInEditMode)
            {
                _lastValue = Text;
                Text = "";
                _isCaretVisible = true;
            }
            if (_isInEditMode == false)
            {
                _isCaretVisible = false;
            }
            RiseRenderRequestEvent();
        }
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

    public override Size Measure(Size availableSize)
    {
        return new Size(availableSize.Width, 1);
    }

    public override void Render(IRenderContext ctx)
    {
        
        if (IsInEditMode)
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
            var unitSize = Units.Measure(new Size(ctx.Width,1));
            var valueWidth = Text?.Length??0 + unitSize.Width;
            Header.Render(ctx.Crop(0,0,ctx.Width - valueWidth,1));
            ctx.WriteString(ctx.Width - valueWidth,0,Text);
            Units.Render(ctx.Crop(ctx.Width-unitSize.Width,0,unitSize.Width,1));
        }
        
    }

    protected override void InternalOnEvent(RoutedEvent e)
    {
        if (e is ValueChangedEvent changed && changed.Target == this)
        {
            Text = changed.Text;
        }
        if (IsInEditMode)
        {
            if (e is KeyDownEvent key)
            {
                switch (key.Key.Type)
                {
                    case KeyType.Enter:
                        IsInEditMode = false;
                        InternalOnEvent(new ValueEditedEvent(this, Text));
                        break;
                    case KeyType.Digit or KeyType.Dot:
                        Text += key.Key.Value.ToString();
                        RiseRenderRequestEvent();
                        break;
                    case KeyType.Escape:
                        Text = _lastValue;
                        IsInEditMode = false;
                        break;
                }
                e.IsHandled = true;
            }
        }
        else
        {
            if (e is KeyDownEvent { Key.Type: KeyType.Enter })
            {
                IsInEditMode = true;
                e.IsHandled = true;
            }
        }
        
        if (e is AnimationTickEvent anim)
        {
            if (_isInEditMode)
            {
                if (anim.TimeProvider.GetElapsedTime(_lastBlink) > BlinkTime)
                {
                    _isCaretVisible = !_isCaretVisible;
                    _lastBlink = anim.TimeProvider.GetTimestamp();
                    RiseRenderRequestEvent();
                }
            }
        }
    }
}