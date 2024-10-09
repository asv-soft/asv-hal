namespace Asv.Hal;

public class ValueChangedEvent(Control sender, Control target, string text) : RoutedEvent(sender,RoutingStrategy.Tunnel)
{
    public Control Target { get; } = target;
    public string Text { get; } = text;
}