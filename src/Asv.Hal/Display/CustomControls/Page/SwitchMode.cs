using System.Text;
using System.Text.RegularExpressions;

namespace Asv.Hal;

public class SwitchMode : InfoPage
{
    private enum ChangeModeState
    {
        JustNow,
        InProgress,
        Complete
    }
    
    private readonly TimeSpan _switchModeDelay;
    private readonly string _onValue;
    private readonly string _offValue;
    private readonly TextBlock _value;
    private readonly Regex _reg = new(@" {2,}");
    private ChangeModeState _state = ChangeModeState.Complete;
    private long _startTime;

    public bool IsEnabled
    {
        get => string.Equals(_onValue, _value.Text);
        set
        {
            _state = ChangeModeState.JustNow;
            _value.Text = value ? _onValue : _offValue;
        }
    }

    public SwitchMode(string modeName, string onValue, string offValue, bool defaultValue, int width,
        TimeSpan switchModeDelay) : base(string.Empty)
    {
        _switchModeDelay = switchModeDelay;
        _onValue = !string.IsNullOrWhiteSpace(onValue) ? onValue[..Math.Min(onValue.Length,width)] : "On"[..Math.Min("On".Length,width)];
        _offValue = !string.IsNullOrWhiteSpace(offValue) ? offValue[..Math.Min(offValue.Length,width)] : "Off"[..Math.Min("Off".Length,width)];
        _value = new TextBlock(defaultValue ? _onValue : _offValue, HorizontalPosition.Center);
        if (string.IsNullOrWhiteSpace(modeName))
        {
            Header = new TextBlock("Mode"[..Math.Min("Mode".Length,width)]);
            Items.Add(_value);
            return;
        }

        modeName = _reg.Replace(modeName.Trim(), " ");
        var modes = modeName.Split(' ');
        if (modes.Length == 1)
        {
            Header = new TextBlock(modes[0][..Math.Min(modes[0].Length,width)]);
            Items.Add(_value);
            return;
        }

        var index = 0;
        var builder = new StringBuilder();
        builder.Append(modes[index++]);
        while (index < modes.Length)
        {
            if (builder.Length >= width - 1 || builder.Length + modes[index].Length > width - 1) break;
            builder.Append(' ');
            builder.Append(modes[index++]);
        }

        var v = builder.ToString();
        Header = new TextBlock(v[..Math.Min(v.Length,width)]);
        builder.Clear();

        if (index < modes.Length)
        {
            builder.Append(modes[index++]);
            while (index < modes.Length)
            {
                if (builder.Length >= width - 1 || builder.Length + 1 + modes[index].Length > width)
                {
                    v = builder.ToString();
                    Items.Add(new TextBlock(v[..Math.Min(v.Length,width)]));
                    builder.Clear();
                    builder.Append(modes[index++]);
                    continue;
                }
                builder.Append(' ');
                builder.Append(modes[index++]);
            }

            if (builder.Length > 0)
            {
                v = builder.ToString();
                Items.Add(new TextBlock(v[..Math.Min(v.Length,width)]));
                builder.Clear();
            }
        }
        Items.Add(_value);
    }

    protected override void InternalOnEvent(RoutedEvent e)
    {
        base.InternalOnEvent(e);
        if (e is not AnimationTickEvent anim) return;
        switch (_state)
        {
            case ChangeModeState.JustNow:
                _startTime = anim.TimeProvider.GetTimestamp();
                _state = ChangeModeState.InProgress;
                break;
            case ChangeModeState.InProgress:
                if (anim.TimeProvider.GetElapsedTime(_startTime).TotalMicroseconds >=
                    _switchModeDelay.TotalMicroseconds)
                {
                    _state = ChangeModeState.Complete;
                    Event(new SetModeCompleteEvent(this, IsEnabled));
                }
                break;
            case ChangeModeState.Complete:
                break;
        }
    }
}