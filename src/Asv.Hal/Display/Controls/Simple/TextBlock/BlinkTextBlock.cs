namespace Asv.Hal;

public class BlinkTextBlock:TextBlock
{
    private bool _isBlink;
    private long _lastBlink;
    private TimeSpan _blinkTime = TimeSpan.FromMilliseconds(500);
    private bool _isBlinkFlag;

    public bool IsBlink
    {
        get => _isBlink;
        set
        {
            _isBlink = value;
            if (value == false)
            {
                _isBlinkFlag = true;
            }
        }
    }

    public TimeSpan BlinkTime
    {
        get => _blinkTime;
        set
        {
            if (_blinkTime == value) return;
            _blinkTime = value;
            RiseRenderRequestEvent();
        }
    }

    public override void Render(IRenderContext ctx)
    {
        if (_isBlinkFlag)
        {
            base.Render(ctx);    
        }
    }

    protected override void InternalOnEvent(RoutedEvent e)
    {
        if (IsBlink && e is AnimationTickEvent anim)
        {
            if (anim.TimeProvider.GetElapsedTime(_lastBlink) > BlinkTime)
            {
                _isBlinkFlag = !_isBlinkFlag;
                _lastBlink = anim.TimeProvider.GetTimestamp();
                RiseRenderRequestEvent();
            }
        }
    }
}