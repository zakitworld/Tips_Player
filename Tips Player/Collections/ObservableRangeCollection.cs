using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Tips_Player.Collections;

/// <summary>
/// An ObservableCollection that supports batch operations for better performance.
/// </summary>
/// <typeparam name="T">The type of elements in the collection.</typeparam>
public class ObservableRangeCollection<T> : ObservableCollection<T>
{
    private bool _suppressNotification;

    /// <summary>
    /// Initializes a new instance of the ObservableRangeCollection class.
    /// </summary>
    public ObservableRangeCollection() : base()
    {
    }

    /// <summary>
    /// Initializes a new instance with items from another collection.
    /// </summary>
    public ObservableRangeCollection(IEnumerable<T> collection) : base(collection)
    {
    }

    /// <summary>
    /// Adds a range of items to the collection, raising a single notification.
    /// </summary>
    /// <param name="items">The items to add.</param>
    public void AddRange(IEnumerable<T> items)
    {
        if (items == null)
        {
            throw new ArgumentNullException(nameof(items));
        }

        var itemList = items.ToList();
        if (itemList.Count == 0)
        {
            return;
        }

        _suppressNotification = true;
        var startIndex = Count;

        foreach (var item in itemList)
        {
            Items.Add(item);
        }

        _suppressNotification = false;

        OnPropertyChanged(new PropertyChangedEventArgs(nameof(Count)));
        OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(
            NotifyCollectionChangedAction.Add,
            itemList,
            startIndex));
    }

    /// <summary>
    /// Removes a range of items from the collection, raising a single notification.
    /// </summary>
    /// <param name="items">The items to remove.</param>
    public void RemoveRange(IEnumerable<T> items)
    {
        if (items == null)
        {
            throw new ArgumentNullException(nameof(items));
        }

        var itemList = items.ToList();
        if (itemList.Count == 0)
        {
            return;
        }

        _suppressNotification = true;

        foreach (var item in itemList)
        {
            Items.Remove(item);
        }

        _suppressNotification = false;

        OnPropertyChanged(new PropertyChangedEventArgs(nameof(Count)));
        OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    /// <summary>
    /// Replaces all items in the collection with the specified items.
    /// </summary>
    /// <param name="items">The new items.</param>
    public void ReplaceRange(IEnumerable<T> items)
    {
        if (items == null)
        {
            throw new ArgumentNullException(nameof(items));
        }

        _suppressNotification = true;

        Items.Clear();
        foreach (var item in items)
        {
            Items.Add(item);
        }

        _suppressNotification = false;

        OnPropertyChanged(new PropertyChangedEventArgs(nameof(Count)));
        OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    /// <summary>
    /// Clears the collection and adds all items from the specified collection.
    /// </summary>
    /// <param name="items">The items to set.</param>
    public void Reset(IEnumerable<T> items)
    {
        ReplaceRange(items);
    }

    /// <summary>
    /// Sorts the collection using the default comparer.
    /// </summary>
    public void Sort()
    {
        Sort(Comparer<T>.Default);
    }

    /// <summary>
    /// Sorts the collection using the specified comparer.
    /// </summary>
    /// <param name="comparer">The comparer to use for sorting.</param>
    public void Sort(IComparer<T> comparer)
    {
        var sorted = Items.OrderBy(x => x, comparer).ToList();
        ReplaceRange(sorted);
    }

    /// <summary>
    /// Sorts the collection using the specified key selector.
    /// </summary>
    /// <typeparam name="TKey">The type of the sort key.</typeparam>
    /// <param name="keySelector">The function to extract the sort key.</param>
    public void Sort<TKey>(Func<T, TKey> keySelector)
    {
        var sorted = Items.OrderBy(keySelector).ToList();
        ReplaceRange(sorted);
    }

    /// <summary>
    /// Sorts the collection in descending order using the specified key selector.
    /// </summary>
    /// <typeparam name="TKey">The type of the sort key.</typeparam>
    /// <param name="keySelector">The function to extract the sort key.</param>
    public void SortDescending<TKey>(Func<T, TKey> keySelector)
    {
        var sorted = Items.OrderByDescending(keySelector).ToList();
        ReplaceRange(sorted);
    }

    /// <summary>
    /// Raises the CollectionChanged event with the provided arguments.
    /// </summary>
    protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        if (!_suppressNotification)
        {
            base.OnCollectionChanged(e);
        }
    }

    /// <summary>
    /// Raises the PropertyChanged event with the provided arguments.
    /// </summary>
    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        if (!_suppressNotification)
        {
            base.OnPropertyChanged(e);
        }
    }
}
