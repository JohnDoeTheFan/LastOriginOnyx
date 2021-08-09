using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunSlingerUni : GunSlinger
{
    public override float RemainEquipCoolTime => 0;

    public override bool CanChangeGun => false;

    protected override void Start()
    {
        base.Start();

        if (EquippedGun != null)
            InitGun(EquippedGun);
    }

    public override void EquipClosestGun()
    {
        return;
    }

    public void InitGun(Gun2D gun)
    {
        BoxCollider2D collider = gun.GetComponent<BoxCollider2D>();
        if (collider != null)
            collider.enabled = false;
        Rigidbody2D rigidBody = gun.GetComponent<Rigidbody2D>();
        if(rigidBody != null)
            rigidBody.simulated = false;
        gun.transform.SetParent(hand);
        gun.transform.localPosition = Vector3.zero;
        gun.transform.localRotation = Quaternion.identity;
        equippedGun = gun;
        gunUnsubscriber = equippedGun.SubscribeManager.Subscribe(this);

        ClassifyAvailableBullets();

        if (gun.Loader.IsEmpty)
            StartLoad();

        SubscribeManager.ForEach(item => item.AfterEquip(this, gun));
    }

    public override void Unequip()
    {
        return;
    }
}
