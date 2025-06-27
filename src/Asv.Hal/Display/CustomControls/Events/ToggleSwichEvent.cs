namespace Asv.Hal;


public class ToggleSwichEvent(Control sender, bool onOff) : RoutedEvent(sender, RoutingStrategy.Bubble)
{
    public bool Value { get; } = onOff;
}
