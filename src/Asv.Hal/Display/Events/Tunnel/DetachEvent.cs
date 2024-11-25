namespace Asv.Hal;

internal class DetachEvent(Control sender) : RoutedEvent(sender,RoutingStrategy.Tunnel);