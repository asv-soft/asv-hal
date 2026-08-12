using System.Text;
using Asv.Hal;
using Asv.IO;
using Moq;
using R3;

namespace Asv.Hal.Test.Display;

public class TelnetKeyboardTests
{
    [Fact]
    public void ParseKeysAfterTelnetNegotiationInSameTcpFrame()
    {
        var context = CreateKeyboard();
        var keys = new List<KeyValue>();
        using var subscription = context.Keyboard.OnKeyPress.Subscribe(keys.Add);

        context.Input.OnNext([
            0xFF, 0xFD, 0x01,
            0xFF, 0xFB, 0x03,
            (byte)'5',
            0x1B, (byte)'[', (byte)'A',
            0x0D, 0x0A,
            0x7F
        ]);

        Assert.Collection(keys,
            key => AssertKey(key, KeyType.Digit, '5'),
            key => AssertKey(key, KeyType.UpArrow),
            key => AssertKey(key, KeyType.Enter),
            key => AssertKey(key, KeyType.Escape));

        Assert.Contains(context.Sent, x => x.SequenceEqual(new byte[] { 0xFF, 0xFB, 0x01 }));
        Assert.Contains(context.Sent, x => x.SequenceEqual(new byte[] { 0xFF, 0xFD, 0x03 }));
    }

    [Fact]
    public void ParseEscapeSequencesSplitAcrossTcpFrames()
    {
        var context = CreateKeyboard();
        var keys = new List<KeyValue>();
        using var subscription = context.Keyboard.OnKeyPress.Subscribe(keys.Add);

        context.Input.OnNext([0x1B]);
        context.Input.OnNext(Encoding.ASCII.GetBytes("[B"));
        context.Input.OnNext([0x1B, (byte)'[', (byte)'1', (byte)'1']);
        context.Input.OnNext([(byte)'~']);
        context.Input.OnNext([0x1B, (byte)'O']);
        context.Input.OnNext([(byte)'P']);
        context.Input.OnNext([0x1B, (byte)'[', (byte)'3']);
        context.Input.OnNext([(byte)'~']);

        Assert.Collection(keys,
            key => AssertKey(key, KeyType.DownArrow),
            key => AssertKey(key, KeyType.Function),
            key => AssertKey(key, KeyType.Function),
            key => AssertKey(key, KeyType.Escape));
    }

    [Fact]
    public void ParseAsciiKeysLikeConsoleKeyboard()
    {
        var context = CreateKeyboard();
        var keys = new List<KeyValue>();
        using var subscription = context.Keyboard.OnKeyPress.Subscribe(keys.Add);

        context.Input.OnNext(Encoding.ASCII.GetBytes("12.,\r\n\n"));

        Assert.Collection(keys,
            key => AssertKey(key, KeyType.Digit, '1'),
            key => AssertKey(key, KeyType.Digit, '2'),
            key => AssertKey(key, KeyType.Dot, '.'),
            key => AssertKey(key, KeyType.Dot, '.'),
            key => AssertKey(key, KeyType.Enter),
            key => AssertKey(key, KeyType.Enter));
    }

    private static KeyboardContext CreateKeyboard()
    {
        var input = new Subject<byte[]>();
        var sent = new List<byte[]>();
        var port = new Mock<IDataStream>();
        port.SetupGet(x => x.OnReceive).Returns(input);
        port.Setup(x => x.Send(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Callback<byte[], int, CancellationToken>((data, count, _) => sent.Add(data.Take(count).ToArray()))
            .Returns(Task.FromResult(true));

        return new KeyboardContext(new TelnetKeyboard(port.Object), input, sent);
    }

    private static void AssertKey(KeyValue key, KeyType type, char? value = null)
    {
        Assert.Equal(type, key.Type);
        Assert.Equal(value, key.Value);
    }

    private sealed record KeyboardContext(TelnetKeyboard Keyboard, Subject<byte[]> Input, List<byte[]> Sent);
}
