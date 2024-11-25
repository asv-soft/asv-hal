using System.Diagnostics;

namespace Asv.Hal;

public class TogglePropertyEditor : PropertyEditor
{
    public TogglePropertyEditor(string? header, Action<bool>? onOffCallback = null)
    {
        Header = new ToggleSwitchWithCallBack(header, onOffCallback);
    }

    public void ExternalUpdateValue(bool onOff)
    {
        ((ToggleSwitchWithCallBack)Header!).SetOnOff(onOff);
    }
    protected override void InternalOnEvent(RoutedEvent e)
    {
        if (e is LostFocusEvent focus && focus.Sender == SelectedItem)
        {
            IsFocused = true;
        }
        if (e is KeyDownEvent key)
        {
            if (IsFocused)
            {
                switch (key.Key.Type)
                {
                    case KeyType.Enter:
                        e.IsHandled = true;
                        var copy1 = e.Clone();
                        Header?.Event(copy1);
                        break;
                    case KeyType.Digit:
                        Debug.Assert(key.Key.Value.HasValue);
                        SelectedIndex = int.Parse(key.Key.Value.Value.ToString()) - 1;
                        e.IsHandled = true;
                        var copy = e.Clone();
                        SelectedItem?.Event(copy);
                        break;
                }
            }
        }
    }

    public void ExternalUpdateValue(int index, bool onOff)
    {
        if (index < Items.Count)
        {
            ((ToggleSwitchWithCallBack)Items[index]).SetOnOff(onOff);
        }
    }
}