# Keyboard input

`IKeyboard.OnKeyPress` is an R3 `Observable<KeyValue>`. Each value has a `Type`
and an optional character `Value`. Digits carry their digit character; `Dot`
carries `'.'`; navigation keys usually have no character.

## Choose an implementation

| Implementation | Input |
|----------------|-------|
| ConsoleKeyboard | Interactive terminal keys read with Console.ReadKey |
| TelnetKeyboard | Telnet commands and terminal key sequences from IDataStream |
| DataStreamKeyBoard | Device-specific bytes from IDataStream or a connection string |
| CompositeKeyboard | Merged input streams from multiple IKeyboard instances |
| KeyboardBase | Base class for implementing a keyboard |

The `DataStreamKeyBoard` spelling is part of the current API. It is a different
protocol adapter from `TelnetKeyboard`; choose it only when the connected device
sends the bytes that implementation expects.

## Provide input without a terminal

Derive from `KeyboardBase` and expose methods for your device callbacks. This
complete example emits two keys synchronously, making it suitable for a desktop
button adapter or a hardware-free demonstration:

```C#
using Asv.Hal;
using R3;

using var keyboard = new DeviceKeyboard();
using var subscription = keyboard.OnKeyPress.Subscribe(key =>
    Console.WriteLine($"{key.Type}: {key.Value}"));

keyboard.Enter();
keyboard.Digit('7');

sealed class DeviceKeyboard : KeyboardBase
{
    public void Enter() => RiseEnterEvent();

    public void Digit(char value)
    {
        if (value is < '0' or > '9')
        {
            throw new ArgumentOutOfRangeException(nameof(value));
        }
        RiseDigitEvent(value);
    }
}
```
{noinject="true"}

Expected output:

```text
Enter:
Digit: 7
```

Other protected helpers include `RiseEscapeEvent`, `RiseFunctionEvent`, the four
arrow-key helpers, and `RiseDotEvent`. The `Rise` spelling is intentional.

Dispose R3 subscriptions when their consumer stops, as described in the
[R3 documentation](https://github.com/Cysharp/R3#subscription).
`Window` subscribes to the supplied keyboard and queues key handling onto its
own UI scheduler. Do not add a second `Console.ReadKey` loop while using
`ConsoleKeyboard`.

## Console key mapping

`ConsoleKeyboard` maps Enter, F1, arrows, digits, and decimal separators.
Both comma and period produce `KeyType.Dot` with `'.'`. Backspace, Delete, and
Escape all produce `KeyType.Escape`; they are not distinct delete operations.
Other keys are ignored. See [Controls and events](controls-events.md) for the
numeric editor's deletion behavior.

## Device keyboard protocol

`DataStreamKeyBoard` decodes ASCII bytes with the following mapping:

| Byte / character | KeyType |
|------------------|---------|
| `0`–`9` | Digit with the corresponding character |
| `a` | Dot |
| `b` | DownArrow |
| `c` | Enter |
| `d` | Escape |
| `e` | Function |
| `f` | LeftArrow |
| `g` | RightArrow |
| `h` | UpArrow |

Other input is ignored. Use the `IDataStream` constructor when the application
owns the connection: create and enable the port explicitly, then dispose the
keyboard before the port. The connection-string constructor creates/enables a
port internally but does not register that port for disposal with the keyboard.

## Combine keyboards

Construct `new CompositeKeyboard(localKeyboard, remoteKeyboard)` to pass both
sources to one window. The composite merges events without taking ownership of
its inputs. Keep both keyboards alive until the window has been disposed, then
dispose each concrete keyboard yourself.

API source: [IKeyboard](https://github.com/asv-soft/asv-hal/blob/main/src/Asv.Hal/Display/Keyboard/IKeyboard.cs),
[KeyboardBase](https://github.com/asv-soft/asv-hal/blob/main/src/Asv.Hal/Display/Keyboard/Impl/KeyboardBase.cs).
