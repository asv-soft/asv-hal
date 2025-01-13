using System.Diagnostics;

namespace Asv.Hal;

public class PropertyEditor:ListBox
{
    public PropertyEditor(string? header = null, HorizontalPosition headerAlign = HorizontalPosition.Left)
        : base(header, headerAlign)
    {
        Pointer = string.Empty;
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
                    case KeyType.DownArrow:
                        SelectedIndex++;
                        e.IsHandled = true;
                        break;
                    case KeyType.UpArrow:
                        SelectedIndex--;
                        e.IsHandled = true;
                        break;
                    case KeyType.Digit:
                        Debug.Assert(key.Key.Value.HasValue);
                        SelectedIndex = int.Parse(key.Key.Value.Value.ToString()) - 1;// numbering start with 1
                        if (SelectedItem != null)
                        {
                            SelectedItem.IsFocused = true;
                            Event(new ValueEditingProcessEvent(SelectedItem));
                        }
                        e.IsHandled = true;
                        break;
                }
            }
            else
            {
                var copy = e.Clone();
                e.IsHandled = true;
                SelectedItem?.Event(copy);
            }
            
        }
    }
}