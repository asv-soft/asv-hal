namespace Asv.Hal;

internal class AnimationTickEvent(object sender, TimeProvider timeProvider) : RoutedEvent(sender,RoutingStrategy.Tunnel)
{
    public TimeProvider TimeProvider { get; } = timeProvider;
}