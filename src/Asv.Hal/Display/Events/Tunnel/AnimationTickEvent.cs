namespace Asv.Hal;

internal class AnimationTickEvent(Control sender, TimeProvider timeProvider) : RoutedEvent(sender,RoutingStrategy.Tunnel)
{
    public TimeProvider TimeProvider { get; } = timeProvider;
}