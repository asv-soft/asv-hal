using Asv.Common;

namespace Asv.Hal;

public class MenuPanel : ListBox
{
    public MenuPanel(string? header = null, HorizontalPosition headerAlign = HorizontalPosition.Left)
        : base(header, headerAlign)
    {
        Events.Catch<KeyDownEvent>(OnKeyDownEvent).DisposeItWith(Disposable);
    }

    private void OnKeyDownEvent(KeyDownEvent e)
    {
        if (e.IsHandled) return;
        if (IsFocused)
        {
            switch (e.Key.Type)
            {
                case KeyType.Enter:
                    if (SelectedItem != null)
                    {
                        // SelectedItem.IsFocused = false;
                        SelectedItem.IsFocused = true;
                    }
                    e.IsHandled = true;
                    break;
                case KeyType.LeftArrow:
                    SelectedIndex = 0;
                    e.IsHandled = true;
                    Events.Rise(new LostFocusEvent(this));
                    break;
                case KeyType.RightArrow:
                    SelectedIndex = Items.Count - 1;
                    e.IsHandled = true;
                    break;
            }
        }
    }
}
