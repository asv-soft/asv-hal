using System.Text;
using Asv.Hal;
using Asv.Hal.Impl;

Console.OutputEncoding = Encoding.UTF8;

switch (args.FirstOrDefault() ?? "frame")
{
    case "frame":
        DrawFrame();
        break;
    case "menu":
        DrawMenu();
        break;
    case "status":
        DrawStatus();
        break;
    case "controls":
        DrawControls();
        break;
    default:
        Console.Error.WriteLine("Choose frame, menu, status, or controls.");
        Environment.ExitCode = 1;
        break;
}

static void DrawFrame()
{
    IScreen screen = new ConsoleScreen(new Size(20, 4));
    using (screen.BeginRenderLoop())
    {
        screen.WriteString(0, 0, "Device ready");
        screen.FillChar(0, 1, 20, '-');
        IRenderContext details = screen.Crop(2, 2, 16, 1);
        details.WriteString(0, 0, "Voltage: 12 V");
    }
}

static void DrawMenu()
{
    using var counter = new TextBlock("Count: 0");
    var count = 0;
    using var increment = new Button(onClick: _ => counter.Text = $"Count: {++count}")
    {
        Content = counter
    };
    using var exit = new Button("Exit");
    using var menu = new MenuPanel("HAL demo", HorizontalPosition.Center)
    {
        Items = { increment, exit }
    };

    // A Button activates on focus and immediately releases it.
    for (var i = 0; i < 3; i++)
    {
        increment.IsFocused = true;
    }

    Render(menu, new Size(32, 4));
}

static void DrawStatus()
{
    using var page = new PropertyReaderPage(
        "Sensor ", "ON", "OFF", "Temperature, C", "F1")
    {
        PropertyValue = 22.5
    };
    page.ExternalUpdateValue(true);
    Render(page, new Size(32, 4));
}

static void DrawControls()
{
    using var temperature = new TextBlock("Temperature: 22.5 C");
    using var enabled = new ToggleSwitch("Output ") { Value = true };
    using var label = new TextBlock("Progress: 65%");
    using var progress = new Progress { Value = 0.65 };
    using var page = new InfoPage("Device status", HorizontalPosition.Center)
    {
        Items = { temperature, enabled, label, progress }
    };
    Render(page, new Size(32, 6));
}

static void Render(Control control, Size size)
{
    IScreen screen = new ConsoleScreen(size);
    using (screen.BeginRenderLoop())
    {
        control.Render(screen);
    }
}
