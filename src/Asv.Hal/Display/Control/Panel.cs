using System.Collections;

namespace Asv.Hal;

public abstract class Panel(string id) : Control(id), IList<Control>
{
    private readonly List<Control> _items = [];
    
    public IEnumerator<Control> GetEnumerator()
    {
        return _items.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Add(Control item)
    {
        _items.Add(item);
        AddVisualChild(item);
    }

    public void Clear()
    {
        foreach (var item in _items)
        {
            RemoveVisualChild(item);
        }
        _items.Clear();
    }

    public bool Contains(Control item)
    {
        return _items.Contains(item);
    }

    public void CopyTo(Control[] array, int arrayIndex)
    {
        _items.CopyTo(array, arrayIndex);
    }

    public bool Remove(Control item)
    {
        if (_items.Remove(item))
        {
            RemoveVisualChild(item);
            return true;
        }

        return false;
    }

    public int Count => _items.Count;
    public bool IsReadOnly => false;
    public int IndexOf(Control item)
    {
        return _items.IndexOf(item);
    }

    public void Insert(int index, Control item)
    {
        _items.Insert(index, item);
        AddVisualChild(item);
    }

    public void RemoveAt(int index)
    {
        var item = _items[index];
        _items.RemoveAt(index);
        RemoveVisualChild(item);
    }

    public Control this[int index]
    {
        get => _items[index];
        set
        {
            var old = _items[index];
            if (old == value) return;
            RemoveVisualChild(_items[index]);
            AddVisualChild(value);
            _items[index] = value;
        }
    }
}