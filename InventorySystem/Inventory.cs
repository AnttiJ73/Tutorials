using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class ItemContainer<T>
{
    [field: SerializeField]
    public T Item
    {
        get; private set;
    }

    public int Amount;

    [field: SerializeField]
    public string Data
    {
        get; private set;
    }

    public ItemContainer(T item, int count)
    {
        this.Item = item;
        this.Amount = count;
        RefreshLabel();
    }

    public bool Compare(ItemContainer<T> other)
    {
        return this.Item.Equals(other.Item)
            && this.Data == other.Data;
    }

    public ItemContainer<T> CreateCopy()
    {
        return new ItemContainer<T>(this.Item, this.Amount)
        {
            Data = this.Data
        };
    }
}


[System.Serializable]
public class Inventory<T>
{
    [field: SerializeField]
    public UnityEvent OnChanged { get; private set; } = new UnityEvent();
    [field: SerializeField]
    public List<ItemContainer<T>> Items { get; private set; } = new List<ItemContainer<T>>();

    #region API Methods

    public bool AddItem(T item, int count)
    {
        ItemContainer<T> ic = new ItemContainer<T>(item, count);
        return Add(ic);
    }

    public bool AddItem(ItemContainer<T> itemContainer)
    {
        ItemContainer<T> ic = itemContainer.CreateCopy();
        return Add(ic);
    }

    public int Count()
    {
        int count = 0;
        foreach (var item in Items)
        {
            count += item.Amount;
        }
        return count;
    }

    public int CountUnique()
    {
        return Items.Count;
    }

    public int Count(ItemContainer<T> itemContainer)
    {
        int count = 0;
        foreach (var item in Items)
        {
            if (itemContainer.Compare(item))
            {
                count += item.Amount;
            }
        }
        return count;
    }

    public int Count(T item)
    {
        int count = 0;
        foreach (var i in Items)
        {
            if (i.Item.Equals(item))
            {
                count += i.Amount;
            }
        }
        return count;
    }

    public int Count(System.Func<ItemContainer<T>, bool> filterFunc)
    {
        int count = 0;
        foreach (var item in Items)
        {
            if (filterFunc(item))
            {
                count += item.Amount;
            }
        }
        return count;
    }

    public int Count(System.Func<ItemContainer<T>, int> countFunction)
    {
        int count = 0;
        foreach (var item in Items)
        {
            count += countFunction(item);
        }
        return count;
    }

    public bool RemoveItem(ItemContainer<T> itemContainer, int removeAmount = 1)
    {
        if (Count(itemContainer) < removeAmount)
        {
            return false;
        }

        int toRemove = removeAmount;

        for (int i = Items.Count - 1; i >= 0; i--)
        {
            if (itemContainer.Compare(Items[i]))
            {
                int removed = Mathf.Min(Items[i].Amount, toRemove);
                Items[i].Amount -= removed;

                if (Items[i].Amount <= 0)
                {
                    Items.RemoveAt(i);
                }
                else
                {
                    Items[i].RefreshLabel();
                }

                toRemove -= removed;

                if (toRemove <= 0)
                {
                    OnChanged.Invoke();
                    return true;
                }
            }
        }
        OnChanged.Invoke();
        return true;
    }

    public bool RemoveItem(T item, int removeAmount = 1)
    {
        if (Count(item) < removeAmount)
        {
            return false;
        }

        int toRemove = removeAmount;

        for (int i = Items.Count - 1; i >= 0; i--)
        {
            if (item.Equals(Items[i].Item))
            {
                int removed = Mathf.Min(Items[i].Amount, toRemove);
                Items[i].Amount -= removed;

                if (Items[i].Amount <= 0)
                {
                    Items.RemoveAt(i);
                }
                else
                {
                    Items[i].RefreshLabel();
                }

                toRemove -= removed;

                if (toRemove <= 0)
                {
                    OnChanged.Invoke();
                    return true;
                }
            }
        }
        OnChanged.Invoke();
        return true;
    }
    #endregion

    #region Internal Methods
    protected virtual bool Add(ItemContainer<T> item)
    {
        int toAdd = item.Amount;
        for (int i = 0; i < Items.Count; i++)
        {
            if (Items[i].Compare(item))
            {
                int added = toAdd;
                Items[i].Amount += added;
                Items[i].RefreshLabel();
                toAdd -= added;
                if (toAdd <= 0)
                {
                    OnChanged.Invoke();
                    return true;
                }
            }
        }

        if (toAdd > 0)
        {
            item.Amount = toAdd;
            Items.Add(item);
        }

        OnChanged.Invoke();
        return true;
    }

    #endregion

    private void OnValidate()
    {
        foreach (var item in Items)
        {
            item.RefreshLabel();
        }
    }
}
