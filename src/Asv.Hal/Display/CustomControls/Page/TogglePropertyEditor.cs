using System.Diagnostics;
using Asv.Common;

namespace Asv.Hal;

public class TogglePropertyEditor : PropertyEditor
{
    private readonly Action<string> _exSetCallback;
    private readonly Func<string?, string> _exValueValidator;
    

    public TogglePropertyEditor(string? header, string trueText = "ON", string falseText = "OFF", Func<string?, string>? exValueValidator = null, Action<string>? exSetCallback = null)
        : base(null, HorizontalPosition.Left, false)
    {
        Events.Catch<LostFocusEvent>(OnLostFocusEvent).DisposeItWith(Disposable);
        Events.Catch<KeyDownEvent>(OnKeyDownEvent).DisposeItWith(Disposable);
        Events.Catch<ValueEditedEvent>(OnValueEditedEvent).DisposeItWith(Disposable);
        _exSetCallback = exSetCallback ?? (_ => { });
        _exValueValidator = exValueValidator ?? (_ => string.Empty);
        Header = new ToggleSwitch(header, trueText, falseText);
    }

    public void ExternalUpdateValue(bool onOff)
    {
        ((ToggleSwitch)Header!).Value = onOff;
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
                case KeyType.Enter:
                    e.IsHandled = true;
                    var copy1 = e.Clone();
                    Header?.Events.Rise(copy1);
                    break;
                case KeyType.Digit:
                    Debug.Assert(e.Key.Value.HasValue);
                    SelectedIndex = int.Parse(e.Key.Value.Value.ToString()) - 1;
                    e.IsHandled = true;
                    if (SelectedIndex == 2 && SelectedItem != null) 
                    {
                        SelectedItem.IsFocused = true;
                        Events.Rise(new ValueEditingProcessEvent(SelectedItem));
                    }
                    else
                    {
                        var copy = e.Clone();
                        SelectedItem?.Events.Rise(copy);    
                    }
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

    private void OnValueEditedEvent(ValueEditedEvent e)
    {
        var value = _exValueValidator(e.Value);
        WriteExValue(value);
        _exSetCallback.Invoke(value);
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
