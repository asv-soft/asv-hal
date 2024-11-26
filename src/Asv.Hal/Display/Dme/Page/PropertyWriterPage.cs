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
    private readonly string _blinkText = "***";


    public PropertyWriterPage(string? header, string? propertyName, IList<double> predefinedValues,
        Func<double, string> stringFormat, Action<bool>? onOffCallback = null, Action<double>? setCallback = null) : base(null)
    {
        Header = new ToggleSwitchWithCallBack(header, onOffCallback);
        PropertyName = propertyName;
        _predefinedValues = predefinedValues.Select((v, i) => new KeyValuePair<int,double>(i, v)).ToDictionary();
        _stringFormat = stringFormat;
        _setCallback = setCallback ?? (_ => { });
        _link = new BlinkTextBlock { Align = HorizontalPosition.Right, IsVisible = true, Text = "***" };
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
    
    public bool Link
    {
        get => _link.IsBlink;
        set
        {
            _link.Text = value ? _blinkText : "";
            _link.IsBlink = value;
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