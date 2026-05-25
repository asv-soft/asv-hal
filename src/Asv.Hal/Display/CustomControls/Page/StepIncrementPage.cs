using System.Diagnostics;
using System.Globalization;
using Asv.Common;

namespace Asv.Hal;

public class StepIncrementPage : PropertyEditor
{
    private readonly string _stringFormat;
    private readonly double _defaultValue;
    private readonly double _stepValue;
    private readonly Func<double, double> _valueValidator;
    private readonly Func<double, double> _stepValidator;
    private readonly Action<double>? _setCallback;
    private readonly Action<TextBox, double> _additionFiller;

    public StepIncrementPage(string header,string trueText, string falseText,  
        string valueHeader, string units, string stringFormat,
        double defaultValue, string stepHeader, double stepValue,
        Func<double, double> valueValidator, Func<double, double> stepValidator, 
        TextBox? addition = null, Action<TextBox, double>? additionFiller = null, Action<double>? setCallback = null)
        : base(null, HorizontalPosition.Left, false)
    {
        Events.Catch<LostFocusEvent>(OnLostFocusEvent).DisposeItWith(Disposable);
        Events.Catch<KeyDownEvent>(OnKeyDownEvent).DisposeItWith(Disposable);
        Events.Catch<ValueEditedEvent>(OnValueEditedEvent).DisposeItWith(Disposable);
        Header = new ToggleSwitch(header, trueText, falseText);
        _stringFormat = stringFormat;
        _defaultValue = valueValidator(defaultValue);
        _stepValue = stepValue;
        _valueValidator = valueValidator;
        _stepValidator = stepValidator;
        _setCallback = setCallback ?? (_ => { }); 
        _additionFiller = additionFiller ?? ((_, _) => { });

        var param = new TextBox(valueHeader, units) { Text = valueValidator(defaultValue).ToString(stringFormat, CultureInfo.InvariantCulture) };
        Items.Add(param);
        Items.Add(new TextBox(stepHeader, units) { Text = stepValidator(stepValue).ToString(stringFormat, CultureInfo.InvariantCulture) });
        if (addition == null) return;
        Items.Add(addition);
        _additionFiller(addition, defaultValue);
    }
    
    protected override string GetItemPrefix(int index)
    {
        var prefix = base.GetItemPrefix(index);
        return index < 2 ? base.GetItemPrefix(index) : string.Empty.PadLeft(prefix.Length, ScreenHelper.Empty);
    }

    public void ExternalUpdateValue(bool onOff)
    {
        ((ToggleSwitch)Header!).Value = onOff;
    }
    
    public void ExternalUpdateValue(double value)
    {
        ((TextBox)Items[0]).Text = value.ToString(_stringFormat, CultureInfo.InvariantCulture);
        if (Items.Count > 2)
            _additionFiller.Invoke((TextBox)Items[2], value);
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
                    var copy = e.Clone();
                    e.IsHandled = true;
                    Header?.Events.Rise(copy);
                    break;
                case KeyType.DownArrow:
                    ChangeValue(KeyType.DownArrow);
                    e.IsHandled = true;
                    break;
                case KeyType.UpArrow:
                    ChangeValue(KeyType.UpArrow);
                    e.IsHandled = true;
                    break;
                case KeyType.Digit:
                    Debug.Assert(e.Key.Value.HasValue);
                    SelectedIndex = int.Parse(e.Key.Value.Value.ToString()) - 1;// numbering start with 1
                    e.IsHandled = true;
                    if (SelectedIndex < 2 && SelectedItem != null)
                    {
                        SelectedItem.IsFocused = true;
                        Events.Rise(new ValueEditingProcessEvent(SelectedItem));
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
        if (e.Sender == Items[0])
        {
            var value = _defaultValue;
            if (double.TryParse(e.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var v))
                value = _valueValidator(v);
            ((TextBox)Items[0]).Text = value.ToString(_stringFormat, CultureInfo.InvariantCulture);
            _setCallback?.Invoke(value);
            if (Items.Count > 2)
                _additionFiller.Invoke((TextBox)Items[2], value);    
        }
        if (e.Sender == Items[1])
        {
            var step = _stepValue;
            if (double.TryParse(e.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var s))
                step = s;
            ((TextBox)Items[1]).Text = _stepValidator(step).ToString(_stringFormat, CultureInfo.InvariantCulture);
        }
        //Events.Riserrr);
    }
    
    private void ChangeValue(KeyType key)
    {
        var m = 1;
        if (key == KeyType.DownArrow) m = -1;
        
        var selected = (TextBox)Items[0];
        var increment = _stepValue;
        var step = (TextBox)Items[1];
        if (double.TryParse(step.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var inc))
            increment = _stepValidator(inc);
            
        var selectedValue = _valueValidator(_defaultValue + increment * m);
        if (double.TryParse(selected.Text, NumberStyles.Any, CultureInfo.InvariantCulture,
                out var value))
            selectedValue = _valueValidator(value + increment * m);
            
            
        selected.Text = selectedValue.ToString(_stringFormat, CultureInfo.InvariantCulture);
        _setCallback?.Invoke(selectedValue);
        if (Items.Count > 2)
            _additionFiller.Invoke((TextBox)Items[2], selectedValue);
    }
}

