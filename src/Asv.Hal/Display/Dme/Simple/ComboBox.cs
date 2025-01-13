namespace Asv.Hal;

public class ComboBox<TValue> : Control where TValue : struct, Enum 
{
    private bool _isCaretVisible;
    private TimeSpan _blinkTime = TimeSpan.FromMilliseconds(500);
    private long _lastBlink;
    private TValue _value;
    private readonly IDictionary<TValue, int> _availableIndexesFromValue;
    private readonly IDictionary<TValue, string> _availableTitlesFromValue;
    private readonly TValue[] _availableValues;
    private readonly char _background = ScreenHelper.Empty;
    private TValue _lastValue;

    public ComboBox(string? header, Func<TValue, string>? nameGetter = null)
    {
        nameGetter ??= @enum => $"{@enum:G}" ;
        if (header != null)
        {
            AddVisualChild(Header = new TextBlock{Text = header});
        }
        else
        {
            AddVisualChild(Header = new TextBlock());
        }
        _availableIndexesFromValue = Enum.GetValues<TValue>().Select((v, i) => new KeyValuePair<TValue, int>(v, i)).ToDictionary();
        _availableTitlesFromValue = Enum.GetValues<TValue>().Select(v => new KeyValuePair<TValue, string>(v, nameGetter(v))).ToDictionary();
        _availableValues = Enum.GetValues<TValue>().ToArray();
    }
    
    public TextBlock Header { get; }
    
    public TValue Value
    {
        get => _value;
        set
        {
            if (_value.Equals(value)) return;
            _value = value;
            RiseRenderRequestEvent();
            // Event(new EnumValueEditedEvent<TValue>(this, _value));
        }
    }

    public override int Width => Math.Max(Header.Width, _availableTitlesFromValue[Value].Length);
    public override int Height => 1;
    public override void Render(IRenderContext ctx)
    {
        Header.Render(ctx.Crop(0, 0, Header.Width, 1));
        if (IsFocused && _isCaretVisible)
        {
            var startX = ctx.Width - Header.Width;
            var backgroundWidth = ctx.Width - (Header.Width + 1);
            ctx.FillChar(startX,0,backgroundWidth,_background);
            ctx.Write(ctx.Width - 1,0,Cursor);
        }
        else
        {
            var strValue = _availableTitlesFromValue[_value];
            var startX = ctx.Size.Width - Header.Width;
            var backgroundWidth = ctx.Size.Width - (Header.Width + strValue.Length);
            ctx.FillChar(startX,0,backgroundWidth,_background);
            startX = ctx.Size.Width - strValue.Length;
            ctx.WriteString(startX,0,strValue);
        }
    }

    public char Cursor { get; set; } = '_';
    
    protected override void OnGotFocus()
    {
        _lastValue = Value;
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

    protected override void InternalOnEvent(RoutedEvent e)
    {
        if (e is KeyDownEvent key)
        {
            switch (key.Key.Type)
            {
                case KeyType.Digit:
                    if (key.Key.Value == '1')
                    {
                        Value = _availableIndexesFromValue.TryGetValue(Value, out var idx)
                            ? _availableValues[(idx + 1) % _availableValues.Length]
                            : _availableValues[0];
                    }
                    e.IsHandled = true;
                    IsFocused = true;
                    break;
                case KeyType.UpArrow:
                    var index = (_availableIndexesFromValue[Value] + 1) % _availableValues.Length;
                    Value = _availableValues[index];
                    e.IsHandled = true;
                    IsFocused = true;
                    break;
                case KeyType.DownArrow:
                    var index1 = _availableIndexesFromValue[Value] - 1;
                    if (index1 < 0) index1 = _availableValues.Length - 1;
                    Value = _availableValues[index1];
                    e.IsHandled = true;
                    IsFocused = true;
                    break;
                case KeyType.Enter:
                    e.IsHandled = true;
                    IsFocused = false;
                    Event(new EnumValueEditedEvent<TValue>(this, Value));
                    break;
                case KeyType.Escape:
                    Value = _lastValue;
                    e.IsHandled = true;
                    IsFocused = false;
                    Event(new EnumValueEditedEvent<TValue>(this, Value));
                    break;
            }
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