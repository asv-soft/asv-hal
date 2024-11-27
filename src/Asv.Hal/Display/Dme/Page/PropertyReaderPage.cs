using System.Globalization;

namespace Asv.Hal;

public enum SignalState
{
    NoSignal,
    Normal,
    Overload
}

public class PropertyReaderPage : GroupBox
{
    private readonly string _stringFormat;
    private double _propertyValue;
    private readonly char _background = ScreenHelper.Empty;
    private readonly BlinkTextBlock _link;
    private readonly string _blinkNormalText = "***";
    private readonly string _blinkOverloadText;
    private SignalState _signal;

    public PropertyReaderPage(string? header, string trueText, string falseText,  string? propertyName, string stringFormat, string overloadText, Action<bool>? onOffCallback = null) : base(null)
    {
        _blinkOverloadText = overloadText;
        Header = new ToggleSwitchWithCallBack(header, trueText, falseText, onOffCallback);
        _stringFormat = stringFormat;
        PropertyName = propertyName;
        _link = new BlinkTextBlock { Align = HorizontalPosition.Right, IsVisible = true, IsBlink = false, Text = "" };
        Items.Add(_link);
    }

    public void ExternalUpdateValue(bool onOff)
    {
        ((ToggleSwitchWithCallBack)Header!).SetOnOff(onOff);
    }
    public string? PropertyName { get; }

    public double PropertyValue
    {
        get => _propertyValue;
        set
        {
            if (_propertyValue == value) return;
            _propertyValue = value;
            if (IsFocused)
                RiseRenderRequestEvent();
        }
    }

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

    protected override void InternalOnEvent(RoutedEvent e)
    {
        if (e is RenderRequestEvent rend && rend.Sender == _link && !IsFocused)
        {
            e.IsHandled = true;
            return;
        }
        
        if (e is not KeyDownEvent { Key.Type: KeyType.Enter }) return;
        var copy = e.Clone();
        e.IsHandled = true;
        Header?.Event(copy);
    }

    protected override void InternalRenderChildren(IRenderContext ctx)
    {
        ctx.WriteString(0,0,PropertyName);
        var v = !double.IsNaN(PropertyValue) ? PropertyValue.ToString(_stringFormat, CultureInfo.InvariantCulture) : "";
        var startX = (ctx.Size.Width - v.Length) / 2;
        ctx.FillChar(0,1,startX,_background);
        ctx.WriteString(startX,1,v);
        ctx.FillChar(startX + v.Length,1,ctx.Size.Width - startX - v.Length,_background);
        _link.Render(ctx.Crop(0, 2, ctx.Size.Width, 1));
    }

    
}