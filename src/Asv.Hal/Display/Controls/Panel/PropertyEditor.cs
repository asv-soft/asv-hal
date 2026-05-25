using System.Diagnostics;
using Asv.Common;

namespace Asv.Hal;

public class PropertyEditor:ListBox
{
    public PropertyEditor(string? header = null, HorizontalPosition headerAlign = HorizontalPosition.Left)
        : this(header, headerAlign, true)
    {
    }

    protected PropertyEditor(string? header, HorizontalPosition headerAlign, bool registerEventHandlers)
        : base(header, headerAlign, false)
    {
        Pointer = string.Empty;
        if (registerEventHandlers == false) return;
        Events.Catch<LostFocusEvent>(OnLostFocusEvent).DisposeItWith(Disposable);
        Events.Catch<KeyDownEvent>(OnKeyDownEvent).DisposeItWith(Disposable);
    }

    private void OnLostFocusEvent(LostFocusEvent e)
    {
        if (e.Sender == SelectedItem)
        {
            IsFocused = true;
        }
    }

    private void OnKeyDownEvent(KeyDownEvent e)
    {
        if (IsFocused)
        {
            switch (e.Key.Type)
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
                    Debug.Assert(e.Key.Value.HasValue);
                    SelectedIndex = int.Parse(e.Key.Value.Value.ToString()) - 1;// numbering start with 1
                    if (SelectedItem != null)
                    {
                        SelectedItem.IsFocused = true;
                        Events.Rise(new ValueEditingProcessEvent(SelectedItem));
                    }
                    e.IsHandled = true;
                    break;
            }
        }
        else
        {
            var copy = e.Clone();
            e.IsHandled = true;
            SelectedItem?.Events.Rise(copy);
        }
    }
}
