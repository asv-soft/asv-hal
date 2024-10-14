using System.Diagnostics;

namespace Asv.Hal;

public class ListBox(string? header = null) : GroupBox(header)
{
    private int _selectedIndex;
    private string _pointer = "->";
    private string _emptyPointer = "  ";

    public Control? SelectedItem => Items.Count == 0 ? null : Items[SelectedIndex];
    public int SelectedIndex
    {
        get => _selectedIndex;
        set
        {
            if (value < 0) value = 0;
            if (value >= Items.Count) value = Items.Count - 1;
            if (_selectedIndex == value) return;
            var old = _selectedIndex;
            _selectedIndex = value;
            RiseRenderRequestEvent();
            var eve = new SelectionChangedEvent(this, Items[old],Items[_selectedIndex]);
            OnSelectionChanged(eve);
            if (eve.IsHandled == false) Event(eve);
        }
    }
    protected override void InternalRenderChildren(IRenderContext ctx)
    {
        var selectedItemY = 0;
        for (var i = 0; i < Items.Count; i++)
        {
            var item = Items[i];
            if (item.IsVisible == false) continue;
            selectedItemY += item.Height;
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
            var prefix = GetItemPrefix(i);
            ctx.WriteString(0,height,prefix);
            var availableSize = new Size(ctx.Size.Width-prefix.Length,ctx.Size.Height-height);
            item.Render(ctx.Crop(prefix.Length,height, availableSize));
            height += item.Height;
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
    
    protected string GetItemPrefix(int index)
    {
        return SelectedIndex == index ? $"{Pointer}{index + 1}." : $"{_emptyPointer}{index + 1}.";
    }
    protected virtual void OnSelectionChanged(SelectionChangedEvent e)
    {
        
    }

    protected override void InternalOnEvent(RoutedEvent e)
    {
        base.InternalOnEvent(e);
        if (e is LostFocusEvent focus && focus.Sender == SelectedItem)
        {
            IsFocused = true;
        }
        if (e is KeyDownEvent key && IsFocused)
        {
            switch (key.Key.Type)
            {
                case KeyType.DownArrow:
                    SelectedIndex++;
                    e.IsHandled = true;
                    break;
                case KeyType.UpArrow:
                    SelectedIndex--;
                    e.IsHandled = true;
                    break;
                case KeyType.Digit:
                    Debug.Assert(key.Key.Value.HasValue);
                    SelectedIndex = int.Parse(key.Key.Value.Value.ToString()) - 1;// numbering start with 1
                    if (SelectedItem != null) SelectedItem.IsFocused = true;
                    e.IsHandled = true;
                    break;
            }
        }
        
    }
}