using Asv.Common;

namespace Asv.Hal;

public class BlinkTextBlock:TextBlock
{
    private bool _isBlink;
    private long _lastBlink;
    private TimeSpan _blinkTime = TimeSpan.FromMilliseconds(500);
    private bool _isBlinkFlag;

    public BlinkTextBlock()
    {
        Events.Catch<AnimationTickEvent>(OnAnimationTickEvent).DisposeItWith(Disposable);
    }

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

    private void OnAnimationTickEvent(AnimationTickEvent e)
    {
        if (IsBlink)
        {
            if (e.TimeProvider.GetElapsedTime(_lastBlink) > BlinkTime)
            {
                _isBlinkFlag = !_isBlinkFlag;
                _lastBlink = e.TimeProvider.GetTimestamp();
                RiseRenderRequestEvent();
            }
        }
    }
}
