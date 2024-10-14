using System.Diagnostics;

namespace Asv.Hal;

public class MenuPanel(string? header = null) : ListBox(header)
{
    protected override void InternalOnEvent(RoutedEvent e)
    {
        base.InternalOnEvent(e);
        if (e.IsHandled) return;
        if (e is KeyDownEvent key && IsFocused)
        {
            switch (key.Key.Type)
            {
                case KeyType.Enter:
                    if (SelectedItem != null) SelectedItem.IsFocused = true;
                    e.IsHandled = true;
                    break;
                case KeyType.LeftArrow:
                    SelectedIndex = 0;
                    e.IsHandled = true;
                    break;
                case KeyType.RightArrow:
                    SelectedIndex = Items.Count - 1;
                    e.IsHandled = true;
                    break;
            }
        }
    }
}