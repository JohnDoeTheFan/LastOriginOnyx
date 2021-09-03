using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeAttack : MonoBehaviourBase
{
    [SerializeField] float damage;
    [SerializeField] float activationTime;
    [SerializeField] float colliderActivationTime;
    [SerializeField] private Vector2 knockBackVelocity;
    [SerializeField] float stiffenTime;

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

            IHitReactor.HitResult hitResult = hitReactor.Hit(IHitReactor.HitType.MeleeAttackStrike, damage, rotatedKnockBack, stiffenTime);
            subscriberManager.ForEach((item) => item.OnHit(this, hitReactor, hitResult));
        }
    }

    public interface ISubscriber
    {
        void OnHit(MeleeAttack attack, IHitReactor hitReactor, IHitReactor.HitResult hitResult);
    }
}

