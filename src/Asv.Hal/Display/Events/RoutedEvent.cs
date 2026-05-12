namespace Asv.Hal;

public abstract class RoutedEvent : AsyncRoutedEvent<Control>
{
    protected RoutedEvent(Control sender, RoutingStrategy strategy)
        : base(sender, strategy)
    {
    }
    
    public virtual RoutedEvent Clone()
    {
        return (RoutedEvent)MemberwiseClone();
    }
}
