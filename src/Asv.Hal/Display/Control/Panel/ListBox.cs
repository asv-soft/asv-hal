using System.Diagnostics;

namespace Asv.Hal;

public class ListBox : Panel
{
    private readonly Action<SelectionChangedEvent>? _onSelectionChanged;
    private Control? _header;
    private int _selectedIndex;
    private string _pointer = "->";
    private string _emptyPointer = "  ";

    public ListBox(string? header = null, Action<SelectionChangedEvent>? onSelectionChanged = null)
    {
        if (header != null)
        {
            Header = header;
        }
        _onSelectionChanged = onSelectionChanged;
    }
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

    public string Pointer
    {
        get => _pointer;
        set
        {
            if (_pointer == value) return;
            _pointer = value;
            _emptyPointer = new string(' ',value.Length);
            RiseRenderRequestEvent();
        } 
    }
    

    public int SelectedIndex
    {
        get => _selectedIndex;
        set
        {
            if (value < 0) value = 0;
            if (value >= Items.Count) value = Items.Count - 1;
            if (_selectedIndex == value) return;
            _selectedIndex = value;
            RiseRenderRequestEvent();
            var eve = new SelectionChangedEvent(this, Items[_selectedIndex]);
            _onSelectionChanged?.Invoke(eve);
            if (eve.IsHandled == false) Event(eve);
        }
    }
    
    public Control? SelectedItem => Items.Count == 0 ? null : Items[SelectedIndex];

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

        foreach (var item in Items.Where(x=>x.IsVisible))
        {
            var size = item.Measure(new Size(width,availableSize.Height - heigth));
            heigth += size.Height;
            width = Math.Max(width, size.Width);
        }
        return new Size(width,heigth);
    }

    public override void Render(IRenderContext ctx)
    {
        if (Header is { IsVisible: true })
        {
            Header?.Render(ctx.Crop(0,0,ctx.Size.Width,1));
            ctx = ctx.Crop(0,1,ctx.Size.Width,ctx.Size.Height-1);
        }

        var selectedItemY = 0;
        for (var i = 0; i < Items.Count; i++)
        {
            var item = Items[i];
            if (item.IsVisible == false) continue;
            var bounds = item.Measure(new Size(ctx.Size.Width,ctx.Size.Height-selectedItemY));
            selectedItemY += bounds.Height;
            if (SelectedIndex == i) break;
        }
        if (selectedItemY > ctx.Size.Height)
        {
            var scroll = selectedItemY - ctx.Size.Height;
            ctx = ctx.Crop(0,-scroll,ctx.Size.Width,ctx.Size.Height+scroll);
        }

        var height = 0;
        for (var i = 0; i < Items.Count; i++)
        {
            var item = Items[i];
            if (item.IsVisible == false) continue;
            var num = i + 1; // numbering start with 1
            var header = SelectedIndex == i ? $"{Pointer}{num}." : $"{_emptyPointer}{num}.";
            ctx.WriteString(0,height,header);
            var availableSize = new Size(ctx.Size.Width-header.Length,ctx.Size.Height-height);
            var bounds = item.Measure(availableSize);
            item.Render(ctx.Crop(header.Length,height, bounds));
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
                if (IsFocused)
                {
                    if (SelectedItem != null) SelectedItem.IsFocused = true;
                    SelectedItem?.Event(new KeyDownEvent(this,new KeyValue(KeyType.Enter,null)));
                    e.IsHandled = true;
                }
                break;
            case KeyType.Digit:
                if (IsFocused)
                {
                    Debug.Assert(e.Key.Value.HasValue);
                    SelectedIndex = int.Parse(e.Key.Value.Value.ToString()) - 1;// numbering start with 1
                    if (SelectedItem != null) SelectedItem.IsFocused = true;
                    SelectedItem?.Event(new KeyDownEvent(this,new KeyValue(KeyType.Enter,null)));
                    e.IsHandled = true;
                }
                break;
            case KeyType.Escape:
                IsFocused = true;
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
                SelectedIndex = 0;
                break;
            case KeyType.RightArrow:
                SelectedIndex = Items.Count - 1;
                break;
            case KeyType.Dot:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}