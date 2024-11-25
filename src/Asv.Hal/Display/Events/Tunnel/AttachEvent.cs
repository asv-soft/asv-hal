namespace Asv.Hal;

internal class AttachEvent(Control sender) : RoutedEvent(sender,RoutingStrategy.Tunnel);