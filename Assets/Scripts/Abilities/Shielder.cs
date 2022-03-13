using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Onyx.Ability;

public class Shielder : AbilityBase
{
    [SerializeField] private Shield shield;
    [SerializeField] UnityEngine.Events.UnityEvent onBlocked;

    private Coroutine shieldEnableCoroutine = null;
    bool isAbilityDisabled = false;

    protected override void Start()
    {
        base.Start();
    }

    public override void InstantiateAbilitySpecificGui(RectTransform abilitySpecificGuiArea)
    {
        return;
    }

    public override void OnKillEnemy(Transform enemyTransform)
    {
        base.OnKillEnemy(enemyTransform);
    }

    public override void OnHit(IHitReactor.HitInfo hitInfo)
    {
        if(! isAbilityDisabled)
        {
            if (shieldEnableCoroutine != null)
                StopCoroutine(shieldEnableCoroutine);
            shield.EnableFunctionality(false);

            shieldEnableCoroutine = StartCoroutine(Job(() => WaitForSecondsRoutine(hitInfo.stiffenTime),
                () => {
                    shield.EnableFunctionality(true);
                }));
        }
    }

    public override void OnDead()
    {
        isAbilityDisabled = true;

        if (shieldEnableCoroutine != null)
            StopCoroutine(shieldEnableCoroutine);
        shield.EnableFunctionality(false);
    }

    public override IHitReactor.HitReaction ReactBeforeHit(IHitReactor.HitInfo hitInfo)
    {
        if(shield.IsFunctionalityEnabled && !hitInfo.isPenetration)
        {
            Vector3 acceptingDirection = hitInfo.direction * -1;
            float acceptingAngle = Vector2.SignedAngle(Vector2.up, acceptingDirection);

            float blockAngleMin = shield.RotatedBlockingAngle - (shield.BlockingArc / 2);
            float blockAngleMax = shield.RotatedBlockingAngle + (shield.BlockingArc / 2);

            bool isBlocked = blockAngleMin <= acceptingAngle && blockAngleMax >= acceptingAngle;

            if (isBlocked)
                onBlocked.Invoke();

            return new IHitReactor.HitReaction(isBlocked);
        }
        else
        {
            return new IHitReactor.HitReaction(false);
        }
    }
}
