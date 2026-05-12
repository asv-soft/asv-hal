// See https://aka.ms/new-console-template for more information

using System.Globalization;
using System.Reflection;
using System.Text;
using Asv.Hal;
using Asv.Hal.Impl;
using Asv.IO;
using ConsoleAppFramework;

Assembly.GetExecutingAssembly().PrintWelcomeToConsole();
Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = Encoding.UTF8;
var app = ConsoleApp.Create();
app.Add<AppCommands>();
app.Run(args);

public class AppCommands
{
    /// <summary>
    /// Simple console-only button example.
    /// </summary>
    /// <param name="width">-w, Console screen width.</param>
    /// <param name="height">-h, Console screen height.</param>
    /// <param name="tcpPort">-p, Optional TCP server port for terminal connection. Disabled by default.</param>
    [Command("button")]
    public void Button(int width = 32, int height = 4, int tcpPort = 0)
    {
        using var keyboard = new ConsoleKeyboard();
        var size = new Size(width, height);
        var consoleScreen = new ConsoleScreen(size);
        IScreen screen = consoleScreen;
        IKeyboard input = keyboard;
        IPort? tcpServer = null;
        TelnetKeyboard? telnetKeyboard = null;

        if (tcpPort > 0)
        {
            tcpServer = PortFactory.Create($"tcp://127.0.0.1:{tcpPort}?srv=true")
                ?? throw new InvalidOperationException($"TCP server port '{tcpPort}' was not created.");
            tcpServer.Enable();
            screen = new CompositeScreen(
                consoleScreen,
                new TelnetScreen(tcpServer, size));
            telnetKeyboard = new TelnetKeyboard(tcpServer);
            input = new CompositeKeyboard(keyboard, telnetKeyboard);
        }

        using var wnd = new Window(
            TimeProvider.System,
            TimeSpan.FromMilliseconds(100),
            input,
            screen,
            CultureInfo.GetCultureInfo("ru"));

        try
        {
            var clickCount = 0;
            var page = new MenuPanel("Пример кнопки", HorizontalPosition.Center)
            {
                Items =
                {
                    new Button("Нажмите Enter", click =>
                    {
                        clickCount++;
                        click.Button.Content = new TextBlock($"Нажато: {clickCount}", HorizontalPosition.Center)
                        {
                            Background = ' '
                        };
                    })
                }
            };

            wnd.GoTo(page);
            WaitForCancel();
        }
        finally
        {
            telnetKeyboard?.Dispose();
            (tcpServer as IDisposable)?.Dispose();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cs">-cs, Connection string to hardware display and keyboard. Empty by default.</param>
    [Command("display")]
    public void Root(string cs = "serial:COM43?br=9600")
    {
        var port = PortFactory.Create("tcp://127.0.0.1:7344?srv=true");
        var port1 = PortFactory.Create("serial:COM12?br=9600");
        port.Enable();
        port1.Enable();
        
        
        var screen = new CompositeScreen(
            new ConsoleScreen(new Size(20,4)), 
            new TelnetScreen(port, new Size(20,4)),
            new DataStreamScreen(port1, new Size(20,4)));

        var keyboard = new CompositeKeyboard(
            new ConsoleKeyboard(),
            new DataStreamKeyBoard(port1)
        );

        var wnd = new Window(TimeProvider.System, TimeSpan.FromMilliseconds(100), keyboard,screen, CultureInfo.GetCultureInfo("en"));

        var dmeRequesterMode = new PageSlider();
        var dmeResponderMode = new PageSlider();

        var selectMode = new MenuPanel("Выбор режима:", HorizontalPosition.Center)
        {
            Items =
            {
                new Button("Запросчик", onClick: x => wnd.GoTo(dmeRequesterMode)),
                new Button("Ответчик", onClick: x => wnd.GoTo(dmeResponderMode))
            },
        };
        wnd.GoTo(selectMode);

        // wnd.OnEvent.Subscribe(e =>
        // {
        //     if (e is KeyDownEvent { Key.Type: KeyType.Escape }) wnd.GoTo(selectMode);
        // });

        
        
        WaitForCancel();
    }
    
    private static void WaitForCancel()
    {
        var tcs = new TaskCompletionSource();
        Console.CancelKeyPress += (_, _) => tcs.TrySetResult();
        tcs.Task.Wait();
    }
}
