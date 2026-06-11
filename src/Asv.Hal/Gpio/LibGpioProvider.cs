using System.Device.Gpio;
using System.Device.Gpio.Drivers;
using Asv.Common;
using Microsoft.Extensions.Logging;
using ZLogger;

namespace Asv.Hal;

public class LibGpioProvider(ILoggerFactory loggerFactory) : AsyncDisposableOnce, IGpioProvider
{
    private readonly SortedList<int,GpioController> _chip = new();
    private readonly object _sync = new();
    private readonly ILogger<LibGpioProvider> _logger = loggerFactory.CreateLogger<LibGpioProvider>();
    private GpioController? _defaultController;


    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _defaultController?.Dispose();
            foreach (var controller in _chip)
            {
                controller.Value.Dispose();
            }
        }

        base.Dispose(disposing);
    }

    protected override async ValueTask DisposeAsyncCore()
    {
        _defaultController?.Dispose();
        foreach (var controller in _chip)
        {
            controller.Value.Dispose();
        }
        await base.DisposeAsyncCore();
    }

    public GpioPin OpenPin(int pinNumber, PinMode mode, int? chipNumber)
    {
        lock (_sync)
        {
            if (chipNumber == null)
            {
                _defaultController ??= new GpioController();
                return _defaultController.OpenPin(pinNumber, mode);
            }

            if (_chip.TryGetValue(chipNumber.Value, out var controller) == false)
            {
                _logger.ZLogDebug($"Create GPIO controller for chip number {chipNumber}");
                _chip.Add(chipNumber.Value, controller = CreateController(chipNumber.Value));
            }
            _logger.ZLogDebug($"Open GIPO pin number {pinNumber}");
            return controller.OpenPin(pinNumber, mode);


        }
    }

    private GpioController CreateController(int chipNumber)
    {
        try
        {
            return new GpioController(new LibGpiodV2Driver(chipNumber));
        }
        catch (PlatformNotSupportedException ex)
        {
            _logger.ZLogWarning(
                $"Unable to create GPIO controller with libgpiod V2 driver for chip number {chipNumber}. Fallback to libgpiod V1 driver. {ex.Message}"
            );
            return new GpioController(new LibGpiodDriver(chipNumber));
        }
    }
}
