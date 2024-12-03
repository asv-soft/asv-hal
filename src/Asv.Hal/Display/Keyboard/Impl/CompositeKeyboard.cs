using R3;

namespace Asv.Hal;

public class CompositeKeyboard(params IKeyboard[] keyboards) : IKeyboard
{
    public Observable<KeyValue> OnKeyPress => keyboards.Select(x => x.OnKeyPress).Merge();
}