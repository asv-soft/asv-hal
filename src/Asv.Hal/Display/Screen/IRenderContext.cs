namespace Asv.Hal;

public interface IRenderContext
{
    Size Size { get; }
    void Write(int x, int y, char value);
    void WriteString(int x, int y, string? value)
    {
        if (value == null) return;
        if (x >= Size.Width || y >= Size.Height) return;
        if (x<0 || y<0) return;
        for (var i = 0; i < value.Length; i++)
        {
            Write(x+i, y, value[i]);
        }
    }
    void FillChar(int x, int y, int width, char c)
    {
        if (x >= Size.Width || y >= Size.Height) return;
        if (x<0 || y<0) return;
        for (var i = 0; i < width; i++)
        {
            Write(x+i, y, c);
        }
    }
    void FillWhiteSpace(int x, int y, int width)
    {
        FillChar(x,y,width,ScreenHelper.Empty);
    }

    IRenderContext CreateSubContext(int x, int y, Size size)
    {
        return new SubRenderContext(this, x, y, size);
    }
    
}

public class SubRenderContext(IRenderContext parent, int x, int y, Size size)
    : IRenderContext
{
    public Size Size { get; } = size;

    public void Write(int x1, int y1, char value)
    {
        parent.Write(x+x1, y+y1, value);
    }
}