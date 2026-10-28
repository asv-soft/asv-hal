# Remote and serial displays

Stream-based screens use Asv.IO ports. Create the port, create the input/output
adapters, and enable the port before displaying a page. Keep the port alive until
the window and keyboard subscriptions have been disposed.

## Mirror a console to a Telnet terminal

The following complete program listens on loopback TCP port 7344. It uses one
`CompositeScreen` for output and one `CompositeKeyboard` for local/remote input.
Run it in an interactive terminal, then connect a Telnet-capable terminal client
to `127.0.0.1:7344`. Press **Enter** in the remote client to produce a key and
request the first frame. The example exits after one minute.

```C#
using System.Globalization;
using Asv.Hal;
using Asv.Hal.Impl;
using Asv.IO;
using R3;

var port = PortFactory.Create("tcp://127.0.0.1:7344?srv=true")
    ?? throw new InvalidOperationException("TCP port was not created.");
using var portLifetime = port as IDisposable;
using var localKeyboard = new ConsoleKeyboard();
using var remoteKeyboard = new TelnetKeyboard(port);
var size = new Size(32, 4);
IScreen screen = new CompositeScreen(
    new ConsoleScreen(size),
    new TelnetScreen(port, size));
IKeyboard keyboard = new CompositeKeyboard(localKeyboard, remoteKeyboard);
using var page = new TextBlock("Local and remote", HorizontalPosition.Center);
using var window = new Window(
    TimeProvider.System,
    TimeSpan.FromMilliseconds(100),
    keyboard,
    screen,
    CultureInfo.InvariantCulture);
using var redraw = keyboard.OnKeyPress.Subscribe(_ =>
{
    window.UiTaskFactory.StartNew(() =>
        page.Events.Rise(new RenderRequestEvent(page)));
});

port.Enable();
window.GoTo(page);
await Task.Delay(TimeSpan.FromMinutes(1));
```
{noinject="true"}

The `R3` import provides the `Subscribe` extension. Telnet output uses
UTF-8 by default and ANSI cursor positioning. `TelnetKeyboard` handles terminal
key sequences and Telnet option commands. A terminal client must support these
sequences and use the same encoding. Telnet traffic is unencrypted; this example
binds to loopback for local development.

The window renders on request, rather than continuously transmitting frames. A
client connecting after the first render may not see that frame. The subscription
above explicitly requests a redraw after keyboard input. In a production host,
request a redraw when a client connects if the transport exposes that lifecycle.

## Device-specific character protocol

Use `DataStreamScreen` only for a display that expects its exact frame format:

1. Byte `0x0A`.
2. `Width × Height` characters in Windows-1251, in row-major order.
3. Byte `0x0D`.

This is not an ANSI terminal protocol or a generic LCD driver. The adapter does
not negotiate the display size or character encoding. Use `DataStreamKeyBoard`
only if the input side implements that keyboard adapter's device protocol.

The following helper renders one frame to an already configured and enabled
serial port; the caller owns the port:

```C#
using Asv.Hal;
using Asv.Hal.Impl;
using Asv.IO;

public static class SerialDisplayExample
{
    public static void ShowReady(IPort enabledPort)
    {
        IScreen screen = new DataStreamScreen(enabledPort, new Size(20, 4));
        using (screen.BeginRenderLoop())
        {
            screen.WriteString(0, 0, "Device ready");
        }
    }
}
```
{noinject="true"}

Choose the connection string, baud rate, pin wiring, and device protocol from the
actual hardware. Do not copy the development shell's physical port defaults into
a deployment. Stream output is synchronous during frame flushing; transport
latency or failures can delay rendering.

API source: [TelnetScreen](https://github.com/asv-soft/asv-hal/blob/main/src/Asv.Hal/Display/Screen/Impl/TelnetScreen.cs),
[DataStreamScreen](https://github.com/asv-soft/asv-hal/blob/main/src/Asv.Hal/Display/Screen/Impl/DataStreamScreen.cs),
[TelnetKeyboard and DataStreamKeyBoard](https://github.com/asv-soft/asv-hal/blob/main/src/Asv.Hal/Display/Keyboard/Impl/DataStreamKeyBoard.cs).
