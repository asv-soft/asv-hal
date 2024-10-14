namespace Asv.Hal;

public class Loading : Control
{
    private readonly TextBlock _text;
    private readonly Progress _progress;

    public Loading(string text)
    {
        AddVisualChild(_text = new TextBlock
        {
            Text = text,
            Align = HorizontalPosition.Center,
        });
        AddVisualChild(_progress = new Progress());
    }

    public string? Text
    {
        get => _text.Text;
        set => _text.Text = value;
    }

    public double Progress
    {
        get => _progress.Value;
        set => _progress.Value = value;
    }

    public override int Height => _text.Height + _progress.Height;
    public override int Width => Math.Max(_text.Width, _progress.Width);

    public override void Render(IRenderContext ctx)
    {
        _text.Render(ctx.Crop(0,1,ctx.Width,1));
        _progress.Render(ctx.Crop(0,2,ctx.Width,1));
    }
}