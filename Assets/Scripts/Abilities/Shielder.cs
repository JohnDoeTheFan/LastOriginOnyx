using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Onyx.Ability;

public class Shielder : AbilityBase, Shield.ISubscriber
{
    [SerializeField]
    private Shield shield;

    private IDisposable unsubscriber;

    protected override void Start()
    {
        base.Start();
        unsubscriber = shield.SubscribeManager.Subscribe(this);
    }

    private void OnDestroy()
    {
        unsubscriber?.Dispose();
    }

    public override void InstantiateAbilitySpecificGui(RectTransform abilitySpecificGuiArea)
    {
        return;
    }

    void Shield.ISubscriber.OnBlock(float damage)
    {
        Debug.Log(gameObject + " Blocked " + damage);
    }
}
