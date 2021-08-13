using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Onyx.GameElement;

public class ThrowBulletBundle : MonoBehaviour, Sight.ISubscriber
{
    [SerializeField]
    private Bullet bullet;
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
        if (unsubscriber != null)
            unsubscriber.Dispose();
    }

    public interface IThrowBulletBundleReactor
    {
        bool AddBullet();
    }

    void Sight.ISubscriber.OnEnter(GameObject enteringObject)
    {
        IThrowBulletBundleReactor bundleReactor = enteringObject.GetComponent<IThrowBulletBundleReactor>();
        if (bundleReactor != null && bundleReactor.AddBullet())
        {
            if (gettingEffectPrefab != null)
                Instantiate<GameObject>(gettingEffectPrefab, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }

    void Sight.ISubscriber.OnExit(GameObject exitingObject)
    {

    }
}
