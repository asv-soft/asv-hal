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

    protected override void InternalOnEvent(RoutedEvent e)
    {
        if (e is ValueChangedEvent changed && changed.Target == this)
        {
            Text = changed.Text;
        }

        if (e is KeyDownEvent key)
        {
            if (IsFocused == false) return;
            switch (key.Key.Type)
            {
                case KeyType.Enter:
                    IsFocused = false;
                    InternalOnEvent(new ValueEditedEvent(this, Text));
                    Event(new ValueEditedEvent(this, Text));
                    break;
                case KeyType.Digit or KeyType.Dot:
                    Text += key.Key.Value.ToString();
                    RiseRenderRequestEvent();
                    break;
                case KeyType.Escape:
                    Text = _lastValue;
                    IsFocused = false;
                    break;
            }
            e.IsHandled = true;
        }
        
        if (e is AnimationTickEvent anim && IsFocused)
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