namespace Asv.Hal;


public class ComboBox<TValue> : Control where TValue : struct, Enum 
{
    private TValue _value;
    private readonly IDictionary<char, TValue> _availableValuesFromChar;
    private readonly IDictionary<TValue, int> _availableIndexesFromValue;
    private readonly IDictionary<TValue, string> _availableTitlesFromValue;
    private readonly TValue[] _availableValues;
    private readonly char _background = ScreenHelper.Empty;

    public ComboBox(string? header, Func<TValue, string>? nameGetter = null)
    {
        nameGetter = nameGetter ?? (@enum => $"{@enum:G}") ;
        if (header != null)
        {
            AddVisualChild(Header = new TextBlock{Text = header});
        }
        else
        {
            AddVisualChild(Header = new TextBlock());
        }
        _availableValuesFromChar = Enum.GetValues<TValue>().Take(9).Select((v, i) => new KeyValuePair<char, TValue>($"{i + 1}"[0], v)).ToDictionary();
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
            Event(new EnumValueEditedEvent<TValue>(this, _value));
        }
    }

    public override int Width => Math.Max(Header.Width, _availableTitlesFromValue[Value].Length);
    public override int Height => 2;
    public override void Render(IRenderContext ctx)
    {
        var strValue = _availableTitlesFromValue[_value];
        // var headerStartX = ctx.Size.Width - Header.Width;
        var headerStartX = 2; // "1."
        // if (headerStartX < 0) headerStartX = 0;
        Header.Render(ctx.Crop(headerStartX, 0, Header.Width, 1));
        var startX = (ctx.Size.Width - strValue.Length) / 2;
        ctx.FillChar(0,1,startX,_background);
        ctx.WriteString(startX,1,strValue);
        ctx.FillChar(startX + strValue.Length,1,ctx.Size.Width - startX - strValue.Length,_background);
    }

    protected override void InternalOnEvent(RoutedEvent e)
    {
        if (e is KeyDownEvent key)
        {
            switch (key.Key.Type)
            {
                case KeyType.Digit:
                    if (_availableValuesFromChar.TryGetValue(key.Key.Value ?? default, out var value))
                    {
                        Value = value;
                    }
                    e.IsHandled = true;
                    IsFocused = false;
                    break;
                case KeyType.UpArrow:
                    var index = (_availableIndexesFromValue[Value] + 1) % _availableValues.Length;
                    Value = _availableValues[index];
                    e.IsHandled = true;
                    IsFocused = false;
                    break;
                case KeyType.DownArrow:
                    var index1 = _availableIndexesFromValue[Value] - 1;
                    if (index1 < 0) index1 = _availableValues.Length - 1;
                    Value = _availableValues[index1];
                    e.IsHandled = true;
                    IsFocused = false;
                    break;
            }
        }

    }
}