namespace Asv.Hal;

public class CustomToggleSwitch : ToggleSwitch
{
    private readonly KeyType _eventKeyType;
    private readonly string _trueText;
    private readonly string _falseText;
    private readonly Action<bool> _onClick;

    public CustomToggleSwitch(KeyType eventKeyType, string? header = null, string trueText = "ON", string falseText = "OFF", Action<bool>? onClick = null) : base(header, trueText, falseText)
    {
        _eventKeyType = eventKeyType;
        _trueText = trueText;
        _falseText = falseText;
        _onClick = onClick ?? (_ => { });
    }

    protected override void InternalOnEvent(RoutedEvent e)
    {
        if (e is KeyDownEvent kd && kd.Key.Type == _eventKeyType)
        {
            e.IsHandled = true;
            Value = !Value;
            _onClick.Invoke(Value);
        }
    }

    public override void Render(IRenderContext ctx)
    {
        var strValue = Value ? _trueText : _falseText;
        Header.Render(ctx);
        ctx.WriteString(Header.Width + 1,0,strValue);
    }
}