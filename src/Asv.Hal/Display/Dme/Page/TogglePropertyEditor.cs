using System.Diagnostics;

namespace Asv.Hal;

public class TogglePropertyEditor : PropertyEditor
{
    private readonly Action<string> _exSetCallback;
    private readonly Func<string?, string> _exValueValidator;
    

    public TogglePropertyEditor(string? header, string trueText = "ON", string falseText = "OFF", Action<bool>? onOffCallback = null, Func<string?, string>? exValueValidator = null, Action<string>? exSetCallback = null)
    {
        _exSetCallback = exSetCallback ?? (_ => { });
        _exValueValidator = exValueValidator ?? (_ => string.Empty);
        Header = new ToggleSwitch(header, trueText, falseText);
    }

    public void ExternalUpdateValue(bool onOff)
    {
        ((ToggleSwitch)Header!).Value = onOff;
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
                        if (SelectedIndex == 2 && SelectedItem != null) 
                        {
                            SelectedItem.IsFocused = true;
                            Event(new ValueEditingProcessEvent(SelectedItem));
                        }
                        else
                        {
                            var copy = e.Clone();
                            SelectedItem?.Event(copy);    
                        }
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
        
        if (e is ValueEditedEvent rrr)
        {
            var value = _exValueValidator(rrr.Value);
            WriteExValue(value);
            _exSetCallback.Invoke(value);
        }
    }

    private void WriteExValue(string value)
    {
        if (Items.Count > 2 && Items[2] is TextBox)
        {
            ((TextBox)Items[2]).Text = value;
        }
    }

    public void ExternalUpdateValue(int index, bool onOff)
    {
        if (index < Items.Count)
        {
            ((ToggleSwitch)Items[index]).Value = onOff;
        }
    }

    public void ExternalUpdateValue(ushort value)
    {
        if (Items.Count == 3) ((TextBox)Items[2]).Text = value.ToString();
    }
}