using R3;

namespace Asv.Hal.Test.Display;

public class ControlModelingEventTests
{
    [Fact]
    public async Task EventAsync_Should_Bubble_To_Parent()
    {
        var parent = new ListBox();
        var child = new Button("child");
        parent.Items.Add(child);

        var handled = false;
        parent.Events.Catch<RenderRequestEvent>((_, _, _) =>
        {
            handled = true;
            return ValueTask.CompletedTask;
        });

        await child.EventAsync(new RenderRequestEvent(child), TestContext.Current.CancellationToken);

        Assert.True(handled);
    }

    [Fact]
    public void Button_Focus_Should_Not_Raise_GotFocus_After_Button_Clears_Focus()
    {
        var button = new Button("button");
        var gotFocus = false;
        var lostFocus = false;

        button.Events.Catch<GotFocusEvent>(_ => gotFocus = true);
        button.Events.Catch<LostFocusEvent>(_ => lostFocus = true);

        button.IsFocused = true;

        Assert.False(button.IsFocused);
        Assert.False(gotFocus);
        Assert.True(lostFocus);
    }
}
