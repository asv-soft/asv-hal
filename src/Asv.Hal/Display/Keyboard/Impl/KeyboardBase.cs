using System.Reactive.Subjects;
using Asv.Common;

namespace Asv.Hal;

public class KeyboardBase:DisposableOnceWithCancel, IKeyboard
{
    private readonly Subject<KeyValue> _onKeyPress;

    protected KeyboardBase()
    {
        _onKeyPress = new Subject<KeyValue>().DisposeItWith(Disposable);
    }

    public IObservable<KeyValue> OnKeyPress => _onKeyPress;

    protected void RiseEnterEvent() => _onKeyPress.OnNext(new KeyValue(KeyType.Enter, null));

    protected void RiseEscapeEvent() => _onKeyPress.OnNext(new KeyValue(KeyType.Escape, null));

    protected void RiseLeftArrowEvent() => _onKeyPress.OnNext(new KeyValue(KeyType.LeftArrow, null));

    protected void RiseRightArrowEvent() => _onKeyPress.OnNext(new KeyValue(KeyType.RightArrow, null));

    protected void RiseUpArrowEvent() => _onKeyPress.OnNext(new KeyValue(KeyType.UpArrow, null));

    protected void RiseDownArrowEvent() => _onKeyPress.OnNext(new KeyValue(KeyType.DownArrow, null));

    protected void RiseDotEvent() => _onKeyPress.OnNext(new KeyValue(KeyType.Dot, '.'));

    protected void RiseDigitEvent(char digit)
    {
        _onKeyPress.OnNext(new KeyValue(KeyType.Digit, digit));
    }
    protected void RiseFunctionEvent()
    {
        _onKeyPress.OnNext(new KeyValue(KeyType.Function, null));
    }
    
    
}