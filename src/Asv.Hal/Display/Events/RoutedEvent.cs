namespace Asv.Hal;


public enum RoutingStrategy
{
    Direct,
    Bubble,
    Tunnel
}
public class RoutedEvent(Control sender, RoutingStrategy strategy)
{
    public Control Sender { get; } = sender;
    public RoutingStrategy Strategy { get; } = strategy;
    public bool IsHandled { get; set; }
   
}