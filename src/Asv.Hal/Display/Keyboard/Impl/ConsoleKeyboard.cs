namespace Asv.Hal;

public class ConsoleKeyboard:KeyboardBase
{
    public ConsoleKeyboard()
    {
        Task.Run(() =>
        {
            while (!IsDisposed)
            {
                var key = Console.ReadKey(true);
                if (char.IsDigit(key.KeyChar))
                {
                    RiseDigitEvent(key.KeyChar);
                    continue;
                }
                if (key.KeyChar is '.' or ',')
                {
                    RiseDotEvent();
                    continue;
                }
                switch (key.Key)
                {
                    case ConsoleKey.Enter:
                        RiseEnterEvent();
                        break;
                    case ConsoleKey.F1:
                        RiseFunctionEvent();
                        break;
                    case ConsoleKey.LeftArrow:
                        RiseLeftArrowEvent();
                        break;
                    case ConsoleKey.RightArrow:
                        RiseRightArrowEvent();
                        break;
                    case ConsoleKey.UpArrow:
                        RiseUpArrowEvent();
                        break;
                    case ConsoleKey.DownArrow:
                        RiseDownArrowEvent();
                        break;
                    case ConsoleKey.Backspace:
                    case ConsoleKey.Delete:
                    case ConsoleKey.Escape:
                        RiseEscapeEvent();
                        break;
                }
            }
        });
    }
}