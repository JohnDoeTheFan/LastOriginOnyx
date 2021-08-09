using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

public class BulletLoaderSet : LoaderSetComponent<Bullet>
{
}

public class LoaderSetComponent<T> : MonoBehaviourBase
{
    readonly List<Loader<T>> loaders = new List<Loader<T>>();

    public ReadOnlyCollection<Loader<T>> Loaders => loaders.AsReadOnly();

    public int newLoaderCapacity = 9999;

    public List<Bundle<T>> Load(Bundle<T> bundle)
    {
        return Load(bundle.item, bundle.quantity);
    }

    public virtual List<Bundle<T>> Load(T item, int quantity)
    {
        List<Bundle<T>> retVal = new List<Bundle<T>>();

        bool failedToAdd = true;
        foreach (var loader in loaders)
        {
            if (loader.Item.Equals(item))
            {
                retVal = loader.Load(item, quantity);
                failedToAdd = false;
                break;
            }
        }

        if (failedToAdd)
        {
            Loader<T> newLoader = new Loader<T>(newLoaderCapacity);
            retVal = newLoader.Load(item, quantity);
            loaders.Add(newLoader);
        }

        return retVal;
    }
}

[Serializable]
public class Loader<T>
{
    [SerializeField]
    private T item;
    [SerializeField]
    private int quantity;
    [SerializeField]
    private int capacity;

    public T Item => item;
    public int Quantity => quantity;
    public int Capacity => capacity;

    public interface ISubscriber
    {
        void OnChange(Loader<T> loader);
    }

    public SubscribeManagerTemplate<ISubscriber> SubscribeManager { private set; get; }

    public Loader()
    {
        SubscribeManager = new SubscribeManagerTemplate<ISubscriber>();
    }

    public Loader(int capacity)
    {
        SubscribeManager = new SubscribeManagerTemplate<ISubscriber>();

        this.capacity = capacity;
    }

    public List<Bundle<T>> Load(T newItem, int numToLoad)
    {
        List<Bundle<T>> itemsToReturn = new List<Bundle<T>>();

        if (item != null)
        {
            if (item.Equals(newItem))
                numToLoad += quantity;
            else
                itemsToReturn.Add(new Bundle<T>(item, quantity));
        }

        int loaded = Mathf.Min(capacity, numToLoad);
        int remains = numToLoad - loaded;

        item = newItem;
        quantity = loaded;

        if (remains > 0)
            itemsToReturn.Add(new Bundle<T>(newItem, remains));

        SubscribeManager.ForEach((item) => item.OnChange(this));

        return itemsToReturn;
    }

    public List<Bundle<T>> Load(Bundle<T> bundle)
    {
        return Load(bundle.item, bundle.quantity);
    }

    public bool HasEnough(int demand)
    {
        return quantity >= demand;
    }

    public Bundle<T> Pop(int demand)
    {
        Bundle<T> retVal = new Bundle<T>(item, Mathf.Min(quantity, demand));

        quantity -= retVal.quantity;

        SubscribeManager.ForEach((item) => item.OnChange(this));

        return retVal;
    }

    public bool IsEmpty => item == null || quantity == 0;
    public bool IsFull => item != null && quantity == capacity;
}

[Serializable]
public struct Bundle<T>
{
    public Bundle(T item, int quantity)
    {
        this.item = item;
        this.quantity = quantity;
        isInfinite = false;
    }

    public T item;
    public int quantity;
    public bool isInfinite;
}
