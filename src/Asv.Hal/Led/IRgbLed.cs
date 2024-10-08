namespace Asv.Hal;

public interface IRgbLed
{
    public byte Red { get; set; }
    public byte Green { get; set; }
    public byte Blue { get; set; }

    ValueTask Set(byte r, byte g, byte b)
    {
        Red = r;
        Green = g;
        Blue = b;
        return ValueTask.CompletedTask;
    }
}

