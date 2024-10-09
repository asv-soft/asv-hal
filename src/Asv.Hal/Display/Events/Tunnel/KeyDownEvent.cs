namespace Asv.Hal;


public class KeyDownEvent(Control sender, KeyValue key)
    : RoutedEvent(sender,RoutingStrategy.Tunnel)
{
    public KeyValue Key { get; } = key;
}

