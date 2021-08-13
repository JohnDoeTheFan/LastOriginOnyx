using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shield : MonoBehaviour, IHitReactor
{
    public SubscribeManagerTemplate<ISubscriber> SubscribeManager { private set; get; } = new SubscribeManagerTemplate<ISubscriber>();
     
    Vector3 IHitReactor.GetWorldPosition => transform.position;

    GameObject IHitReactor.GameObject => gameObject;

    IHitReactor.HitResult IHitReactor.Hit(IHitReactor.HitType type, float damage, Vector3 force)
    {
        SubscribeManager.ForEach(item => item.OnBlock(damage));
        return new IHitReactor.HitResult(0, false);
    }

    public interface ISubscriber
    {
        void OnBlock(float damage);
    }
}
