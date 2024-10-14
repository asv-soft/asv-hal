namespace Asv.Hal;

public class FocusUpdatedEvent(Control sender, Control oldFocus, Control newFocus)
    :RoutedEvent(sender,RoutingStrategy.Tunnel)
{
    public Control OldFocus { get; } = oldFocus;
    public Control NewFocus { get; } = newFocus;
}