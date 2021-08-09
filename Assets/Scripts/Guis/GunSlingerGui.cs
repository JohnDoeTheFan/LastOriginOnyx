using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GunSlingerGui : AbilitySpecificGui<GunSlinger>, GunSlinger.ISubscriber
{
    [SerializeField]
    private GunGui gunGui;
    [SerializeField]
    private  OwningBulletsGui owningAvailableBulletsGui;
    [SerializeField]
    private OwningBulletsGui owningNotAvailableBulletsGui;

    private IDisposable unsubscriber;

    public override void SetAbility(GunSlinger ability)
    {
        if (unsubscriber != null)
            unsubscriber.Dispose();

        unsubscriber = ability.SubscribeManager.Subscribe(this);

        gunGui.SetGun(ability.EquippedGun);

        owningAvailableBulletsGui.SetGunSlinger(ability);
        owningNotAvailableBulletsGui.SetGunSlinger(ability);
    }

    void GunSlinger.ISubscriber.AfterEquip(GunSlinger gunSlinger, Gun2D gun)
    {
        gunGui.SetGun(gun);
    }

    void GunSlinger.ISubscriber.AfterUnequip(GunSlinger gunSlinger, Gun2D gun)
    {
    }

    void GunSlinger.ISubscriber.BeforeDestroy(GunSlinger gunSlinger)
    {
        unsubscriber.Dispose();
    }

    void GunSlinger.ISubscriber.OnChangedClosestGun(GunSlinger gunSlinger, Gun2D closestGun)
    {
    }

    void GunSlinger.ISubscriber.OnUpdateBullets(GunSlinger gunSlinger)
    {
    }

    void GunSlinger.ISubscriber.OnUpdateReloadTime(GunSlinger gunSlinger)
    {
    }
}
