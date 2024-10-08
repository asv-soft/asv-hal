namespace Asv.Hal;

public class NullLed :IRgbLed
{
    public NullLed() 
    {
        
    }

    public byte Red { get; set; }
    public byte Green { get; set; }
    public byte Blue { get; set; }
}