namespace Asv.Hal;

public class LoadingByTime(string text, TimeSpan time) : Loading(text)
{
    private long? _startTime;

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
                Progress = delay.TotalMilliseconds / time.TotalMilliseconds;
            }
            
        }
    }
    
    public void Reset()
    {
        _startTime = null;
        Progress = 0;
    }
   
}