using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IdeaImplementation;

public class MyObject : MonoBehaviour
{
    [SerializeField]
    private float _durability;
    public float Durability { get { return _durability; } }

    public float maximumDurability;

    public bool IsBroken { get { return _durability == 0; } }

    [Header("DurabilityOptions")]
    public bool canOverRecover = false;
    public float overRecoverLimit = 0;
    public bool canBeUnbrokenByDurability = false;

    public enum TempertureStatus
    {
        None,
        Burning,
        Frozen
    }

    [Header("Burn")]
    public bool isBurnable = true;
    private MusicBox burnMusicBox;

    [Header("Freeze")]
    public bool isFreezable = true;
    private MusicBox freezeMusicBox;

    private TempertureStatus _tempertureStatus = TempertureStatus.None;
    public TempertureStatus CurrentTempertureStatus { get { return _tempertureStatus; } }

    public enum WetnessStatus
    {
        None,
        Wet
    }

    public WetnessStatus wetnessStatus = WetnessStatus.None;

    [Header("ElectricConduction")]
    public bool isConductible = true;

    public void Damage(float damageValue)
    {
        _durability = Mathf.Max(_durability - damageValue, 0);
    }

    public void Recover(float recoverValue)
    {
        float maximumDurabilityWithOverRecover = maximumDurability + (canOverRecover? overRecoverLimit : 0);

        if ( ! IsBroken)
        {
            _durability = Mathf.Min(_durability + recoverValue, maximumDurabilityWithOverRecover);
        }
        else if(IsBroken && canBeUnbrokenByDurability)
        {
            _durability = Mathf.Min(_durability + recoverValue, maximumDurabilityWithOverRecover);
        }
        else
        {
            Debug.LogError("Undefined damage situation!");
        }
    }

    public void Burn()
    {
        if (CurrentTempertureStatus == TempertureStatus.Frozen)
        {
            _tempertureStatus = TempertureStatus.None;
        }
        else if (CurrentTempertureStatus == TempertureStatus.None)
        {
            _tempertureStatus = TempertureStatus.Burning;
            if (burnMusicBox != null)
                burnMusicBox.Stop();
            burnMusicBox = new MusicBox(this, OnBurnTick, OnBurnEnd, MusicBox.Type.DoWhileStyle, 10, 0.5f);
        }
    }

    private void OnBurnTick()
    {
        Damage(0.02f);
    }

    private void OnBurnEnd()
    {
        burnMusicBox.Stop();
        burnMusicBox = null;
    }

    private void OnFreezeTick()
    {
        Damage(0.02f);
    }

    private void OnFreezeEnd()
    {
        freezeMusicBox.Stop();
        freezeMusicBox = null;
    }

    public void Freeze()
    {

        if (CurrentTempertureStatus == TempertureStatus.Burning)
        {
            _tempertureStatus = TempertureStatus.None;
            if (burnMusicBox != null)
                burnMusicBox.Stop();
        }
        else if (CurrentTempertureStatus == TempertureStatus.None)
        {
            _tempertureStatus = TempertureStatus.Frozen;
            if (freezeMusicBox != null)
                freezeMusicBox.Stop();
            freezeMusicBox = new MusicBox(this, OnFreezeTick, OnFreezeEnd, MusicBox.Type.DoWhileStyle, 10, 0.5f);
        }
    }
}
