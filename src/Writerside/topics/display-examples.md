# Display output gallery

These examples run the actual Asv.Hal controls and `ConsoleScreen` without opening
GPIO pins, serial ports, or network connections. Each example renders one final
frame and exits, so its output is repeatable.

The screenshots are browser previews of the captured console text. They preserve
the output characters and spacing; the surrounding card is a documentation
presentation, not a device UI. They are not photographs of a physical LCD. The terminal card intentionally
uses the same colors in both documentation themes.

## Run the examples

From the repository root, build the supplied example project once:

```bash
dotnet build src/Writerside/examples/DisplayExamples/DisplayExamples.csproj -c Release
```

Then select an example with `frame`, `menu`, `status`, or `controls`:

```bash
dotnet run --project src/Writerside/examples/DisplayExamples/DisplayExamples.csproj -c Release --no-build --no-restore -- frame
```

`Rendered: 0` is a diagnostic line emitted by `ConsoleScreen` below its border.
It identifies the first rendered frame, not a menu selection or a button count.
These static examples render a control tree directly. For an interactive window
with keyboard input and periodic rendering, use [Getting started](getting-started.md).

## Direct frame

The `frame` example uses the exact drawing operations from
[Screens and rendering](screens.md): text on the first row, a separator on the
second, and a cropped region starting at column 2 on the third.

<code-block lang="text" src="DisplayExamples/output/frame.txt"/>

![A 20 by 4 ConsoleScreen frame with Device ready and Voltage: 12 V](display-frame.jpg){width="700"}

## Menu after three activations

Select `menu` instead of `frame` in the run command. The program activates the
counter button three times through its focus callback, then renders the menu
once. The first item remains selected and its label reads `Count: 3`.
In the interactive example, press Enter three times to obtain the same menu
content; its render diagnostics can differ.

<code-block lang="text" src="DisplayExamples/output/menu.txt"/>

![A 32 by 4 menu with Count: 3 selected and an Exit item](display-menu.jpg){width="700"}

## Numeric readout

Select `status` to render `PropertyReaderPage` with a value of 22.5 and format
`F1`. The sample explicitly calls `ExternalUpdateValue(true)` to put its header
switch in the ON state; this boolean is separate from the numeric value.

<code-block lang="text" src="DisplayExamples/output/status.txt"/>

![A numeric readout showing Sensor ON and a centered temperature of 22.5](display-status.jpg){width="700"}

See [Property pages](property-pages.md) for numeric and enum editor examples.

## Text, toggle, and progress

Select `controls` for an `InfoPage` containing a `TextBlock`, `ToggleSwitch`, and
`Progress`. The progress value is `0.65`, rather than `65`. With a 32-column
screen the bar has 30 inner cells, of which 19 are filled after truncation.

<code-block lang="text" src="DisplayExamples/output/controls.txt"/>

![A six-row status page showing temperature, Output ON, and a 65 percent progress bar](display-controls.jpg){width="700"}

See [Controls and events](controls-events.md) for event handling and updates from
background work.

## Reproduce the screenshots

The screenshot inputs are the text files in
`src/Writerside/examples/DisplayExamples/output`. To refresh one, redirect the
corresponding example output, for example:

```bash
dotnet run --project src/Writerside/examples/DisplayExamples/DisplayExamples.csproj -c Release --no-build --no-restore -- menu > src/Writerside/examples/DisplayExamples/output/menu.txt
```

Serve the example directory locally:

```bash
python3 -m http.server 8772 --bind 127.0.0.1 --directory src/Writerside/examples/DisplayExamples
```

Open `http://127.0.0.1:8772/preview.html?example=menu` in a browser. Other supported
values are `frame`, `status`, and `controls`. Capture the entire dark output card,
including its title and footer, then save the image in `src/Writerside/images`.
Save an identical copy with the `_dark.jpg` suffix for Writerside's dark theme
(for example, `display-menu_dark.jpg`). Repeat for every changed example, and
verify that the text blocks and images still agree. Stop the local server after capturing.

Writerside includes the text below directly from the same source file used to
build the example project, so the displayed C# is not a separate copy.

## Example source

<code-block lang="C#" src="DisplayExamples/Program.cs" noinject="true"/>
