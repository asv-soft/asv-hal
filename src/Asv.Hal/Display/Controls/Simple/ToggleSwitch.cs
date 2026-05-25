using Asv.Common;

namespace Asv.Hal;

public class ToggleSwitch:Control
{
    private bool _value;
    private string _trueText;
    private string _falseText;
    
    public ToggleSwitch(string? header = null, string trueText = "ON", string falseText = "OFF")
        : this(header, trueText, falseText, true)
    {
    }

    protected ToggleSwitch(string? header, string trueText, string falseText, bool registerEventHandlers)
    {
        _trueText = trueText;
        _falseText = falseText;
        if (registerEventHandlers)
        {
            Events.Catch<KeyDownEvent>(OnKeyDownEvent).DisposeItWith(Disposable);
        }

        if (header != null)
        {
            AddVisualChild(Header = new TextBlock{Text = header});
        }
        else
        {
            AddVisualChild(Header = new TextBlock());
        }
    }

    protected TextBlock Header { get; }
    public bool Value
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

    private void OnKeyDownEvent(KeyDownEvent e)
    {
        if (e.Key.Type == KeyType.Enter)
        {
            e.IsHandled = true;
            Value = !Value;
            Events.Rise(new ToggleSwichEvent(this, _value));
        }
    }
}
