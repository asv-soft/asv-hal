// See https://aka.ms/new-console-template for more information

using System.Globalization;
using System.Reactive.Linq;
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
    private static readonly double[] predefinedValues = [0.0, 98.0, 186.0, 274.0, 400.0];

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
            // new DataStreamScreen(port1, new Size(20,4)),
            new DataStreamScreen(port1, new Size(20,4)));

        var keyboard = new CompositeKeyboard(
            new ConsoleKeyboard(),
            // new TelnetKeyboard(port),
            new DataStreamKeyBoard(port1)
        );

        var wnd = new Window(TimeProvider.System, TimeSpan.FromMilliseconds(100), keyboard,screen);

        var dmeRequesterMode = new PageSlider();
        var dmeResponderMode = new PageSlider();

        CreateRequesterPages(dmeRequesterMode);
        CreateResponderPages(dmeResponderMode);
        
        var selectMode = new MenuPanel("Выбор режима:", HorizontalPosition.Center)
        {
            Items =
            {
                new Button("Запросчик", onClick: x => wnd.GoTo(dmeRequesterMode)),
                new Button("Ответчик", onClick: x => wnd.GoTo(dmeResponderMode))
            },
        };
        wnd.GoTo(selectMode);

        wnd.OnEvent.Subscribe(e =>
        {
            if (e is KeyDownEvent { Key.Type: KeyType.Escape }) wnd.GoTo(selectMode);
        });

        
        
        var tcs = new TaskCompletionSource();
        Console.CancelKeyPress += (_, _) =>
        {
            tcs.TrySetResult();
        };
        tcs.Task.Wait();
    }

    private static void CreateRequesterPages(PageSlider dmeRequesterMode)
    {
        var levelPage = new StepIncrementPage("Запросчик", "Pвых:", "дБм", "F2", -1.0, 0.5,
            d =>
            {
                var divided = d / 0.5;
                var rounded = Math.Round(divided, MidpointRounding.AwayFromZero);
                var result = rounded * 0.5;
                result = Math.Round(result, 1);
                return result switch
                {
                    < -1.0 => -1.0,
                    > 6.0 => 6.0,
                    _ => result
                };
            }, 
            d =>
            {
                var divided = d / 0.5;
                var rounded = Math.Round(divided, MidpointRounding.AwayFromZero);
                var result = rounded * 0.5;
                result = Math.Round(result, 1);
                return result switch
                {
                    < 0.5 => 0.5,
                    > 10 => 10,
                    _ => result
                };
            });


        var frequencyPage = new StepIncrementPage("Запросчик", "Fоп:", "МГц", "F6", 1025.0, 1.0,
            d => DmeChannel.XChannel.GetInfoFromRequestFreq(d * 1_000_000)!.RequesterFreq / 1_000_000,
            d =>
            {
                d = Math.Round(d);
                return d switch
                {
                    < 1.0 => 1.0,
                    > 999 => 999.0,
                    _ => d
                };
            }, new TextBox("Канал"), 
            (box, d) =>
            {
                var freq = DmeChannel.XChannel.GetInfoFromRequestFreq(d * 1_000_000);
                box.Text = $"{freq.ChannelName} {freq.ResponderFreq / 1_000_000:F0}MHz";
            });

        var measuredDistance = new PropertyReaderPage("Запросчик", "Изм. дальн. (км):", "F3");

        var requestCntPage = new StepIncrementPage("Запросчик", "Загр.:", "п/с", "F2", 100.0, 20.0,
            d =>
            {
                d = Math.Round(d);
                return d switch
                {
                    < 20.0 => 20.0,
                    > 6000.0 => 6000.0,
                    _ => d
                };
            },
            d =>
            {
                d = Math.Round(d);
                return d switch
                {
                    < 1.0 => 1.0,
                    > 1000.0 => 1000.0,
                    _ => d
                };
            });


        var channel = new EnumPropertyEditor<DmeChannel>("Запросчик", "Формат излучения:", DmeChannel.XChannel,
            channel =>
            {
                return channel switch
                {
                    DmeChannel.XChannel => "Канал - X",
                    DmeChannel.YChannel => "Канал - Y",
                    _ => throw new ArgumentOutOfRangeException(nameof(channel), channel, null)
                };
            });
        
        var rnd = new Random();
        measuredDistance.PropertyValue = 400.0;
        Observable.Timer(TimeSpan.FromMilliseconds(1000), TimeSpan.FromMilliseconds(100))
            .Subscribe(_ => measuredDistance.PropertyValue = 400 + rnd.NextDouble() * 10 - 5);
        
        dmeRequesterMode.Items.Add(levelPage);
        dmeRequesterMode.Items.Add(frequencyPage);
        dmeRequesterMode.Items.Add(measuredDistance);
        dmeRequesterMode.Items.Add(requestCntPage);
        dmeRequesterMode.Items.Add(channel);
    }
    
    private void CreateResponderPages(PageSlider dmeResponderMode)
    {
        var levelPage = new StepIncrementPage("Ответчик", "Pвых:", "дБм", "F2", -1.0, 0.5, d =>
        {
            var divided = d / 0.5;
            var rounded = Math.Round(divided, MidpointRounding.AwayFromZero);
            var result = rounded * 0.5;
            result = Math.Round(result, 1);
            return result switch
            {
                < -1.0 => -1.0,
                > 6.0 => 6.0,
                _ => result
            };
        }, d =>
        {
            var divided = d / 0.5;
            var rounded = Math.Round(divided, MidpointRounding.AwayFromZero);
            var result = rounded * 0.5;
            result = Math.Round(result, 1);
            return result switch
            {
                < 0.5 => 0.5,
                > 10 => 10,
                _ => result
            };
        });


        var frequencyPage = new StepIncrementPage("Ответчик", "Fоп:", "МГц", "F6", 962.0, 1.0,
            d => DmeChannel.YChannel.GetInfoFromResponderFreq(d * 1_000_000)!.ResponderFreq / 1_000_000,
            d =>
            {
                d = Math.Round(d);
                return d switch
                {
                    < 1.0 => 1.0,
                    > 999 => 999.0,
                    _ => d
                };
            }, new TextBox("Канал:"), 
            (box, d) =>
            {
                var freq = DmeChannel.YChannel.GetInfoFromResponderFreq(d * 1_000_000);
                box.Text = $"{freq.ChannelName} {freq.RequesterFreq / 1_000_000:F0}MHz";
            });


        var channel = new EnumPropertyEditor<DmeChannel>("Ответчик", "Формат излучения:", DmeChannel.XChannel,
            channel =>
            {
                return channel switch
                {
                    DmeChannel.XChannel => "Канал - X",
                    DmeChannel.YChannel => "Канал - Y",
                    _ => throw new ArgumentOutOfRangeException(nameof(channel), channel, null)
                };
            });

        var distance = new PropertyWriterPage("Ответчик", "Имитир. дальн.",
            predefinedValues,
            v =>
            {
                var valKm = Math.Round(v).ToString("F0", CultureInfo.InvariantCulture);
                var valNm = Math.Round(v * 0.5399568).ToString("F0", CultureInfo.InvariantCulture);
                return $"{valKm} км / {valNm} n.mil";
            });
        var customSettings = new TogglePropertyEditor("Ответчик")
        {
            Items =
            {
                new CustomToggleSwitch(KeyType.Digit, "СО:"), 
                new CustomToggleSwitch(KeyType.Digit, "ХИП:")
            }
        };
        
        
        var rnd = new Random();
        uint index = 0;
        Observable.Timer(TimeSpan.FromMilliseconds(1000), TimeSpan.FromMilliseconds(3000))
            .Subscribe(_ => distance.PropertyValue = predefinedValues[index++ % 5] + rnd.NextDouble() * 10 - 5);
        
        dmeResponderMode.Items.Add(levelPage);
        dmeResponderMode.Items.Add(frequencyPage);
        dmeResponderMode.Items.Add(distance);
        dmeResponderMode.Items.Add(channel);
        dmeResponderMode.Items.Add(customSettings);
        
    }
}