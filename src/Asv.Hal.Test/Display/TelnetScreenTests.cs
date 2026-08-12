using System.Text;
using Asv.Hal.Impl;
using Asv.IO;
using Moq;

namespace Asv.Hal.Test.Display;

public class TelnetScreenTests
{
    [Fact]
    public void RenderSendsAnsiFrameWithUtf8Content()
    {
        var context = CreateScreen(new Size(4, 2));
        IRenderContext screen = context.Screen;

        using (context.Screen.BeginRenderLoop())
        {
            screen.WriteString(0, 0, "AB");
            screen.WriteString(1, 1, "\u042F!");
        }

        var text = Encoding.UTF8.GetString(Assert.Single(context.Sent));
        Assert.Equal("\u001B[2J\u001B[H\u001B[1;1HAB  \u001B[K\u001B[2;1H \u042F! \u001B[K\u001B[H", text);
    }

    [Fact]
    public void RenderDoesNotResetTerminalAfterFirstFrame()
    {
        var context = CreateScreen(new Size(2, 1));
        IRenderContext screen = context.Screen;

        using (context.Screen.BeginRenderLoop())
        {
            screen.WriteString(0, 0, "A");
        }

        using (context.Screen.BeginRenderLoop())
        {
            screen.WriteString(0, 0, "B");
        }

        Assert.Equal(2, context.Sent.Count);
        var firstFrame = Encoding.UTF8.GetString(context.Sent[0]);
        var secondFrame = Encoding.UTF8.GetString(context.Sent[1]);
        Assert.Contains("\u001B[2J", firstFrame);
        Assert.DoesNotContain("\u001B[2J", secondFrame);
        Assert.Equal("\u001B[H\u001B[1;1HB \u001B[K\u001B[H", secondFrame);
    }

    [Fact]
    public void RenderEscapesTelnetIacByteInEncodedContent()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        var context = CreateScreen(new Size(1, 1), Encoding.GetEncoding("windows-1251"));

        using (context.Screen.BeginRenderLoop())
        {
            context.Screen.Write(0, 0, '\u044F');
        }

        var frame = Assert.Single(context.Sent);
        var iacIndex = Array.IndexOf(frame, (byte)0xFF);
        Assert.NotEqual(-1, iacIndex);
        Assert.True(iacIndex + 1 < frame.Length);
        Assert.Equal(0xFF, frame[iacIndex + 1]);
    }

    private static ScreenContext CreateScreen(Size size, Encoding? encoding = null)
    {
        var sent = new List<byte[]>();
        var port = new Mock<IPort>();
        port.Setup(x => x.Send(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Callback<byte[], int, CancellationToken>((data, count, _) => sent.Add(data.Take(count).ToArray()))
            .Returns(Task.FromResult(true));

        var screen = encoding == null
            ? new TelnetScreen(port.Object, size)
            : new TelnetScreen(port.Object, size, encoding);

        return new ScreenContext(screen, sent);
    }

    private sealed record ScreenContext(TelnetScreen Screen, List<byte[]> Sent);
}
