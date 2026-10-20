# Controls and events

A `Control` has a parent, visual children, focus state, visibility, dimensions,
and a `Render(IRenderContext)` method. Panels attach controls through `Items`;
content controls attach their content through their own properties.

## Choose controls

| Type | Purpose |
|------|---------|
| TextBlock | One line of left-, center-, or right-aligned text |
| Button | Activate a callback on receiving focus |
| Progress | One-line progress bar; Value is a fraction from 0 to 1 |
| ToggleSwitch | Boolean value with customizable ON/OFF text |
| TextBox | Numeric text entry, including a decimal point and optional sign |
| MenuPanel | Select and activate menu items |
| PropertyEditor | List of editable controls, selected with numeric shortcuts |
| PageSlider | Switch between populated pages with Left/Right |

`TextBox` is a numeric editor, not a general-purpose text input. While editing,
**Left Arrow** removes the final character, **Enter** commits, **Escape** restores
the previous value, and **Function** toggles a leading minus sign. With
`ConsoleKeyboard`, Backspace and Delete map to Escape and therefore cancel.

## Control output

An `InfoPage` can combine text, an ON/OFF switch, and a progress bar. In this
32-column, six-row example, `Progress.Value` is `0.65`:

![InfoPage combining a text readout, toggle switch, and progress bar](display-controls.jpg){width="700"}

Run the `controls` example in the [Display output gallery](display-examples.md)
to reproduce the frame and inspect its complete source.

## Navigate between pages

This helper can be used with a `Window` from [Getting started](getting-started.md).
The caller owns the returned menu and the controls placed in it, and keeps the
supplied status page alive for as long as it can be revisited:

```C#
using Asv.Hal;

public static class NavigationExample
{
    public static MenuPanel CreateMenu(Window window, Control statusPage)
    {
        return new MenuPanel("Main menu")
        {
            Items =
            {
                new Button("Status", _ => window.GoTo(statusPage))
            }
        };
    }
}
```
{noinject="true"}

Use `window.GoTo(menu)` to return to the existing menu. `Window.Current` is set
on the UI scheduler; navigation detaches the previous page and attaches the new
one. A detached page is not automatically disposed.

Populate `PageSlider.Items` before attaching it or accessing `SelectedPage`.
Its current implementation indexes the collection directly and requires a page.
Likewise, set list selection only after populating the list.

## Catch routed events

Controls expose `Events.Catch<TEvent>`. For example, observe button activation
from a parent menu rather than passing a callback to every button:

```C#
using Asv.Hal;

public static class EventExample
{
    public static IDisposable Observe(MenuPanel menu, TextBlock status)
    {
        return menu.Events.Catch<ButtonClickEvent>(e =>
        {
            status.Text = "Button activated";
            e.IsHandled = true;
        });
    }
}
```
{noinject="true"}

The caller must dispose the returned subscription. This handler runs as part of
routed event processing, so keep it short. A `Button` first invokes its own
callback and only raises `ButtonClickEvent` if the callback leaves `IsHandled`
false. Do not mark the callback handled when a parent needs to observe the event.

Common events include:

| Event | Routing / purpose |
|-------|-------------------|
| KeyDownEvent | Tunnel: deliver keyboard input through the control tree |
| ButtonClickEvent | Bubble: report activation to ancestors |
| ValueEditedEvent | Bubble: report a numeric text edit |
| ToggleSwichEvent | Bubble: report a ToggleSwitch value; API spelling preserved |
| RenderRequestEvent | Bubble: ask the window to draw a frame |


`AttachEvent` and `DetachEvent` are internal lifecycle events, rather than public
application extension points.

## Update from background work

A window owns a dedicated UI scheduler. Marshal updates from telemetry, timers,
or background tasks with `UiTaskFactory`. This helper returns a task that finishes
after the update has run:

```C#
using Asv.Hal;

public static class BackgroundUpdateExample
{
    public static Task SetStatusAsync(Window window, TextBlock status, string text)
    {
        return window.UiTaskFactory.StartNew(() => status.Text = text);
    }
}
```
{noinject="true"}

Stop and await producers before disposing the window or its controls. Ordinary
property setters such as `TextBlock.Text` request rendering automatically.
Changing `Panel.Items` updates parent/child membership but does not itself
request a new frame: prefer building the tree before attaching it, or explicitly
raise a `RenderRequestEvent` on the UI scheduler after a dynamic collection edit.

API source: [Control](https://github.com/asv-soft/asv-hal/blob/main/src/Asv.Hal/Display/Controls/Control.cs),
[Button](https://github.com/asv-soft/asv-hal/blob/main/src/Asv.Hal/Display/Controls/Simple/Button.cs),
[TextBox](https://github.com/asv-soft/asv-hal/blob/main/src/Asv.Hal/Display/Controls/Simple/TextBox.cs).
