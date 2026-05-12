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

        await child.EventAsync(new RenderRequestEvent(child));

        Assert.True(handled);
    }
}
