
using System.Globalization;

namespace Asv.Hal;

public class DmeChannelEditor<TValue> : PropertyEditor where TValue : struct, Enum 
{
    private readonly int? _chNumDefaultValue;
    private readonly Func<int?, int?> _chNumValidator;
    private readonly Func<TValue, int?, double> _vorFreqGetter;
    private readonly Func<double, double> _vorFreqValidator;
    private readonly Func<double, (TValue, int)> _channelModeGetter;
    private readonly Action<(TValue mode, int? num)> _setChModeCallback;
    private readonly double _vorFreqDefVal;

    public DmeChannelEditor(string header, string trueText, string falseText,
        string? chModeTitle, TValue chModeDefaultValue, 
        string? chNumTitle, int? chNumDefaultValue, Func<int?, int?> chNumValidator,
        string? vorFreqTitle, Func<TValue, int?, double> vorFreqGetter, Func<double, double> vorFreqValidator,
        Func<double, (TValue, int)> channelModeGetter,
        Func<TValue, string>? chModeNameGetter = null,
        Action<(TValue mode, int? num)>? setChModeCallback = null) : base(null)
    {
        Header = new ToggleSwitch(header, trueText, falseText);
        var channelMode = new ComboBox<TValue>(chModeTitle, chModeNameGetter);
        channelMode.Value = chModeDefaultValue;
        Items.Add(channelMode);

        _chNumDefaultValue = chNumDefaultValue;
        _chNumValidator = chNumValidator;
        _vorFreqGetter = vorFreqGetter;
        _vorFreqValidator = vorFreqValidator;
        _channelModeGetter = channelModeGetter;
        _setChModeCallback = setChModeCallback ?? (_ => { });

        var chNumDef = _chNumValidator(chNumDefaultValue);
        var chNum = new TextBox(chNumTitle) { Text = chNumDef != null ? $"{chNumDef:000}" : string.Empty };
        Items.Add(chNum);

        _vorFreqDefVal = _vorFreqGetter(channelMode.Value, chNumDefaultValue);
        var vorFreq = new TextBox(vorFreqTitle, "MHz")
        {
            Text = !double.IsNaN(_vorFreqDefVal)
                ? (_vorFreqDefVal / 1_000_000.0).ToString("F2", CultureInfo.InvariantCulture)
                : string.Empty
        };
        Items.Add(vorFreq);
    }

    public void ExternalUpdateValue(bool onOff)
    {
        ((ToggleSwitch)Header!).Value = onOff;
    }

    public void ExternalUpdateValue(TValue channel, int? number, double vorFreq)
    {
        WriteChannelValue(channel);
        WriteChannelNumberValue(number);
        WriteVorFrequencyValue(vorFreq);
    }
    
    protected override void InternalOnEvent(RoutedEvent e)
    {
        base.InternalOnEvent(e);
        
        if (e is EnumValueEditedEvent<TValue> enEv)
        {
            var mode = enEv.Value;
            var num = ReadChannelNumberValue();
            var vorFreq = _vorFreqGetter(mode, num);
            WriteVorFrequencyValue(vorFreq);
            _setChModeCallback.Invoke((mode, num));
        }
        
        if (e is ValueEditedEvent rrr)
        {
            if (rrr.Sender == Items[1])
            {
                var value = ValidateAndGetChannelNumberValue(rrr.Value);
                WriteChannelNumberValue(value);
                var mode = ReadChannelValue();
                var vorFreq = _vorFreqGetter(mode, value);
                WriteVorFrequencyValue(vorFreq);
                _setChModeCallback.Invoke((mode, value));
            }
            if (rrr.Sender == Items[2])
            {
                var value = ValidateAndGetVorFrequencyValue(rrr.Value);
                if (double.IsNaN(value)) return;
                WriteVorFrequencyValue(value);
                var channel = _channelModeGetter(value);
                WriteChannelValue(channel.Item1);
                WriteChannelNumberValue(channel.Item2);
                _setChModeCallback.Invoke(channel);
            }
            // Event(rrr);
        }
    }

    private TValue ReadChannelValue()
    {
        return ((ComboBox<TValue>)Items[0]).Value;
    }
    
    private void WriteChannelValue(TValue value)
    {
        ((ComboBox<TValue>)Items[0]).Value = value;
    }
    
    private int? ReadChannelNumberValue()
    {
        return int.TryParse(((TextBox)Items[1]).Text, out var v)
            ? v
            : null;
    }
    
    private void WriteChannelNumberValue(int? value)
    {
        ((TextBox)Items[1]).Text = value != null ? $"{value:000}" : string.Empty;
    }
    
    private double ReadVorFrequencyValue()
    {
        return double.TryParse(((TextBox)Items[2]).Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var v)
            ? v
            : double.NaN;
    }
    
    private void WriteVorFrequencyValue(double value)
    {
        ((TextBox)Items[2]).Text =
            !double.IsNaN(value) ? (value / 1_000_000.0).ToString("F2", CultureInfo.InvariantCulture) : string.Empty;
    }
    private int? ValidateAndGetChannelNumberValue(string? chNumString)
    {
        return double.TryParse(chNumString, NumberStyles.Any, CultureInfo.InvariantCulture, out var v)
            ? _chNumValidator((int)Math.Round(v))
            : null;
    }
    
    private double ValidateAndGetVorFrequencyValue(string? vorFreqString)
    {
        return double.TryParse(vorFreqString, NumberStyles.Any, CultureInfo.InvariantCulture, out var v)
            ? _vorFreqValidator(v * 1_000_000.0)
            : double.NaN;
    }

    
}