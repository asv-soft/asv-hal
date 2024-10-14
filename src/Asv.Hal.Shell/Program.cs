// See https://aka.ms/new-console-template for more information

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
    /// 
    /// </summary>
    /// <param name="cs">-cs, Connection string to hardware display and keyboard. Empty by default.</param>
    [Command("display")]
    public void Root(string cs = "serial:COM43?br=9600")
    {
        var port = PortFactory.Create("tcp://127.0.0.1:7344?srv=true");
        port.Enable();
        var screen = new CompositeScreen(
            new ConsoleScreen(new Size(20,4)), 
            new TelnetScreen(port, new Size(20,4)));

        var keyboard = new CompositeKeyboard(
            new ConsoleKeyboard(),
            new TelnetKeyboard(port)
        );

        var wnd = new Window(TimeProvider.System, TimeSpan.FromMilliseconds(100), keyboard,screen);
        
        var loading = new LoadingByTime("Loading...", TimeSpan.FromSeconds(3));
        var editor = new PropertyEditor()
        {
            Header = new ToggleSwitch("Header"),
            Items =
            {
                new TextBox("Param1","dBm"),
                new TextBox("Param2","Hz"),
                new TextBox("Param3","m"),
                new TextBox("Param4","m/s"),
                new TextBox("Param5","m/s^2"),},
        };
        var selectMode = new MenuPanel
        {
            Header = "Select mode:",
            Items =
            {
                new Button("Loading", onClick: x => wnd.GoTo(loading)),
                new Button("Item 2"),
                new Button("Item 3"),
                new Button("Item 4"),
                new Button("Item 5"),
                new Button("Editor", onClick: x => wnd.GoTo(editor)),

            },
        };
        
        wnd.GoTo(selectMode);
        
        var tcs = new TaskCompletionSource();
        Console.CancelKeyPress += (_, _) =>
        {
            tcs.TrySetResult();
        };
        tcs.Task.Wait();
    }
}