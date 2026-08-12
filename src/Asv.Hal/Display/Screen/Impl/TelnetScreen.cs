using System.Text;
using Asv.IO;

namespace Asv.Hal.Impl;

public class TelnetScreen : BufferScreen
{
    private readonly IPort _port;
    private readonly Encoding _encoding;
    private bool _terminalInitialized;

    public TelnetScreen(IPort port, Size size) : this(port, size, Encoding.UTF8)
    {
    }

    public TelnetScreen(IPort port, Size size, Encoding encoding) : base(size)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        ArgumentNullException.ThrowIfNull(port);
        ArgumentNullException.ThrowIfNull(encoding);

        _port = port;
        _encoding = encoding;
    }

    protected override void InternalRender(char[,] buffer)
    {
        var data = BuildFrame(buffer, !_terminalInitialized);
        _port.Send(data, data.Length, default).Wait();
        _terminalInitialized = true;
    }

    private byte[] BuildFrame(char[,] buffer, bool clearScreen)
    {
        var frame = new StringBuilder((Size.Width + 16) * Size.Height + 16);
        if (clearScreen)
        {
            frame.Append("\u001B[2J");
        }

        frame.Append("\u001B[H");
        for (var y = 0; y < Size.Height; y++)
        {
            frame.Append("\u001B[")
                .Append(y + 1)
                .Append(";1H");

            for (var x = 0; x < Size.Width; x++)
            {
                frame.Append(buffer[y, x]);
            }

            frame.Append("\u001B[K");
        }

        frame.Append("\u001B[H");

        return EscapeTelnetData(_encoding.GetBytes(frame.ToString()));
    }

    private static byte[] EscapeTelnetData(byte[] data)
    {
        if (!data.Contains((byte)TelnetOptions.Iac))
        {
            return data;
        }

        var escaped = new List<byte>(data.Length + 1);
        foreach (var b in data)
        {
            escaped.Add(b);
            if (b == (byte)TelnetOptions.Iac)
            {
                escaped.Add(b);
            }
        }

        return escaped.ToArray();
    }
}
