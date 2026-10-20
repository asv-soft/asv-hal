# Asv.Hal

Asv.Hal provides .NET hardware abstractions for components such as displays,
keyboards, GPIO, LEDs, and switches.

## Installation

Install the [Asv.Hal NuGet package](https://www.nuget.org/packages/Asv.Hal)
in a .NET 10 project:

```bash
dotnet add package Asv.Hal
```

Development packages are available on
[GitHub Packages](https://github.com/orgs/asv-soft/packages/nuget/package/Asv.Hal).

## Learn by example

Start with [Getting started](getting-started.md) for a console menu that runs
without hardware. Every C# example is either a complete program or a helper type
with explicit parameters; helper types are meant to be called by your application.

| Guide | Examples |
|-------|----------|
| [Display output gallery](display-examples.md) | Captured output, screenshots, and a runnable sample project |
| [Screens and rendering](screens.md) | Draw a frame, crop a region, implement a screen |
| [Keyboard input](keyboards.md) | Emit and subscribe to device keys |
| [Controls and events](controls-events.md) | Navigate pages, handle button events, marshal updates |
| [Property pages](property-pages.md) | Read numeric values, select presets, edit enums |
| [Remote and serial displays](remote-displays.md) | Mirror console/Telnet output, send a device frame |
| [GPIO and LEDs](gpio-leds.md) | Play an animation without hardware, open GPIO, configure RGB |

## Components

- Screens: console, buffer, composite, data stream, and Telnet implementations.
- Keyboards: console, composite, and data stream implementations.
- Controls and routed events for display interaction.
- GPIO providers and LED animations.

## Development

See the [development guide](development.md) for build, test, and documentation
publishing instructions, and the [GitHub repository](https://github.com/asv-soft/asv-hal)
for source code and issues.
