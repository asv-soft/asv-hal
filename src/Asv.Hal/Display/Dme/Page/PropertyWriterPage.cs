using System.Diagnostics;

namespace Asv.Hal;

public class PropertyWriterPage : GroupBox
{
    private readonly IDictionary<int, double> _predefinedValues;
    private readonly Func<double, string> _stringFormat;
    private readonly Action<double> _setCallback;
    private readonly char _background = ScreenHelper.Empty;
    private double _propertyValue;
    private bool _isInternalChanged;
    private readonly BlinkTextBlock _link;
    private readonly string _blinkNormalText = "***";
    private readonly string _blinkOverloadText;
    private SignalState _signal;


    public PropertyWriterPage(string? header, string trueText, string falseText, string? propertyName, IList<double> predefinedValues,
        Func<double, string> stringFormat, string overloadText, Action<bool>? onOffCallback = null, Action<double>? setCallback = null) : base(null)
    {
        _blinkOverloadText = overloadText;
        Header = new ToggleSwitchWithCallBack(header, trueText, falseText, onOffCallback);
        PropertyName = propertyName;
        _predefinedValues = predefinedValues.Select((v, i) => new KeyValuePair<int,double>(i, v)).ToDictionary();
        _stringFormat = (Func<double, string>)stringFormat.Clone();
        _setCallback = setCallback ?? (_ => { });
        _link = new BlinkTextBlock { Align = HorizontalPosition.Right, IsVisible = true, Text = "", IsBlink = false };
        Items.Add(_link);
    }

    public void ExternalUpdateValue(bool onOff)
    {
        ((ToggleSwitchWithCallBack)Header!).SetOnOff(onOff);
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

    protected override void InternalRenderChildren(IRenderContext ctx)
    {
        ctx.WriteString(0,0,PropertyName);
        try
        {
            var val = _predefinedValues.First(
                v => Math.Abs(Math.Round(v.Value) - Math.Round(_propertyValue)) < 0.1);
            ctx.WriteString(PropertyName?.Length ?? 0,0, $" ({val.Key + 1})");
        }
        catch (Exception)
        {
            // ignored
        }

        var v = _stringFormat(PropertyValue);
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
        
        if (e is KeyDownEvent key)
        {
            if (IsFocused)
            {
                switch (key.Key.Type)
                {
                    case KeyType.Enter:
                        var copy = e.Clone();
                        e.IsHandled = true;
                        Header?.Event(copy);
                        break;
                    case KeyType.DownArrow:
                        var index1 = GetNearestValue(PropertyValue, _predefinedValues) - 1;
                        if (index1 < 0) index1 = _predefinedValues.Count - 1;
                        PropertyValue = _predefinedValues[index1];
                        e.IsHandled = true;
                        break;
                    case KeyType.UpArrow:
                        var index2 = GetNearestValue(PropertyValue, _predefinedValues) + 1;
                        if (index2 > _predefinedValues.Count - 1) index2 = 0;
                        PropertyValue = _predefinedValues[index2];
                        e.IsHandled = true;
                        break;
                    case KeyType.Digit:
                        Debug.Assert(key.Key.Value.HasValue);
                        var index3 = int.Parse(key.Key.Value.Value.ToString()) - 1;
                        if (index3 >= 0 && index3 < _predefinedValues.Count)
                            PropertyValue = _predefinedValues[index3];
                        e.IsHandled = true;
                        break;
                }
            }
        }
    }

    private static int GetNearestValue(double val, IDictionary<int, double> predefinedValues)
    {
        return predefinedValues.Select(item => new KeyValuePair<int, double>(item.Key, Math.Abs(item.Value - val)))
            .MinBy(item => item.Value).Key;
    }
}