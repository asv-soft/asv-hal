namespace Asv.Hal;

public class Calibration : Control
{
    private readonly TimeSpan _time;
    private readonly TextBlock _text;
    private readonly TextBlock _progress;
    private readonly TextBlock _unit;
    private long? _startTime;

    public Calibration(string text, TimeSpan time)
    {
        _time = time;
        AddVisualChild(_text = new TextBlock
        {
            Text = text,
            Align = HorizontalPosition.Left,
        });
        AddVisualChild(_progress = new TextBlock
        {
            Text = "0",
            Align = HorizontalPosition.Right,
        });
        AddVisualChild(_unit = new TextBlock
        {
            Text = "%",
            Align = HorizontalPosition.Center,
        });
    }

    public string? Text
    {
        get => _text.Text;
        set => _text.Text = value;
    }

    public double Progress
    {
        get => int.Parse(_progress.Text ?? "0") / 100.0;
        set
        {
            var val = (int)Math.Round(value * 100.0, 0);
            if (val > 100) val = 100;
            if (val < 0) val = 0;
            _progress.Text = val.ToString();
        }
    }

    public override int Height => _text.Height + _progress.Height;
    public override int Width => Math.Max(_text.Width, _progress.Width);

    public override void Render(IRenderContext ctx)
    {
        _text.Render(ctx.Crop(2, 2, _text.Width, 1));
        _progress.Render(ctx.Crop(_text.Width + 2, 2, ctx.Width - (_text.Width + 5), 1));
        _progress.Render(ctx.Crop(_text.Width + 2, 2, ctx.Width - (_text.Width + 5), 1));
        _unit.Render(ctx.Crop(ctx.Width - 3, 2, 1, 1));
    }
    
    protected override void InternalOnEvent(RoutedEvent e)
    {
        if (e is AnimationTickEvent anim)
        {
            if (_startTime == null)
            {
                _startTime = anim.TimeProvider.GetTimestamp(); 
            }
            else
            {
                var delay = anim.TimeProvider.GetElapsedTime(_startTime.Value);
                Progress = delay.TotalMilliseconds / _time.TotalMilliseconds;
            }
        }
    }
    
    public void Reset()
    {
        _startTime = null;
        Progress = 0;
    }
}