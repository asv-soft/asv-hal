using System.Globalization;

namespace Asv.Hal;

public class PropertyReaderPage : GroupBox
{
    private readonly string _stringFormat;
    private double _propertyValue;
    private readonly char _background = ScreenHelper.Empty;
    private readonly BlinkTextBlock _link;
    private readonly string _blinkText = "***";

    public PropertyReaderPage(string? header, string? propertyName, string stringFormat, Action<bool>? onOffCallback = null) : base(null)
    {
        Header = new ToggleSwitchWithCallBack(header, onOffCallback);
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

    public bool Link
    {
        get => _link.IsBlink;
        set
        {
            _link.Text = value ? _blinkText : "";
            _link.IsBlink = value;
        }
    }

    protected override void InternalOnEvent(RoutedEvent e)
    {
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