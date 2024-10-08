namespace Asv.Hal;


public class KeyDownEvent(object sender, KeyValue key, RoutingStrategy strategy = RoutingStrategy.Tunnel) : RoutedEvent(sender,strategy)
{
    public KeyValue Key { get; } = key;
}

