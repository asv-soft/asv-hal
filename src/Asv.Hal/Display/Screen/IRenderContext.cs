namespace Asv.Hal;

public interface IRenderContext
{
    
    Size Size { get; }
    void Write(int x, int y, char value);
    int Width => Size.Width;
    int Height => Size.Height;
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

    IRenderContext Crop(int x, int y, Size size)
    {
        return new SubRenderContext(this, x, y, size);
    }
    IRenderContext Crop(int x, int y, int width, int height)
    {
        return new SubRenderContext(this, x, y, new Size(width, height));
    }
    
}

public class SubRenderContext(IRenderContext parent, int x, int y, Size size)
    : IRenderContext
{
    public Size Size { get; } = size;

    public void Write(int x1, int y1, char value)
    {
        if (x1<0 || y1<0 || x1 >= Size.Width || y1 >= Size.Height) return;
        parent.Write(x+x1, y+y1, value);
    }
}