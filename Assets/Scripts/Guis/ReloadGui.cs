using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReloadGui : MonoBehaviour, GunSlinger.ISubscriber
{
    [SerializeField]
    private ValueBarGui valueBarGui;
    [SerializeField]
    GunSlinger gunSlinger;

    private Vector2 adjust;
    private IDisposable unsubscriber;

    public void Awake()
    {
        unsubscriber = gunSlinger.SubscribeManager.Subscribe(this);

        if (gunSlinger.RemainLoadingTime == 0)
            gameObject.SetActive(false);

    }
    public void Initialize(Camera targetCamera, RectTransform mainCanvasTransform, GunSlinger gunSlinger)
    {
    }

    public void OnDestroy()
    {
        unsubscriber.Dispose();
    }


    void GunSlingerWild.ISubscriber.AfterEquip(GunSlinger gunSlinger, Gun2D gun) { }

    void GunSlingerWild.ISubscriber.AfterUnequip(GunSlinger gunSlinger, Gun2D gun) { }

    void GunSlingerWild.ISubscriber.BeforeDestroy(GunSlinger gunSlinger) { }

    void GunSlingerWild.ISubscriber.OnChangedClosestGun(GunSlinger gunSlinger, Gun2D closestGun) { }

    void GunSlingerWild.ISubscriber.OnUpdateBullets(GunSlinger gunSlinger) { }

    void GunSlingerWild.ISubscriber.OnUpdateReloadTime(GunSlinger gunSlinger)
    {
        if (gunSlinger.EquippedGun == null || gunSlinger.RemainLoadingTime == 0)
        { 
            gameObject.SetActive(false);
            return;
        }
        else
        {
            gameObject.SetActive(true);
            valueBarGui.SetValue(gunSlinger.RemainLoadingTime / gunSlinger.EquippedGun.LoadingTime);
        }
    }
}
