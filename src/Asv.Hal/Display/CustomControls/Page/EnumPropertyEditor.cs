using System.Diagnostics;
using Asv.Common;

namespace Asv.Hal;

public class EnumPropertyEditor<TValue> : GroupBox where TValue : struct, Enum 
{
    private readonly Action<TValue> _setCallback;

    public EnumPropertyEditor(string? header, string trueText, string falseText,  string? enumHeader, TValue defaultValue, Func<TValue, string>? nameGetter = null, Action<TValue>? setCallback = null) : base(null)
    {
        Events.Catch<LostFocusEvent>(OnLostFocusEvent).DisposeItWith(Disposable);
        Events.Catch<EnumValueEditedEvent<TValue>>(OnEnumValueEditedEvent).DisposeItWith(Disposable);
        Events.Catch<KeyDownEvent>(OnKeyDownEvent).DisposeItWith(Disposable);
        _setCallback = setCallback ?? (_ => { }) ;
        Header = new ToggleSwitch(header, trueText, falseText);
        var item = new SingleComboBox<TValue>(enumHeader, nameGetter);
        item.Value = defaultValue;
        Items.Add(item);
    }

    public void ExternalUpdateValue(bool onOff)
    {
        ((ToggleSwitch)Header!).Value = onOff;
    }
    public SingleComboBox<TValue>? Item => Items.Count > 0 ? (SingleComboBox<TValue>)Items[0] : null;

    public void ExternalUpdateValue(TValue value)
    {
        if (Item != null) Item.Value = value;
    }
    
    private void OnLostFocusEvent(LostFocusEvent e)
    {
        if (e.Sender == Item)
        {
            IsFocused = true;
        }
    }

    private void OnEnumValueEditedEvent(EnumValueEditedEvent<TValue> e)
    {
        if (e.Sender == Item)
        {
            _setCallback.Invoke(e.Value);
        }
    }

    private void OnKeyDownEvent(KeyDownEvent e)
    {
        if (IsFocused)
        {
            switch (e.Key.Type)
            {
                case KeyType.Enter:
                    var copy = e.Clone();
                    e.IsHandled = true;
                    Header?.Events.Rise(copy);
                    break;
                case KeyType.DownArrow:
                    if (Item != null) Item.IsFocused = true;
                    e.IsHandled = true;
                    IsFocused = false;
                    var copy1 = e.Clone();
                    Item?.Events.Rise(copy1);
                    break;
                case KeyType.UpArrow:
                    if (Item != null) Item.IsFocused = true;
                    e.IsHandled = true;
                    IsFocused = false;
                    var copy2 = e.Clone();
                    Item?.Events.Rise(copy2);
                    break;
                case KeyType.Digit:
                    Debug.Assert(e.Key.Value.HasValue);
                    if (Item != null) Item.IsFocused = true;
                    e.IsHandled = true;
                    IsFocused = false;
                    var copy3 = e.Clone();
                    Item?.Events.Rise(copy3);
                    break;
            }
        }
    }

    protected override void InternalRenderChildren(IRenderContext ctx)
    {
        if (Item == null) return;
        var prefix = "1.";
        // var startX = ctx.Size.Width - prefix.Length - Item.Width;
        var startX = 0;
        // if (startX < 0) startX = 0;
        ctx.WriteString(startX,0,prefix);
        // var availableSize = new Size(ctx.Width - 2, ctx.Size.Height);
        // Item.Render(ctx.Crop(startX, 0, availableSize));
        Item.Render(ctx);
    }
}
