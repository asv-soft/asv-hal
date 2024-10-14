namespace Asv.Hal;

public class ValueEditedEvent(Control sender, string? value) : RoutedEvent(sender,RoutingStrategy.Bubble)
{
    public string? Value { get; } = value;
}