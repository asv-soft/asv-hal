namespace Asv.Hal;

public class SwitchModeCompleteEvent(Control sender, bool isOn) : RoutedEvent(sender, RoutingStrategy.Bubble)
{
    public bool IsOn { get; } = isOn;
}