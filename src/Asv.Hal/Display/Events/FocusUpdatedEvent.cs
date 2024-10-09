namespace Asv.Hal;

public class FocusUpdatedEvent(Control sender, Control target,RoutingStrategy strategy)
    :RoutedEvent(sender,strategy)
{
    public Control Target { get; } = target;
}