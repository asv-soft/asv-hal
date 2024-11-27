namespace Asv.Hal;

public class CustomToggleSwitch : ToggleSwitchWithCallBack
{
    private readonly KeyType _eventKeyType;

    public CustomToggleSwitch(KeyType eventKeyType, string? header = null, string trueText = "ON", string falseText = "OFF", Action<bool>? onOffCallBack = null) : base(header, trueText, falseText, onOffCallBack)
    {
        _eventKeyType = eventKeyType;
    }

    protected override void InternalOnEvent(RoutedEvent e)
    {
        if (e is KeyDownEvent kd && kd.Key.Type == _eventKeyType)
        {
            e.IsHandled = true;
            Value = !Value;
            // Event(new LostFocusEvent(this));
        }
    }

    public override void Render(IRenderContext ctx)
    {
        var strValue = _value ? _trueText : _falseText;
        Header.Render(ctx);
        ctx.WriteString(Header.Width + 1,0,strValue);
    }
}