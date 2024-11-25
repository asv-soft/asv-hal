namespace Asv.Hal;

public class ValueEditedEvent(Control sender, string? value) : RoutedEvent(sender,RoutingStrategy.Bubble)
{
    public string? Value { get; } = value;
}

public class EnumValueEditedEvent<TValue>(Control sender, TValue? value) : RoutedEvent(sender, RoutingStrategy.Bubble)
    where TValue : Enum
{
    public TValue? Value { get; } = value;
}