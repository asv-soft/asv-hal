namespace Asv.Hal;

public class BusyPage : Control
{
    private readonly TextBlock _text;
    private readonly string _titleText;
    private readonly string[] _progress1 = ["----------", "=---------", "==--------", "===-------", "====------", "=====-----", "======----", "=======---", "========--", "=========-", "==========", "-=========", "--========", "---=======", "----======", "-----=====", "------====", "-------===", "--------==", "---------="];
    private readonly string[] _progress2 = ["==========", "=========-", "========--", "=======---", "======----", "=====-----", "====------", "===-------", "==--------", "=---------", "----------", "---------=", "--------==", "-------===", "------====", "-----=====", "----======", "---=======", "--========", "-========="];
    // private readonly string[] _progress1 = ["==--------", "-==-------", "--==------", "---==-----", "----==----", "-----==---", "------==--", "-------==-", "--------==", "=--------="];
    // private readonly string[] _progress2 = ["--------==", "-------==-", "------==--", "-----==---", "----==----", "---==-----", "--==------", "-==-------", "==--------", "=--------="];
    // private readonly string[] _progress1 = ["*  ", " * ", "  *"];
    // private readonly string[] _progress2 = ["  *", " * ", "*  "];
    private uint _progressIndex = 0;
    private uint _index = 0;
    private readonly TextBlock _text1;
    private readonly TextBlock _text2;


    public BusyPage(string text)
    {
        _titleText = text;
        AddVisualChild(_text = new TextBlock
        {
            Text = _titleText,
            Align = HorizontalPosition.Center
        });
        AddVisualChild(_text1 = new TextBlock
        {
            Text = $"{_progress1[_progressIndex % _progress1.Length]}",
            Align = HorizontalPosition.Center
        });
        AddVisualChild(_text2 = new TextBlock
        {
            Text = $"{_progress2[_progressIndex % _progress2.Length]}",
            Align = HorizontalPosition.Center
        });
    }
        
    public override int Width => _text.Width;
    public override int Height => _text.Height;
    public override void Render(IRenderContext ctx)
    {
        _text.Render(ctx.Crop(0, 1, ctx.Width, 1));
        _text1.Render(ctx.Crop(0, 2, ctx.Width, 1));
        _text2.Render(ctx.Crop(0, 3, ctx.Width, 1));
        
    }
    
    protected override void InternalOnEvent(RoutedEvent e)
    {
        if (e is AnimationTickEvent anim)
        {
            ++_index;
            if (_index % 2 == 0)
            {
                ++_progressIndex;
                _text1.Text = $"{_progress1[_progressIndex % _progress1.Length]}";
                _text2.Text = $"{_progress2[_progressIndex % _progress2.Length]}";
            }
            
        }
    }
    
    public void Reset()
    {
        _progressIndex = 0;
    }
}