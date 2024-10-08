using System.Text.RegularExpressions;
using Asv.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ZLogger;

namespace Asv.Hal;



public partial class LedAnimation : DisposableOnce
{
    public const char Red = 'R';
    public const char Green = 'G';
    public const char Blue = 'B';
    public const char Pause = '_';
    public const char RepeatSymbol = '*';
    private const string RegexString = @"^[RGB_\*]+$";
    [GeneratedRegex(RegexString, RegexOptions.Compiled)]
    private static partial Regex GetRegex();
    
    
    private readonly IRgbLed _led;
    private readonly string _record;
    private int _tick;
    private readonly ITimer _timer;
    private readonly ILogger _logger;
    private int _busy;

    public LedAnimation(IRgbLed led, TimeProvider timeProvider, TimeSpan animationTick, string record, ILogger? logger = null)
    {
        _logger = logger ?? NullLogger.Instance;
        _led = led;
        _record = record;
        if (GetRegex().IsMatch(record) == false)
        {
            throw new ArgumentException($"Invalid record: {record}. Must be match {RegexString}");
        }
        _logger.ZLogTrace($"Start animation LED: tick{animationTick:g}, rec:{record}");
        _timer = timeProvider.CreateTimer(Tick, null, TimeSpan.Zero, animationTick);
    }

    private async void Tick(object? state)
    {
        if (Interlocked.Exchange(ref _busy, 1) != 0)
        {
            // skip tick
            return;
        }

        try
        {
            start:
            var position = Interlocked.Increment(ref _tick) - 1;
            if (position >= _record.Length)
            {
                Dispose();
                return;
            }

            var symbol = _record[position];

            switch (symbol)
            {
                case Red:
                    await _led.Set(byte.MaxValue, 0, 0);
                    break;
                case Green:
                    await _led.Set(0, byte.MaxValue, 0);
                    break;
                case Blue:
                    await _led.Set(0, 0, byte.MaxValue);
                    break;
                case Pause:
                    await _led.Set(0, 0, 0);
                    break;
                case RepeatSymbol:
                    Interlocked.Exchange(ref _tick, 0);
                    goto start;
                    break;
                default:
                    break;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;

        }
        finally
        {
            Interlocked.Exchange(ref _busy, 0);
        }
    }

    protected override void InternalDisposeOnce()
    {
        _logger.LogTrace("Stop animation LED");
        _timer.Dispose();
    }
}