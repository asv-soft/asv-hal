namespace Asv.Hal;

public class SelectionChangedEvent(Control sender, Control item) : RoutedEvent(sender, RoutingStrategy.Bubble)
{
    public Control Item { get; } = item;
}