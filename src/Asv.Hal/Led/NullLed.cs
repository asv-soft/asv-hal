namespace Asv.Hal;

public class NullLed :IRgbLed
{
    public static IRgbLed Instance { get; } = new NullLed();

    public NullLed() 
    {
        
    }

    public byte Red { get; set; }
    public byte Green { get; set; }
    public byte Blue { get; set; }
    
}