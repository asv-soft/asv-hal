namespace Asv.Hal;

public class ButtonClickEvent(Button sender) :RoutedEvent(sender, RoutingStrategy.Bubble)
{
    public Button Button { get; } = sender;
}