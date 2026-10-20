# Screens and rendering

Asv.Hal renders a grid of character cells. `Size.Width` is a column count and
`Size.Height` is a row count; coordinates start at `(0, 0)` in the top-left cell.
Choose positive dimensions that fit the device.

## Rendering interfaces

`IRenderContext` exposes `Size` and `Write(x, y, char)`. It also supplies default
methods for writing strings, filling cells, and creating cropped contexts.
`IScreen` adds `BeginRenderLoop()` and diagnostic output methods.

| Implementation | Output |
|----------------|--------|
| ConsoleScreen | A bordered character grid in the terminal |
| BufferScreen | Abstract frame buffer for implementing a device |
| CompositeScreen | The same frame sent to multiple screens of identical size |
| DataStreamScreen | Windows-1251 character frame over an Asv.IO port |
| TelnetScreen | UTF-8 by default, ANSI cursor positioning, escaped Telnet data |

`ConsoleScreen`, `DataStreamScreen`, and `TelnetScreen` live in `Asv.Hal.Impl`.
The interfaces, `BufferScreen`, and `CompositeScreen` live in `Asv.Hal`.

## Draw a frame directly

This complete example runs without hardware. Use an `IScreen` variable to access
the interface's default helper methods. Disposing the render-loop token flushes
the completed frame; starting a `BufferScreen` render loop clears its buffer.

```C#
using Asv.Hal;
using Asv.Hal.Impl;

IScreen screen = new ConsoleScreen(new Size(20, 4));
using (screen.BeginRenderLoop())
{
    screen.WriteString(0, 0, "Device ready");
    screen.FillChar(0, 1, 20, '-');
    IRenderContext details = screen.Crop(2, 2, 16, 1);
    details.WriteString(0, 0, "Voltage: 12 V");
}
```
{noinject="true"}

`Crop` translates local coordinates and clips writes to the subcontext. Keep
text within the available width. The default `WriteString` implementation checks
the starting position and then calls `Write` for each character, so a custom
context must also bound-check individual writes.

Default interface members are called through an interface reference; see the
[C# interface documentation](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/interfaces/explicit-interface-implementation).

## Example output

The direct-frame example above produces this 20-column, four-row frame:

<code-block lang="text" src="DisplayExamples/output/frame.txt"/>

![ConsoleScreen output for the direct frame example](display-frame.jpg){width="700"}

The `LineScreen` example below prints two rows without a border:

```text
Ready
Count: 3
```

Both rows contain trailing spaces up to 12 columns. See the
[Display output gallery](display-examples.md) for more screenshots and the
capture procedure.

## Implement a screen

Derive from `BufferScreen` to reuse cell storage, bounds checks, frame clearing,
and frame flushing. The following complete program prints two rows without
requiring terminal cursor control:

```C#
using Asv.Hal;

IScreen screen = new LineScreen(new Size(12, 2));
using (screen.BeginRenderLoop())
{
    screen.WriteString(0, 0, "Ready");
    screen.WriteString(0, 1, "Count: 3");
}

sealed class LineScreen(Size size) : BufferScreen(size)
{
    protected override void InternalRender(char[,] buffer)
    {
        for (var y = 0; y < Size.Height; y++)
        {
            var row = new char[Size.Width];
            for (var x = 0; x < Size.Width; x++)
            {
                row[x] = buffer[y, x];
            }
            Console.WriteLine(new string(row));
        }
    }
}
```
{noinject="true"}

The buffer index order is `[row, column]`, or `[y, x]`. Consume or copy the buffer
inside `InternalRender`; do not retain the mutable array for later use. This
method runs synchronously while the buffer lock is held. A slow device write
therefore delays the render loop.

When a `Window` owns rendering, let it open and finish render loops. Do not run a
second render loop against the same screen concurrently.

## Mirror output

Supply at least one screen to `CompositeScreen`. Every child must have exactly
the same `Size`; mismatched sizes throw an exception. Writes and diagnostics are
forwarded to each child, and disposing the combined render token flushes them.
See [Remote and serial displays](remote-displays.md) for a console/Telnet example.

API source: [IRenderContext](https://github.com/asv-soft/asv-hal/blob/main/src/Asv.Hal/Display/Screen/IRenderContext.cs),
[BufferScreen](https://github.com/asv-soft/asv-hal/blob/main/src/Asv.Hal/Display/Screen/Impl/BufferScreen.cs).
