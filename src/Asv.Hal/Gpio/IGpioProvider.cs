using System.Collections.Immutable;
using System.Device.Gpio;

namespace Asv.Hal;

public interface IGpioProvider
{
    GpioPin OpenPin(int pinNumber, PinMode mode, int? chipNumber);
}
