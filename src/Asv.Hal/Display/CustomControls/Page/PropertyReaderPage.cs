using System.Globalization;

namespace Asv.Hal;

public class PropertyReaderPage : GroupBox
{
    private readonly string _stringFormat;
    private double _propertyValue;
    private readonly char _background = ScreenHelper.Empty;
    
    public PropertyReaderPage(string? header, string trueText, string falseText,  string? propertyName, string stringFormat) : base(null)
    {
        Header = new ToggleSwitch(header, trueText, falseText);
        _stringFormat = stringFormat;
        PropertyName = propertyName;
        
    }

    public void ExternalUpdateValue(bool onOff)
    {
        ((ToggleSwitch)Header!).Value = onOff;
    }

    private string? PropertyName { get; }

    public double PropertyValue
    {
        get => _propertyValue;
        set
        {
            if (Math.Abs(_propertyValue - value) < 0.01) return;
            _propertyValue = value;
            if (IsFocused)
                RiseRenderRequestEvent();
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
    }
}