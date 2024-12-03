using R3;

namespace Asv.Hal;

public interface IKeyboard
{
    Observable<KeyValue> OnKeyPress { get; }
}

public enum KeyType
{
    Enter,
    Digit,
    Escape,
    Function,
    UpArrow,
    DownArrow,
    LeftArrow,
    RightArrow,
    Dot
}

public class KeyValue(KeyType type, char? value)
{
    public KeyType Type { get; } = type;
    public char? Value { get; } = value;
}