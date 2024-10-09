namespace Asv.Hal;

public class Progress : Control
{
    private double _value;

    public double Value
    {
        get => _value;
        set
        {
            if (value >= 1) value = 1;
            if (value < 0) value = 0;
            if (Math.Abs(value - _value) < 0.01) return;
            _value = value;
            RiseRenderRequestEvent();
        }
    }

    public char Left { get; set; } = '[';
    public char Right { get; set; } = ']';
    public char Fill { get; set; } = '=';
    public char Empty { get; set; } = '-';
    
    public override Size Measure(Size availableSize)
    {
        return new Size(availableSize.Width, 1);
    }

    public override void Render(IRenderContext ctx)
    {
        var width = ctx.Width - 2;
        var first = (int)(width * _value);
        var last = width - first;
        ctx.Write(0,0,Left);
        ctx.FillChar(1,0,first,Fill);
        ctx.FillChar(1 + first,0,last,Empty);
        ctx.Write(ctx.Width - 1,0,Right);
        
    }
}