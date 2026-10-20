# Property pages

Asv.Hal includes ready-made pages for numeric readouts, preset values, and enum
selection. Build them before attaching them to a window. Use at least four
character rows for the page layouts in these examples.

## Display and edit numeric values

The following helper creates a readout and an editor. The caller owns the pages
and can attach either one with `window.GoTo(page)`, or place both in a populated
`PageSlider`:

```C#
using System.Globalization;
using Asv.Hal;

public static class NumericPageExample
{
    public static PropertyReaderPage CreateReadout()
    {
        return new PropertyReaderPage(
            "Sensor ", "ON", "OFF", "Temperature, C", "F1")
        {
            PropertyValue = 22.5
        };
    }

    public static PropertyWriterPage CreateEditor(Action<double> setValue)
    {
        var page = new PropertyWriterPage(
            header: "Output ",
            trueText: "ON",
            falseText: "OFF",
            propertyName: "Level",
            predefinedValues: new double[] { 10, 20, 30 },
            min: 0,
            max: 100,
            stringFormat: value => value.ToString("F1", CultureInfo.InvariantCulture),
            setCallback: setValue);
        page.ExternalUpdateValue(20.0);
        return page;
    }
}
```
{noinject="true"}

`PropertyReaderPage` formats values with invariant culture. A `double.NaN` value
renders as an empty numeric field. Its property setter ignores differences below
0.01. The readout is numeric; the header's ON/OFF switch is a separate boolean
state and does not itself turn hardware on or off.

For a focused `PropertyWriterPage`:

| Key | Action |
|-----|--------|
| 1–9 | Choose an available preset |
| Up / Down | Cycle forward / backward through presets |
| 0 | Start entering a custom numeric value |
| Enter while editing | Finish entry and apply a parsed value |
| Left while editing | Delete the final character |
| Escape while editing | Leave editing and restore the stored edit text |

Custom numeric entry is clamped to `min` and `max` when applied. Presets and
programmatic assignments go directly through `PropertyValue` and are not clamped
by that entry handler, so supply valid values yourself. The current setter
ignores changes smaller than 0.5; choose this component only when that resolution
fits the parameter.

Keep presets non-empty: cycling uses the nearest preset and assumes at least
one value exists. `ExternalUpdateValue(double)` updates a numeric writer without
calling `setCallback`, which is useful when reflecting an external device state.
Assigning `PropertyValue` directly can invoke the callback. External telemetry
updates must be marshalled onto `Window.UiTaskFactory`; request a render when a
page's setter does not already request one for its current focus state.

## Readout preview

With `PropertyValue = 22.5`, `F1` formatting, and `ExternalUpdateValue(true)`,
the readout appears as follows on a 32-column, four-row screen:

![PropertyReaderPage with the Sensor header ON and temperature 22.5](display-status.jpg){width="700"}

The [Display output gallery](display-examples.md) includes the exact text output
and a runnable `status` example.

## Select an enum value

Use enum values with distinct numeric values, and select a value actually defined
in the enum. This helper creates a page with a separate ON/OFF header and an enum
selector:

```C#
using Asv.Hal;

public enum DisplayMode
{
    Summary,
    Details
}

public static class EnumPageExample
{
    public static EnumPropertyEditor<DisplayMode> Create(Action<DisplayMode> apply)
    {
        return new EnumPropertyEditor<DisplayMode>(
            header: "Display ",
            trueText: "ON",
            falseText: "OFF",
            enumHeader: "Mode",
            defaultValue: DisplayMode.Summary,
            nameGetter: mode => mode == DisplayMode.Summary ? "Summary" : "Details",
            setCallback: apply);
    }
}
```
{noinject="true"}

The page uses `SingleComboBox<TValue>` internally. Up/Down cycle enum values;
the digit **1** advances to the next value. These operations apply changes
immediately. They are not a separate confirmation dialog.

Unlike the numeric writer, `EnumPropertyEditor.ExternalUpdateValue(TValue)`
sets the child selector's `Value` and can invoke `setCallback` when the value
changes. Guard against feedback loops when synchronizing enum state with a
device. `ExternalUpdateValue(bool)` changes the ON/OFF header state separately.

Callbacks run in routed event processing. Queue slow device work to your
application's background processing and marshal its completion back to the
window; see [Controls and events](controls-events.md).

API source: [PropertyReaderPage](https://github.com/asv-soft/asv-hal/blob/main/src/Asv.Hal/Display/CustomControls/Page/PropertyReaderPage.cs),
[PropertyWriterPage](https://github.com/asv-soft/asv-hal/blob/main/src/Asv.Hal/Display/CustomControls/Page/PropertyWriterPage.cs),
[EnumPropertyEditor](https://github.com/asv-soft/asv-hal/blob/main/src/Asv.Hal/Display/CustomControls/Page/EnumPropertyEditor.cs).
