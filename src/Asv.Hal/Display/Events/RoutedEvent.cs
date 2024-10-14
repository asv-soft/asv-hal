namespace Asv.Hal;


public enum RoutingStrategy
{
    Bubble,
    Tunnel
}
public abstract class RoutedEvent(Control sender, RoutingStrategy strategy)
{
    public Control Sender { get; } = sender;
    public RoutingStrategy Strategy { get; } = strategy;
    public bool IsHandled { get; set; }
    public virtual RoutedEvent Clone()
    {
        return (RoutedEvent)MemberwiseClone();
    }
    
}