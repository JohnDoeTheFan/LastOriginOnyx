using System;
using System.Collections.Generic;

public interface IUnsubscriber
{
    public void Unsubscribe();
    void SetOnUnsubscribe(Action onUnsubscribe);
}

public class UniUnsubscriber : IUnsubscriber
{
    IDisposable unsubscriber;
    Action onUnsubscribe;

    public void Unsubscribe()
    {
        onUnsubscribe?.Invoke();
        unsubscriber.Dispose();
    }

    public void InitUniSubscriber(IDisposable unsubscriber)
    {
        this.unsubscriber = unsubscriber;
    }

    public void SetOnUnsubscribe(Action onUnsubscribe)
    {
        this.onUnsubscribe = onUnsubscribe;
    }
}

public class UnsubscriberPack
{
    readonly private List<IUnsubscriber> unsubscribers = new List<IUnsubscriber>();

    public void Add(IUnsubscriber unsubscriber)
    {
        unsubscribers.Add(unsubscriber);
        unsubscriber.SetOnUnsubscribe(() => unsubscribers.Remove(unsubscriber));
    }

    public void UnsubscribeAll()
    {
        unsubscribers.ForEach(item =>
        {
            item.SetOnUnsubscribe(null);
            item.Unsubscribe();
        });

        unsubscribers.Clear();
    }
}

public class SubscribeManagerTemplate<TSubscriber>
{
    private readonly List<TSubscriber> subscribers = new List<TSubscriber>();

    virtual public IDisposable Subscribe(TSubscriber subscriber)
    {
        if (!subscribers.Contains(subscriber))
            subscribers.Add(subscriber);

        return new Unsubscriber(subscribers, subscriber);
    }

    public void ForEach(Action<TSubscriber> action)
    {
        // Copying list make it can unsubscribe in callback;
        List<TSubscriber> copyOfList = new List<TSubscriber>(subscribers);
        copyOfList.ForEach(action);
    }

    class Unsubscriber : IDisposable
    {
        readonly List<TSubscriber> subscribers;
        readonly TSubscriber subscriber;

        public Unsubscriber(List<TSubscriber> subscribers, TSubscriber subscriber)
        {
            this.subscribers = subscribers;
            this.subscriber = subscriber;
        }
        void IDisposable.Dispose()
        {
            subscribers.Remove(subscriber);
        }
    }
}