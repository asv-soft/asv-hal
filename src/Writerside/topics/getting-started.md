# Getting started

Build a small console menu before connecting physical hardware. This example uses
`ConsoleScreen` and `ConsoleKeyboard`; it does not open GPIO pins or serial ports.

## Create a project

Install the .NET 10 SDK, then create a console application:

```bash
dotnet new console -n HalDemo -f net10.0
cd HalDemo
dotnet add package Asv.Hal
```

The examples in this guide use the API in the current repository. When working
with an unreleased checkout, reference `src/Asv.Hal/Asv.Hal.csproj` instead of a
published package if their APIs differ.

## Show a menu

Replace `Program.cs` with the following complete example:

```C#
using System.Globalization;
using Asv.Hal;
using Asv.Hal.Impl;

using var stopped = new ManualResetEventSlim();
using var keyboard = new ConsoleKeyboard();
using var counter = new TextBlock("Count: 0");
var count = 0;
using var increment = new Button(onClick: _ =>
{
    counter.Text = $"Count: {++count}";
})
{
    Content = counter
};
using var exit = new Button("Exit", _ => stopped.Set());
using var menu = new MenuPanel("HAL demo", HorizontalPosition.Center)
{
    Items = { increment, exit }
};
using var window = new Window(
    TimeProvider.System,
    TimeSpan.FromMilliseconds(100),
    keyboard,
    new ConsoleScreen(new Size(32, 4)),
    CultureInfo.InvariantCulture);

void OnCancel(object? sender, ConsoleCancelEventArgs args)
{
    args.Cancel = true;
    stopped.Set();
}

Console.CancelKeyPress += OnCancel;
try
{
    window.GoTo(menu);
    stopped.Wait();
}
finally
{
    Console.CancelKeyPress -= OnCancel;
}
```
{noinject="true"}

Run `dotnet run` in an interactive terminal with at least 34 columns available
for the 32-character screen and its border. The first menu item is selected
initially. Press **Enter** to increment the counter, **Down Arrow** followed by
**Enter** to exit, or **Ctrl+C** to stop.

`Window` connects keyboard input to routed events, owns the animation timer and
UI scheduler, and renders on a timer tick when a render has been requested.
`GoTo` queues a page change; it does not synchronously attach the page.

## Output preview

After three Enter activations, the counter reads `Count: 3`. This screenshot
shows the same control tree rendered once by the gallery sample; its first
`Rendered: 0` diagnostic is independent of the counter value.

![The HAL demo menu after three counter activations](display-menu.jpg){width="700"}

See the [Display output gallery](display-examples.md) for the captured text and
commands to reproduce this frame.

## Keyboard shortcuts

| Key | Behavior in a focused MenuPanel |
|-----|--------------------------------|
| Up / Down | Select the previous / next item |
| Enter | Give focus to the selected item; a Button invokes its callback |
| 1–9 | Select and activate the corresponding item when present |
| Right | Select the last item |
| Left | Select the first item and raise LostFocusEvent |

A `Button` runs its callback when it receives focus. Avoid setting a button's
`IsFocused` property just to decorate it: doing so activates it.

## Lifetime and threading

Keep the window alive while the application is running. Dispose it before
releasing the keyboard, subscriptions, controls, or transport objects it uses.
The `using` declarations above unwind in reverse order.

`Window` disposes its own timer and subscriptions. It does not dispose the
supplied keyboard or screen. `CompositeScreen` and `CompositeKeyboard` do not
own their underlying devices either. Removing a control from a panel or
navigating away detaches it; it does not dispose it. Keep reusable pages alive
and dispose application-owned controls and subscriptions at shutdown.

Build control trees before attaching them. For changes arriving from background
work, use `Window.UiTaskFactory`; see [Controls and events](controls-events.md).
`ConsoleKeyboard` uses a background `Console.ReadKey` loop, so it requires
interactive input and cannot serve as an awaited, cancellable input reader.

The Ctrl+C handler sets `Cancel` and releases the main wait so normal cleanup can
run, following the [.NET Console.CancelKeyPress contract](https://learn.microsoft.com/en-us/dotnet/api/system.console.cancelkeypress?view=net-10.0).

## Next steps

- [Screens and rendering](screens.md): draw directly or implement an output device.
- [Keyboard input](keyboards.md): consume input or provide your own keyboard.
- [Controls and events](controls-events.md): navigate and update controls.
- [Remote and serial displays](remote-displays.md): connect stream-based devices.
- [GPIO and LEDs](gpio-leds.md): configure pins and play LED patterns.

API source: [Window](https://github.com/asv-soft/asv-hal/blob/main/src/Asv.Hal/Display/Controls/Window.cs).
