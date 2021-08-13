using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ThrowerSpecificGui : AbilitySpecificGui<Thrower>, Thrower.ISubscriber
{
    [SerializeField] private Image bulletImage;
    [SerializeField] private Text bulletCount;
    [SerializeField] private Text maxBulletCount;

    private IDisposable unsubscriber;
    public override void SetAbility(Thrower ability)
    {
        unsubscriber = ability.SubscribeManager.Subscribe(this);

        bulletImage.sprite = ability.Bullet.Image;
        bulletCount.text = ability.BulletCount.ToString();
        maxBulletCount.text = ability.MaxBulletCount.ToString();
    }

    void Thrower.ISubscriber.OnBulletCountChanged(Thrower thrower)
    {
        bulletCount.text = thrower.BulletCount.ToString();
    }
}
