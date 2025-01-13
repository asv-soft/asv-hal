namespace Asv.Hal;

public class ValueEditedEvent(Control sender, string? value) : RoutedEvent(sender,RoutingStrategy.Bubble)
{
    public string? Value { get; } = value;
}


public class EnumValueEditedEvent(Control sender) : RoutedEvent(sender, RoutingStrategy.Bubble);
public class EnumValueEditedEvent<TValue>(Control sender, TValue? value) : EnumValueEditedEvent(sender)
    where TValue : Enum
{
    public TValue? Value { get; } = value;
}