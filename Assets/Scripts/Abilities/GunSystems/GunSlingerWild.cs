using Onyx.GameElement;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunSlingerWild : GunSlinger, Sight.ISubscriber
{
    [SerializeField]
    protected Sight sight;

    public override bool CanChangeGun => true;
    public override float RemainEquipCoolTime => remainEquipCoolTime;


    private float remainEquipCoolTime = 0f;
    private readonly float equipCoolTime = 0.1f;

    private IDisposable sightUnsubscriber;
    private Gun2D closestGun;
    private readonly List<Gun2D> gunsInSight = new List<Gun2D>();


    protected override void Start()
    {
        base.Start();

        if (EquippedGun != null)
            InitGun(EquippedGun);

        if (sight != null)
            sightUnsubscriber = sight.SubscribeManager.Subscribe(this);

        StartCoroutines();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        sightUnsubscriber?.Dispose();
    }

    void StartCoroutines()
    {
        IEnumerator ReduceRemainEquipCoolTime()
        {
            while(true)
            {
                yield return new WaitUntil(() => remainEquipCoolTime > 0);
                yield return new WaitForSeconds(TargetFrameSeconds);

                remainEquipCoolTime = Mathf.Max(0, remainEquipCoolTime - TargetFrameSeconds);
            }
        }

        IEnumerator UpdateClosestGun()
        {
            while (true)
            {
                yield return new WaitForSeconds(TargetFrameSeconds);

                Gun2D minDistanceGun = null;
                float minDistance = float.MaxValue;
                foreach(Gun2D gun in gunsInSight )
                {
                    float distance = Vector3.Distance(transform.position, gun.transform.position);
                    if(distance < minDistance)
                    {
                        minDistanceGun = gun;
                        minDistance = distance;
                    }
                }
                if(closestGun != minDistanceGun)
                {
                    closestGun = minDistanceGun;
                    SubscribeManager.ForEach(item=>item.OnChangedClosestGun(this, closestGun));
                }
            }
        }

        StartCoroutine(ReduceRemainEquipCoolTime());
        StartCoroutine(UpdateClosestGun());
    }

    public override void EquipClosestGun()
    {
        if(closestGun != null)
            InitGun(closestGun);
    }

    public void EquipGun(Gun2D gun)
    {
        Unequip();

        InitGun(gun);
    }

    public void InitGun(Gun2D gun)
    {

        BoxCollider2D collider = gun.GetComponent<BoxCollider2D>();
        if (collider != null)
            collider.enabled = false;
        Rigidbody2D rigidBody = gun.GetComponent<Rigidbody2D>();
        if (rigidBody != null)
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
        if (EquippedGun == null)
            return;

        StopFire();
        StopLoad();

        Gun2D unequippedGun = equippedGun;
        equippedGun = null;
        gunUnsubscriber?.Dispose();

        BoxCollider2D collider = unequippedGun.GetComponent<BoxCollider2D>();
        if (collider != null)
            collider.enabled = true;
        Rigidbody2D rigidBody = unequippedGun.GetComponent<Rigidbody2D>();
        if (rigidBody != null)
            rigidBody.simulated = true;
        unequippedGun.transform.parent = null;
        unequippedGun.transform.localRotation = Quaternion.identity;

        remainEquipCoolTime = equipCoolTime;

        ClassifyAvailableBullets();

        SubscribeManager.ForEach(item => item.AfterUnequip(this, unequippedGun));
    }

    void Sight.ISubscriber.OnEnter(GameObject enteringObject)
    {
        if (CompareTag("Player"))
        {
            Gun2D gun = enteringObject.GetComponent<Gun2D>();
            if(gun != null)
                gunsInSight.Add(gun);
        }
    }

    void Sight.ISubscriber.OnExit(GameObject exitingObject)
    {
        if (CompareTag("Player"))
        {
            Gun2D gun = exitingObject.GetComponent<Gun2D>();
            if (gun != null)
                gunsInSight.Remove(gun);
        }
    }
}
