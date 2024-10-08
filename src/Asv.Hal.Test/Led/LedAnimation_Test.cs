using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Time.Testing;
using Xunit.Abstractions;
using Moq;

namespace Asv.Hal.Test.Led;

public class LedAnimation_Test
{
    private IRgbLed _led;
    private FakeTimeProvider _timeProvider = new();
    private ILogger _logger;
    private TimeSpan _tick;
    private readonly ITestOutputHelper _output;
    private int _busy;

    public LedAnimation_Test(ITestOutputHelper outputHelper)
    {
        _output = outputHelper;
    }

    [Fact]
    public async Task TestAnimationWithPattern_G___R____B()
    {
        // Arrange
        var ledMock = new Mock<IRgbLed>();
        var fakeTimeProvider = new FakeTimeProvider(DateTime.UtcNow);
        var animationTick = TimeSpan.FromMilliseconds(100);
        var record = "G___R____B";
        var animation = new LedAnimation(ledMock.Object, fakeTimeProvider, animationTick, record);

        // Act
        for (int i = 0; i < record.Length; i++)
        {
            fakeTimeProvider.Advance(animationTick); 
            await Task.Delay(10); 
        }
        // Assert
        ledMock.Verify(led => led.Set(0, byte.MaxValue, 0), Times.Once); 
        ledMock.Verify(led => led.Set(byte.MaxValue, 0, 0), Times.Once); 
        ledMock.Verify(led => led.Set(0, 0, byte.MaxValue), Times.Once);
        ledMock.Verify(led => led.Set(0, 0, 0), Times.Exactly(7)); 
    }

    [Fact]
    private async Task LedAnimation_Test_Red_Light_After_tick()
    {
        var record = @"R_G_B_R_GB";
        _tick = new TimeSpan(1);
        _led = new NullLed();
        using var a = new LedAnimation(_led, _timeProvider, _tick, record);
        _output.WriteLine($"{_led.Red} {_led.Green} {_led.Blue}");
        Assert.Equal(255, _led.Red);
    }

    [Fact]
    private void LedAnimation_Test_Green_Light_After_tick()
    {
        var record = @"G___R____B";
        _tick = new TimeSpan(1);
        _led = new NullLed();
        using var a = new LedAnimation(_led, _timeProvider, _tick, record);
        _output.WriteLine($"{_led.Red} {_led.Green} {_led.Blue}");
        Assert.Equal(255, _led.Green);
    }

    [Fact]
    private void LedAnimation_Test_Blue_Light_After_tick()
    {
        var record = @"B";
        _tick = new TimeSpan(1);
        _led = new NullLed();
        using var a = new LedAnimation(_led, _timeProvider, _tick, record);
        _output.WriteLine($"{_led.Red} {_led.Green} {_led.Blue}");
        Assert.Equal(255, _led.Blue);
    }
}