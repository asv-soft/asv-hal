using System.Device.Gpio;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ZLogger;

namespace Asv.Hal;

public class GpioRgbLedConfig
{
    public int? RedPinChip { get; set; } = 0;
    public int RedPin { get; set; } = 64;
    public bool RedInverted { get; set; }
    public int? GreenPinChip { get; set; } = 0;
    public int GreenPin { get; set; } = 65;
    public bool GreenInverted { get; set; }
    public int? BluePinChip { get; set; } = 0;
    public int BluePin { get; set; } = 66;
    public bool BlueInverted { get; set; }
}

public class GpioRgbLed : IRgbLed
{
    private readonly PinValue _redEnabled;
    private readonly PinValue _greenEnabled;
    private readonly PinValue _blueEnabled;
    private readonly PinValue _blueDisabled;
    private readonly PinValue _greenDisabled;
    private readonly PinValue _redDisabled;
    private readonly GpioPin _redPin;
    private readonly GpioPin _greenPin;
    private readonly GpioPin _bluePin;
    private byte _red;
    private byte _green;
    private byte _blue;


    public GpioRgbLed(GpioRgbLedConfig config, IGpioProvider controller, ILogger? logger = null)
    {
        var log = logger ?? NullLogger.Instance;
        _redEnabled = config.RedInverted ? PinValue.High : PinValue.Low;
        _greenEnabled = config.GreenInverted ? PinValue.High : PinValue.Low;
        _blueEnabled = config.BlueInverted ? PinValue.High : PinValue.Low;
        _redDisabled = config.RedInverted ? PinValue.Low : PinValue.High;
        _greenDisabled = config.GreenInverted ? PinValue.Low : PinValue.High;
        _blueDisabled = config.BlueInverted ? PinValue.Low : PinValue.High;
        try
        {
            _redPin = controller.OpenPin(config.RedPin, PinMode.Output, config.RedPinChip);
            _greenPin = controller.OpenPin(config.GreenPin, PinMode.Output, config.GreenPinChip);
            _bluePin = controller.OpenPin(config.BluePin, PinMode.Output, config.BluePinChip);
            Red = 0;
            Green = 0;
            Blue = 0;
            log.ZLogDebug($"Ctor RGB LED: Red={config.RedPin}[inverted:{config.RedInverted}, chip:{config.RedPinChip}], Green={config.GreenPin}[inverted:{config.GreenInverted}, chip:{config.GreenPinChip}], Blue={config.BluePin}[inverted:{config.BlueInverted}, chip:{config.BluePinChip}]");
        }
        catch (Exception e)
        {
            log.ZLogError(e,$"Error to open GPIO controller:{e.Message}");
            throw;
        }
    }

    public byte Red
    {
        get => _red;
        set
        {
            if (_red == value) return;
            _red = value;
            _redPin.Write(value > 0 ? _redEnabled : _redDisabled);
        }
    }

    public byte Green
    {
        get => _green;
        set
        {
            if (_green == value) return;
            _green = value;
            _greenPin.Write(value > 0 ? _greenEnabled : _greenDisabled);
        }
    }

    public byte Blue
    {
        get => _blue;
        set
        {
            if (_blue == value) return;
            _blue = value;
            _bluePin.Write(value > 0 ? _blueEnabled:_blueDisabled);
        }
    }

}