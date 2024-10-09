namespace Asv.Hal;


public enum RoutingStrategy
{
    Direct,
    Bubble,
    Tunnel
}
public class RoutedEvent(object sender, RoutingStrategy strategy)
{
    public object Sender { get; } = sender;
    public RoutingStrategy Strategy { get; } = strategy;
    public bool IsHandled { get; set; }
   
}