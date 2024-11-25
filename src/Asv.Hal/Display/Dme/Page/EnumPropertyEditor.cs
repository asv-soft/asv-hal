using System.Diagnostics;

namespace Asv.Hal;

public class EnumPropertyEditor<TValue> : GroupBox where TValue : struct, Enum 
{
    private readonly Action<TValue> _setCallback;

    public EnumPropertyEditor(string? header, string? enumHeader, TValue defaultValue, Func<TValue, string>? nameGetter = null, Action<bool>? onOffCallback = null, Action<TValue>? setCallback = null) : base(null)
    {
        _setCallback = setCallback ?? (_ => { }) ;
        Header = new ToggleSwitchWithCallBack(header, onOffCallback);
        var item = new ComboBox<TValue>(enumHeader, nameGetter);
        Items.Add(item);
        item.Value = defaultValue;
    }

    public void ExternalUpdateValue(bool onOff)
    {
        ((ToggleSwitchWithCallBack)Header!).SetOnOff(onOff);
    }
    private ComboBox<TValue>? Item => (ComboBox<TValue>)Items[0];

    public void ExternalUpdateValue(TValue value)
    {
        if (Item != null) Item.Value = value;
    }
    
    protected override void InternalOnEvent(RoutedEvent e)
    {
        if (e is LostFocusEvent focus && focus.Sender == Item)
        {
            IsFocused = true;
        }

        if (e is EnumValueEditedEvent<TValue> edit && edit.Sender == Item)
        {
            _setCallback.Invoke(edit.Value);
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
                        if (Item != null) Item.IsFocused = true;
                        e.IsHandled = true;
                        IsFocused = false;
                        var copy1 = e.Clone();
                        Item?.Event(copy1);
                        break;
                    case KeyType.UpArrow:
                        if (Item != null) Item.IsFocused = true;
                        e.IsHandled = true;
                        IsFocused = false;
                        var copy2 = e.Clone();
                        Item?.Event(copy2);
                        break;
                    case KeyType.Digit:
                        Debug.Assert(key.Key.Value.HasValue);
                        if (Item != null) Item.IsFocused = true;
                        e.IsHandled = true;
                        IsFocused = false;
                        var copy3 = e.Clone();
                        Item?.Event(copy3);
                        break;
                }
            }
        }
    }

    protected override void InternalRenderChildren(IRenderContext ctx)
    {
        if (Item == null) return;
        var prefix = "1.";
        var startX = ctx.Size.Width - prefix.Length - Item.Width;
        if (startX < 0) startX = 0;
        ctx.WriteString(startX,0,prefix);
        var availableSize = new Size(ctx.Width - startX, ctx.Size.Height);
        Item.Render(ctx.Crop(startX, 0, availableSize));
    }
}