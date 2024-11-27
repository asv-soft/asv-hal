namespace Asv.Hal;

public class ToggleSwitch:Control
{
    protected bool _value;
    protected string _trueText;
    protected string _falseText;

    public ToggleSwitch(string? header = null, string trueText = "ON", string falseText = "OFF")
    {
        _trueText = trueText;
        _falseText = falseText;
        if (header != null)
        {
            AddVisualChild(Header = new TextBlock{Text = header});
        }
        else
        {
            AddVisualChild(Header = new TextBlock());
        }
    }
    public TextBlock Header { get; }
    public virtual bool Value
    {
        get => _value;
        set
        {
            if (_value == value) return;
            _value = value;
            RiseRenderRequestEvent();
        }
    }

    public string TrueText
    {
        get => _trueText;
        set
        {
            if (_trueText == value) return;
            _trueText = value;
            RiseRenderRequestEvent();
        }
    }
    public string FalseText
    {
        get => _falseText;
        set
        {
            if (_falseText == value) return;
            _falseText = value;
            RiseRenderRequestEvent();
        }
    }

    public override int Width => Header.Width + (Value ? _trueText.Length : _falseText.Length);
    public override int Height => 1;

    public override void Render(IRenderContext ctx)
    {
        var strValue = _value ? _trueText : _falseText;
        Header.Render(ctx.Crop(0,0,ctx.Width-strValue.Length,1));
        ctx.WriteString(ctx.Width-strValue.Length,0,strValue);
    }

    protected override void InternalOnEvent(RoutedEvent e)
    {
        if (e is KeyDownEvent { Key.Type: KeyType.Enter })
        {
            e.IsHandled = true;
            Value = !Value;
        }
    }
}

public class ToggleSwitchWithCallBack(string? header = null, string trueText = "ON", string falseText = "OFF", Action<bool>? onOffCallBack = null) : ToggleSwitch(header, trueText, falseText)
{
    private bool _isInternalChanged;

    public override bool Value
    {
        get => base.Value;
        set
        {
            base.Value = value;
            if (!_isInternalChanged) onOffCallBack?.Invoke(value);
        }
    }

    public void SetOnOff(bool b)
    {
        _isInternalChanged = true;
        Value = b;
        _isInternalChanged = false;
    }
}