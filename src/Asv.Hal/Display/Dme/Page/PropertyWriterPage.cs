using System.Diagnostics;
using System.Globalization;

namespace Asv.Hal;

public class PropertyWriterPage : GroupBox
{
    private bool _isCaretVisible;
    private TimeSpan _blinkTime = TimeSpan.FromMilliseconds(500);
    private long _lastBlink;
    private readonly IDictionary<int, double> _predefinedValues;
    private readonly Func<double, string> _stringFormat;
    private readonly Action<double> _setCallback;
    private readonly char _background = ScreenHelper.Empty;
    private double _propertyValue;
    private bool _isInternalChanged;
    private readonly BlinkTextBlock _link;
    private readonly string _blinkNormalText = "***";
    private readonly double _min;
    private readonly double _max;
    private readonly string _blinkOverloadText;
    private SignalState _signal;
    private readonly Dictionary<int,string> _predefinedTitles;
    private bool _isEditingProcess;
    private string? _text;
    private string? _lastValue;


    public PropertyWriterPage(string? header, string trueText, string falseText, string? propertyName, IList<double> predefinedValues, double min, double max,
        Func<double, string> stringFormat, string overloadText, Action<double>? setCallback = null) : base(null)
    {
        _min = min;
        _max = max;
        _blinkOverloadText = overloadText;
        Header = new ToggleSwitch(header, trueText, falseText);
        PropertyName = propertyName;
        _predefinedValues = predefinedValues.Select((v, i) => new KeyValuePair<int,double>(i, v)).ToDictionary();
        _stringFormat = (Func<double, string>)stringFormat.Clone();
        _predefinedTitles = predefinedValues.Select((v, i) => new KeyValuePair<int,string>(i, _stringFormat(v))).ToDictionary();
        _setCallback = setCallback ?? (_ => { });
        _link = new BlinkTextBlock { Align = HorizontalPosition.Right, IsVisible = true, Text = "", IsBlink = false };
        Items.Add(_link);
    }

    public void ExternalUpdateValue(bool onOff)
    {
        ((ToggleSwitch)Header!).Value = onOff;
    }
    
    public void ExternalUpdateValue(double value)
    {
        _isInternalChanged = true;
        PropertyValue = value;
        _isInternalChanged = false;
    }
    
    public string? PropertyName { get; }
    
    public SignalState Signal
    {
        get => _signal;
        set
        {
            if (value == _signal) return;
            _signal = value;
            switch (_signal)
            {
                case SignalState.NoSignal:
                    _link.Text = string.Empty;
                    _link.IsBlink = false;
                    break;
                case SignalState.Normal:
                    _link.Text = _blinkNormalText;
                    _link.IsBlink = true;
                    break;
                case SignalState.Overload:
                    _link.Text = _blinkOverloadText;
                    _link.IsBlink = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(value), value, null);
            }
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
    
    public double PropertyValue
    {
        get => _propertyValue;
        set
        {
            if (Math.Abs(_propertyValue - value) < 0.5) return;
            _propertyValue = value;
            if (!_isInternalChanged) _setCallback(value);
            if (IsFocused)
                RiseRenderRequestEvent();
        }
    }

    public char Cursor { get; set; } = '_';
    protected override void InternalRenderChildren(IRenderContext ctx)
    {
        ctx.WriteString(0,0,PropertyName);
        
        string? v = null;
        try
        {
            if (!_isEditingProcess)
            {
                var val = _predefinedValues.First(
                    pv => Math.Abs(Math.Round(pv.Value) - Math.Round(_propertyValue)) < 0.1);
                ctx.WriteString(PropertyName?.Length ?? 0,0, $" ({val.Key + 1})");
                v = _predefinedTitles.FirstOrDefault(x => x.Key == val.Key).Value;
            }
            
            
        }
        catch (Exception)
        {
            // ignored
        }

        if (!_isEditingProcess)
        {
            v ??= _stringFormat(PropertyValue);
        }
        else
        {
            v = _text ?? string.Empty;
            if (_isCaretVisible)
            {
                if (v.Length > 0)
                    v = v[..^1] + Cursor;
                else
                    v = Cursor.ToString();
            }
        }
        
        var startX = (ctx.Size.Width - v.Length) / 2;
        ctx.FillChar(0,1,startX,_background);
        ctx.WriteString(startX,1,v);
        ctx.FillChar(startX + v.Length,1,ctx.Size.Width - startX - v.Length,_background);
        _link.Render(ctx.Crop(0, 2, ctx.Size.Width, 1));
    }
    
    protected override void InternalOnEvent(RoutedEvent e)
    {
        if (e is RenderRequestEvent rend && rend.Sender == _link && !IsFocused)
        {
            e.IsHandled = true;
            return;
        }

        if (e is ValueEditedEvent rrr && rrr.Sender == this)
        {
            if (string.IsNullOrWhiteSpace(rrr.Value)) return;
            if (double.TryParse(rrr.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var value))
            {
                if (value < _min) value = _min;
                if (value > _max) value = _max;
                PropertyValue = value;
            }
        }
        
        if (e is KeyDownEvent key)
        {
            if (IsFocused)
            {
                switch (key.Key.Type)
                {
                    case KeyType.LeftArrow:
                        if (_isEditingProcess)
                        {
                            if (_text?.Length > 0) _text = _text[..^1];
                            RiseRenderRequestEvent();
                            e.IsHandled = true;
                        }
                        break;
                    case KeyType.Escape:
                        if (_isEditingProcess)
                        {
                            _text = _lastValue;
                            _isCaretVisible = false;
                            _isEditingProcess = false;
                            RiseRenderRequestEvent();
                            InternalOnEvent(new ValueEditedEvent(this, _text));
                            Event(new ValueEditedEvent(this, _text));
                            e.IsHandled = true;
                        }
                        break;
                    case KeyType.Function:
                        if (_isEditingProcess)
                        {
                            if (_text?.Length > 0)
                            {
                                _text = _text[0] == '-' ? _text.Substring(1, _text.Length - 1) : $"-{_text}";
                            }
                            else
                            {
                                _text = "-";
                            }
                            RiseRenderRequestEvent();
                            e.IsHandled = true;
                        }
                        break;
                    case KeyType.Enter:
                        if (_isEditingProcess)
                        {
                            _isCaretVisible = false;
                            _isEditingProcess = false;
                            RiseRenderRequestEvent();
                            InternalOnEvent(new ValueEditedEvent(this, _text));
                            Event(new ValueEditedEvent(this, _text));
                        }
                        else
                        {
                            var copy = e.Clone();
                            Header?.Event(copy);    
                        }
                        e.IsHandled = true;
                        break;
                    case KeyType.DownArrow:
                        var index1 = GetNearestValue(PropertyValue, _predefinedValues) - 1;
                        if (index1 < 0) index1 = _predefinedValues.Count - 1;
                        PropertyValue = _predefinedValues[index1];
                        _text = PropertyValue.ToString("F", CultureInfo.InvariantCulture);
                        e.IsHandled = true;
                        break;
                    case KeyType.UpArrow:
                        var index2 = GetNearestValue(PropertyValue, _predefinedValues) + 1;
                        if (index2 > _predefinedValues.Count - 1) index2 = 0;
                        PropertyValue = _predefinedValues[index2];
                        _text = PropertyValue.ToString("F", CultureInfo.InvariantCulture);
                        e.IsHandled = true;
                        break;
                    case KeyType.Digit:
                        Debug.Assert(key.Key.Value.HasValue);
                        var digit = int.Parse(key.Key.Value.Value.ToString());
                        if (_isEditingProcess)
                        {
                            _text += key.Key.Value.ToString();
                            RiseRenderRequestEvent();
                        }
                        else
                        {
                            switch (digit)
                            {
                                case 0:
                                    _isEditingProcess = true;
                                    _lastValue = _text;
                                    _text = "";
                                    _isCaretVisible = true;
                                    RiseRenderRequestEvent();
                                    Event(new ValueEditingProcessEvent(this));
                                    break;
                                case > 0 when digit <= _predefinedValues.Count:
                                    PropertyValue = _predefinedValues[digit - 1];
                                    _text = PropertyValue.ToString("F", CultureInfo.InvariantCulture);
                                    break;
                            }
                        }
                        
                        e.IsHandled = true;
                        break;
                    case KeyType.Dot:
                        if (_isEditingProcess)
                        {
                            if (!string.IsNullOrEmpty(_text)) _text = _text.Replace(".", "");
                            _text += key.Key.Value.ToString();
                            RiseRenderRequestEvent();
                            e.IsHandled = true;
                        }
                        break;
                }
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

    private static int GetNearestValue(double val, IDictionary<int, double> predefinedValues)
    {
        return predefinedValues.Select(item => new KeyValuePair<int, double>(item.Key, Math.Abs(item.Value - val)))
            .MinBy(item => item.Value).Key;
    }
}