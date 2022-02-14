using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeAttack : MonoBehaviourBase
{
    [SerializeField] float damage;
    [SerializeField] float activationTime;
    [SerializeField] float colliderActivationTime;
    [SerializeField] private Vector2 knockBackVelocity;
    [SerializeField] private Vector2 attackDirection;
    [SerializeField] float stiffenTime;
    [SerializeField] bool isPenetration = false;

    public SubscribeManagerTemplate<ISubscriber> subscriberManager { private set; get; } = new SubscribeManagerTemplate<ISubscriber>();

    private Collider2D attackCollider2D;

    public void Awake()
    {
        attackCollider2D = GetComponent<Collider2D>();
    }

    public void Activate()
    {
        gameObject.SetActive(true);
        attackCollider2D.enabled = true;
        StartCoroutine(Job(() => WaitForSecondsRoutine(activationTime), () => gameObject.SetActive(false)));
        StartCoroutine(Job(() => WaitForSecondsRoutine(colliderActivationTime), () => attackCollider2D.enabled = false));
    }
    public void Activate(Func<bool> deactivateCondition)
    {
        gameObject.SetActive(true);
        attackCollider2D.enabled = true;
        StartCoroutine(Job(() => WaitUntilAndForSecondsRoutine(activationTime, deactivateCondition), () => gameObject.SetActive(false)));
        StartCoroutine(Job(() => WaitUntilAndForSecondsRoutine(colliderActivationTime, deactivateCondition), () => attackCollider2D.enabled = false));

        IEnumerator WaitUntilAndForSecondsRoutine(float seconds, Func<bool> condition)
        {
            yield return new WaitForSeconds(seconds);
            yield return new WaitUntil(condition);
        }
    }

    public void SetDamage(float damage)
    {
        this.damage = damage;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        IHitReactor hitReactor = collision.gameObject.GetComponent<IHitReactor>();
        if (hitReactor != null)
        {
            Vector2 rotatedKnockBack = knockBackVelocity;
            if (transform.rotation.y != 0)
                rotatedKnockBack.x *= -1;

            Vector2 rotatedAttackDirection = attackDirection;
            if (transform.rotation.y != 0)
                rotatedAttackDirection.x *= -1;

            IHitReactor.HitResult hitResult = hitReactor.Hit(new IHitReactor.HitInfo(IHitReactor.HitType.MeleeAttackStrike, damage, rotatedAttackDirection.normalized, isPenetration, rotatedKnockBack, stiffenTime));
            subscriberManager.ForEach((item) => item.OnHit(this, hitReactor, hitResult));
        }
    }

    public interface ISubscriber
    {
        void OnHit(MeleeAttack attack, IHitReactor hitReactor, IHitReactor.HitResult hitResult);
    }
}

