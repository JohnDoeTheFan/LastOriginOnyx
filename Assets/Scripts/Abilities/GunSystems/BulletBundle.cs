using Onyx.GameElement;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletBundle : MonoBehaviour, Sight.ISubscriber
{
    [SerializeField]
    private Bullet bullet;
    [SerializeField]
    private int quantity;
    [SerializeField]
    private Sight sight;
    [SerializeField]
    private GameObject gettingEffectPrefab;

    private IDisposable unsubscriber;

    private void Start()
    {
        unsubscriber = sight.SubscribeManager.Subscribe(this);
    }

    private void OnDestroy()
    {
        if(unsubscriber != null)
            unsubscriber.Dispose();
    }

    public interface IBulletBundleReactor
    {
        bool AddBullet(Bundle<Bullet> bullets);
    }

    void Sight.ISubscriber.OnEnter(GameObject enteringObject)
    {
        IBulletBundleReactor bundleReactor = enteringObject.GetComponent<IBulletBundleReactor>();
        if (bundleReactor != null && bundleReactor.AddBullet(new Bundle<Bullet>(bullet, quantity)))
        {
            if(gettingEffectPrefab != null)
                Instantiate<GameObject>(gettingEffectPrefab, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }

    void Sight.ISubscriber.OnExit(GameObject exitingObject)
    {

    }
}
