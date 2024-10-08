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
        var port = PortFactory.Create("serial:COM43?br=9600");
        port.Enable();
        var screen = new CompositeScreen(
            new ConsoleScreen(new Size(20,4)), 
            new DataStreamScreen(port, new Size(20,4)));

        var keyboard = new CompositeKeyboard(
            new ConsoleKeyboard(),
            new DataStreamKeyBoard(port)
        );

        
        
        
        var tcs = new TaskCompletionSource();
        Console.CancelKeyPress += (_, _) =>
        {
            tcs.TrySetResult();
        };
        tcs.Task.Wait();
    }
}