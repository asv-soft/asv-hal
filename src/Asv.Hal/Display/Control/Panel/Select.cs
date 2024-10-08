using System.Diagnostics;

namespace Asv.Hal;

public class Select(string id): Panel(id)
{
    private Control? _header;
    private int _selectedIndex;

    public Control? Header
    {
        get => _header;
        set
        {
            if (_header == value) return;
            RemoveVisualChild(_header);
            _header = value;
            AddVisualChild(_header);
            RiseRenderRequestEvent();
        } 
    }

    public int SelectedIndex
    {
        get => _selectedIndex;
        set
        {
            if (value < 0) value = 0;
            if (value >= Count) value = Count - 1;
            if (_selectedIndex == value) return;
            _selectedIndex = value;
            RiseRenderRequestEvent();
        }
    }

    public override Size Measure(Size availableSize)
    {
        var heigth = 0;
        var width = 0;
        if (Header is { IsVisible: true })
        {
            var size = Header.Measure(availableSize);
            heigth += size.Height;
            width = Math.Max(width, size.Width);
        }

        foreach (var item in this.Where(x=>x.IsVisible))
        {
            var size = item.Measure(new Size(width,availableSize.Height - heigth));
            heigth += size.Height;
            width = Math.Max(width, size.Width);
        }
        return new Size(width,heigth);
    }

    public override void Render(IRenderContext context)
    {
        
        
        if (Header is { IsVisible: true })
        {
            Header?.Render(context.CreateSubContext(0,0,context.Size.Width,1));
            context = context.CreateSubContext(0,1,context.Size.Width,context.Size.Height-1);
        }

        var selectedItemY = 0;
        for (var i = 0; i < Count; i++)
        {
            var bounds = this[i].Measure(new Size(context.Size.Width,context.Size.Height-selectedItemY));
            selectedItemY += bounds.Height;
            if (SelectedIndex == i) break;
        }
        if (selectedItemY > context.Size.Height)
        {
            var scroll = selectedItemY - context.Size.Height;
            context = context.CreateSubContext(0,-scroll,context.Size.Width,context.Size.Height+scroll);
        }

        var height = 0;
        for (var i = 0; i < Count; i++)
        {
            var num = i + 1; // numbering start with 1
            var header = SelectedIndex == i ? $"->{num}." : $"  {num}.";
            context.WriteString(0,height,header);
            var item = this[i];
            var availableSize = new Size(context.Size.Width-header.Length,context.Size.Height-height);
            var bounds = item.Measure(availableSize);
            item.Render(context.CreateSubContext(header.Length,height, bounds));
            height += bounds.Height;
        }
    }

    protected override void InternalOnEvent(RoutedEvent e)
    {
        if (e is KeyDownEvent key)
        {
            OnKeyDown(key);
        }
    }

    private void OnKeyDown(KeyDownEvent e)
    {
        switch (e.Key.Type)
        {
            case KeyType.Enter:
                
                break;
            case KeyType.Digit:
                Debug.Assert(e.Key.Value.HasValue);
                e.IsHandled = true;
                SelectedIndex = int.Parse(e.Key.Value.Value.ToString()) - 1;// numbering start with 1
                break;
            case KeyType.Escape:
                break;
            case KeyType.Function:
                break;
            case KeyType.UpArrow:
                e.IsHandled = true;
                --SelectedIndex;
                break;
            case KeyType.DownArrow:
                e.IsHandled = true;
                ++SelectedIndex;
                break;
            case KeyType.LeftArrow:
                break;
            case KeyType.RightArrow:
                break;
            case KeyType.Dot:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}