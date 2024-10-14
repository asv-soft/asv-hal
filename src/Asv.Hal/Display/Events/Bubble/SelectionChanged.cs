namespace Asv.Hal;

public class SelectionChangedEvent(Control sender,Control oldItem, Control newItem) : RoutedEvent(sender, RoutingStrategy.Bubble)
{
    public Control? OldItem { get; } = oldItem;
    public Control NewItem { get; } = newItem;
}