using Asv.Common;

namespace Asv.Hal;

public class LoadingByTime : Loading
{
    private readonly TimeSpan _time;
    private long? _startTime;

    public LoadingByTime(string text, TimeSpan time)
        : base(text)
    {
        _time = time;
        Events.Catch<AnimationTickEvent>(OnAnimationTickEvent).DisposeItWith(Disposable);
    }

    private void OnAnimationTickEvent(AnimationTickEvent e)
    {
        if (_startTime == null)
        {
            _startTime = e.TimeProvider.GetTimestamp(); 
        }
        else
        {
            var delay = e.TimeProvider.GetElapsedTime(_startTime.Value);
            Progress = delay.TotalMilliseconds / _time.TotalMilliseconds;
        }
    }
    
    public void Reset()
    {
        _startTime = null;
        Progress = 0;
    }
}
