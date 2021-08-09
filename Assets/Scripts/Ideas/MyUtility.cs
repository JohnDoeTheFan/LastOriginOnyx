using Onyx.GameElement;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MyUtility
{
    public static bool IsInRange<T>(this IReadOnlyCollection<T> list, int index)
    {
        if (0 <= index && index < list.Count)
            return true;
        else
            return false;
    }

    public static void SetAnchorMaxX(this RectTransform rectTransform, float value)
    {
        Vector2 anchorMax = rectTransform.anchorMax;
        anchorMax.x = value;
        rectTransform.anchorMax = anchorMax;
    }
}

public class WaitUntilOrForSeconds : CustomYieldInstruction
{
    private readonly Func<bool> predicate;
    private float remainedSeconds;
    public override bool keepWaiting => (remainedSeconds -= Time.deltaTime) > 0f && !predicate();

    public WaitUntilOrForSeconds(Func<bool> predicate, float seconds)
    {
        this.predicate = predicate;
        this.remainedSeconds = seconds;
    }
}
public class WaitWhileOrForSeconds : CustomYieldInstruction
{
    private readonly Func<bool> predicate;
    private float remainedSeconds;
    public override bool keepWaiting => (remainedSeconds -= Time.deltaTime) > 0f && predicate();

    public WaitWhileOrForSeconds(Func<bool> predicate, float seconds)
    {
        this.predicate = predicate;
        this.remainedSeconds = seconds;
    }
}

public abstract class AbstractState
{
    protected List<ITransition> transitions = new List<ITransition>();
    public abstract IEnumerator Start(TangibleComponent owner);

    protected IEnumerator TryToTransition(TangibleComponent owner)
    {
        while (transitions.Count > 0)
        {
            for (int i = 0; i < transitions.Count; i++)
            {
                if (transitions[i].Check)
                {
                    owner.StartCoroutine(transitions[i].State.Start(owner));
                    yield break;
                }
            }

            yield return new WaitForEndOfFrame();
        }
    }

    public void AddTransition(ConditionalTransition transition)
    {
        transitions.Add(transition);
    }

    public void AddTransition(StaticTransition transition)
    {
        transitions.Add(transition);
    }

    public void AddTransition(Func<bool> condition, AbstractState state)
    {
        AddTransition(new ConditionalTransition(condition, state));
    }

    public void AddTransition(AbstractState state)
    {
        AddTransition(new StaticTransition(state));
    }
}

public class State : AbstractState
{
    private readonly Func<IEnumerator> toDo;
    public State(Func<IEnumerator> toDo)
    {
        this.toDo = toDo;
    }

    public override IEnumerator Start(TangibleComponent owner)
    {
        yield return owner.StartCoroutine(toDo());

        yield return TryToTransition(owner);
    }
}

public class EmptyState : AbstractState
{
    public override IEnumerator Start(TangibleComponent owner)
    {
        yield return TryToTransition(owner);
    }
}

public interface ITransition
{
    bool Check { get; }
    public AbstractState State { get; }
}

public struct ConditionalTransition : ITransition
{
    private readonly Func<bool> condition;
    private readonly AbstractState state;

    public ConditionalTransition(Func<bool> condition, AbstractState state)
    {
        this.condition = condition;
        this.state = state;
    }

    bool ITransition.Check => condition();

    AbstractState ITransition.State => state;
}

public struct StaticTransition : ITransition
{
    private readonly AbstractState state;

    public StaticTransition(AbstractState state)
    {
        this.state = state;
    }

    bool ITransition.Check => true;

    AbstractState ITransition.State => state;
}

public class ClosestObjectInSightManager<T> : Sight.ISubscriber where T : Component
{
    private readonly HashSet<T> list = new HashSet<T>();
    private T closest;

    readonly private IDisposable unsubscriber;
    readonly private Transform transform;
    readonly private Action<T, T> onChangedClosestObject;

    private bool shouldStop = false;

    public T Closest => closest;

    public ClosestObjectInSightManager(Sight sight, Transform transform, Action<T, T> onChangedClosestObject)
    {
        unsubscriber = sight.SubscribeManager.Subscribe(this);
        this.transform = transform;
        this.onChangedClosestObject = onChangedClosestObject;
    }

    public IEnumerator UpdateCoroutine()
    {
        while (!shouldStop)
        {
            Update();
            yield return null;
        }
    }

    public IEnumerator UpdateCoroutine(float interval)
    {
        while(!shouldStop)
        {
            Update();
            yield return new WaitForSeconds(interval);
        }
    }

    public void Update()
    {
        float minDistance = float.MaxValue;
        T newClosest = null;
        foreach(T item in list)
        {
            float distance = Vector3.Distance(transform.position, item.transform.position);
            if (minDistance > distance)
            {
                minDistance = distance;
                newClosest = item;
            }
        }

        if(closest != newClosest)
        {
            onChangedClosestObject(closest, newClosest);
            closest = newClosest;
        }
    }

    void Sight.ISubscriber.OnEnter(GameObject enteringObject)
    {
        T t = enteringObject.GetComponent<T>();
        if (t != null)
            list.Add(t);
    }

    void Sight.ISubscriber.OnExit(GameObject exitingObject)
    {
        T t = exitingObject.GetComponent<T>();
        if (t != null && list.Contains(t))
            list.Remove(t);
    }

    public void Dispose()
    {
        if(unsubscriber != null)
            unsubscriber.Dispose();
        shouldStop = true;
    }
}

[Serializable]
public struct MinMax2
{
    public MinMax2(Vector2 min, Vector2 max)
    {
        this.min = min;
        this.max = max;
    }

    public Vector2 min;
    public Vector2 max;
}