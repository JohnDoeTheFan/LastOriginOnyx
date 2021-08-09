using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GunGui : MonoBehaviour, Gun2D.ISubscriber, Loader<Bullet>.ISubscriber
{
    [SerializeField]
    private Image gunImage;
    [SerializeField]
    private Text gunName;
    [SerializeField]
    private Text remainBullet;
    [SerializeField]
    private Text magazineSize;
    [SerializeField]
    private RectTransform BulletInfoPanel;
    [SerializeField]
    private Image bulletImage;
    [SerializeField]
    private Text bulletName;

    private IDisposable gunUnsubscriber;
    private IDisposable loaderUnsubscriber;

    private void OnDestroy()
    {
        gunUnsubscriber?.Dispose();
        loaderUnsubscriber?.Dispose();
    }

    public void UpdateGunInfo(Gun2D gun)
    {
        gameObject.SetActive(true);

        gunImage.sprite = gun.Image;
        gunImage.preserveAspect = true;
        gunName.text = gun.name;
    }

    public void EmptyGunInfo()
    {
        gameObject.SetActive(false);

        gunImage.sprite = null;
        gunImage.preserveAspect = true;
        gunName.text = "";
        remainBullet.text = "";
        magazineSize.text = "";

        EmptyBulletInfo();
    }

    public void UpdateBulletInfo(Loader<Bullet> loader)
    {
        if (loader.Item != null)
        {
            BulletInfoPanel.gameObject.SetActive(true);

            remainBullet.text = loader.Quantity.ToString();
            magazineSize.text = loader.Capacity.ToString();
            bulletImage.sprite = loader.Item.Image;
            gunImage.preserveAspect = true;
            bulletName.text = loader.Item.name;
        }
        else
            EmptyBulletInfo();
    }

    public void EmptyBulletInfo()
    {
        BulletInfoPanel.gameObject.SetActive(false);

        remainBullet.text = "0";
        magazineSize.text = "0";
        bulletImage.sprite = null;
        gunImage.preserveAspect = true;
        bulletName.text = "";
    }

    public void SetGun(Gun2D gun)
    {
        gunUnsubscriber?.Dispose();
        loaderUnsubscriber?.Dispose();
        gunUnsubscriber = gun.SubscribeManager.Subscribe(this);
        loaderUnsubscriber = gun.Loader.SubscribeManager.Subscribe(this);

        UpdateGunInfo(gun);
        UpdateBulletInfo(gun.Loader);
    }

    public void EmptyGun()
    {
        gunUnsubscriber?.Dispose();
        loaderUnsubscriber?.Dispose();

        EmptyGunInfo();
        EmptyBulletInfo();
    }

    void Gun2D.ISubscriber.BeforeDestroy(Gun2D gun)
    {
        gunUnsubscriber?.Dispose();

        EmptyGunInfo();
        EmptyBulletInfo();
    }

    void Loader<Bullet>.ISubscriber.OnChange(Loader<Bullet> loader)
    {
        UpdateBulletInfo(loader);
    }

    void Gun2D.ISubscriber.OnFire(Gun2D gun, Bullet bullet)
    {
    }
}

