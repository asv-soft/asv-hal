# GPIO and LEDs

`IGpioProvider` opens GPIO pins. `IRgbLed` exposes red, green, and blue channel
values, and `LedAnimation` applies a timed sequence to any `IRgbLed` implementation.

## Run without GPIO

Use a fresh `NullLed` to keep channel state in memory. This complete program
plays one sequence and then prints the final channel values:

```C#
using Asv.Hal;

IRgbLed led = new NullLed();
using (var animation = new LedAnimation(
    led,
    TimeProvider.System,
    TimeSpan.FromMilliseconds(100),
    "RGB_"))
{
    await Task.Delay(TimeSpan.FromSeconds(1));
}

await led.Set(0, 0, 0);
Console.WriteLine($"R={led.Red}, G={led.Green}, B={led.Blue}");
```
{noinject="true"}

Expected final output: `R=0, G=0, B=0`. The sample explicitly sets the final state:
disposing an animation stops its timer but does not reset the LED. `NullLed`
stores channel values and performs no physical output. `NullLed.Instance` is a
shared mutable instance; prefer `new NullLed()` when independent state matters.

`Set` is a default interface method, so call it through an `IRgbLed` reference.
See [Screens and rendering](screens.md) for the same interface-member pattern.

## Animation records

| Symbol | Effect |
|--------|--------|
| R | Red on; green and blue off |
| G | Green on; red and blue off |
| B | Blue on; red and green off |
| _ | All channels off |
| * | Restart the record immediately |

For example, `"R_G_B_*"` repeats three colored flashes separated by off periods.
Each color or pause occupies one animation tick. The timer starts immediately;
`*` restarts without waiting for an extra tick. A record without `*` ends when
the next tick reaches its end.

Records must be non-empty and contain only uppercase `R`, `G`, `B`, `_`, and `*`.
Put a repeat marker only at the end, after at least one timed symbol. A record
such as `"*"` passes the current character validation but repeatedly restarts
inside one callback and never advances. Symbols after `*` are unreachable.
Keep only one animation active per LED and dispose it before replacing it.

## Open a GPIO output

`LibGpioProvider` uses `System.Device.Gpio`. On a supported Linux device, install
the libgpiod runtime appropriate to the driver and give the process permission
to open the required GPIO device. Explicit chip selection tries `LibGpiodV2Driver`
and falls back to the v1 driver only on `PlatformNotSupportedException`.
A null chip uses the default `GpioController` instead.

This helper deliberately takes a chip number and line offset from the caller;
it does not assume a board's header-pin mapping:

```C#
using Asv.Hal;
using Microsoft.Extensions.Logging.Abstractions;
using System.Device.Gpio;

public static class GpioExample
{
    public static void WriteHigh(int chipNumber, int lineOffset)
    {
        using var provider = new LibGpioProvider(NullLoggerFactory.Instance);
        IGpioProvider gpio = provider;
        var pin = gpio.OpenPin(lineOffset, PinMode.Output, chipNumber);
        pin.Write(PinValue.High);
    }
}
```
{noinject="true"}

The provider owns its controllers and releases them on disposal. This short
helper only demonstrates a write; keep the provider alive for an output that
must remain controlled by your application. Numbering is passed directly to the
controller/driver and is not a physical connector pin number. See the
[System.Device.Gpio OpenPin API](https://learn.microsoft.com/en-us/dotnet/api/system.device.gpio.gpiocontroller.openpin?view=iot-dotnet-latest).

## Configure an RGB LED

Provide a separate chip, line, and polarity for each channel. Do not use the
configuration class's default pin numbers as a wiring specification.

```C#
using Asv.Hal;

public static class RgbLedExample
{
    public static IRgbLed Create(
        IGpioProvider gpio, int chip, int redLine, int greenLine, int blueLine)
    {
        return new GpioRgbLed(new GpioRgbLedConfig
        {
            RedPinChip = chip,
            RedPin = redLine,
            RedInverted = false,
            GreenPinChip = chip,
            GreenPin = greenLine,
            GreenInverted = false,
            BluePinChip = chip,
            BluePin = blueLine,
            BlueInverted = false
        }, gpio);
    }
}
```
{noinject="true"}

The caller owns the provider and must keep it alive while the LED or animation is
in use. `GpioRgbLed` does not implement `IDisposable`.

For this implementation, `Inverted = false` means **active low**: a nonzero value
writes `Low`, and zero writes `High`. `Inverted = true` reverses these levels.
Despite its byte-valued properties, `GpioRgbLed` is a digital on/off driver:
values 1 through 255 all enable a channel; it does not implement PWM brightness.

The constructor opens pins and assigns channel properties to zero. Since backing
values already start at zero and setters skip unchanged values, these assignments
do not explicitly write an initial off level. Establish the required initial
levels in your hardware integration. Stopping an animation also does not force
an off state; arrange the required final state before disposing the provider.

API source: [LibGpioProvider](https://github.com/asv-soft/asv-hal/blob/main/src/Asv.Hal/Gpio/LibGpioProvider.cs),
[GpioRgbLed](https://github.com/asv-soft/asv-hal/blob/main/src/Asv.Hal/Led/GpioRgbLed.cs),
[LedAnimation](https://github.com/asv-soft/asv-hal/blob/main/src/Asv.Hal/Led/LedAnimation.cs).
