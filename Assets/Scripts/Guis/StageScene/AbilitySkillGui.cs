using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Onyx.Ability;

public class AbilitySkillGui : MonoBehaviour, AbilitySkill.ISubscriber
{
    [SerializeField] private Image skillImage;
    [SerializeField] private Image coolTimeIndicator;
    [SerializeField] private Graphic availableIndicator;

    private AbilitySkill abilitySkill;
    private IDisposable unsubscriber;

    public void SetAbilitySkill(AbilitySkill abilitySkill)
    {
        UnsetAbilitySkill();

        this.abilitySkill = abilitySkill;

        skillImage.sprite = this.abilitySkill.SkillImage;
        unsubscriber = this.abilitySkill.SubscribeManager.Subscribe(this);
    }

    public void UnsetAbilitySkill()
    {
        abilitySkill = null;
        skillImage.sprite = null;
        unsubscriber?.Dispose();
    }

    void AbilitySkill.ISubscriber.OnRemainCoolTimeChanged(AbilitySkill abilitySkill)
    {
        float fillAmount = (abilitySkill.CoolTime != 0) ? Mathf.Clamp(abilitySkill.RemainCoolTime / abilitySkill.CoolTime, 0, 1) : 0;

        coolTimeIndicator.fillAmount = fillAmount;
    }

    void AbilitySkill.ISubscriber.OnAvailableChanged(AbilitySkill abilitySkill)
    {
        bool active = !abilitySkill.IsAvailable;

        availableIndicator.gameObject.SetActive(active);
    }

    public void OnPointerDown()
    {
        abilitySkill.OnSkillTouchDown();
    }

    public void OnPointerUp()
    {
        abilitySkill.OnSkillTouchUp();
    }

}
