using System.Reactive.Linq;

namespace Asv.Hal;

public class CompositeKeyboard(params IKeyboard[] keyboards) : IKeyboard
{
    public IObservable<KeyValue> OnKeyPress => keyboards.Select(x => x.OnKeyPress).Merge();
}