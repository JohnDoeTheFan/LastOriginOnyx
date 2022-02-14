using System;
using System.Collections;
using UnityEngine;

public class GrenadeBullet : Bullet, Explosion.ISubscriber
{
    [Header("Explosion")]
    [SerializeField]
    private float ExplosionDelayAfterTouch = 1.5f;
    [SerializeField]
    private Explosion explosion;
    [SerializeField]
    private float explosionDamageMultiplier;

    private bool isFirstTouch =  true;
    private IDisposable explosionUnsubscriber;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collisionException.Contains(collision.gameObject))
            return;

        Bullet bullet = collision.gameObject.GetComponent<Bullet>();
        if (bullet != null && bullet.shootSourceId == shootSourceId)
            return;

        IHitReactor reactor = collision.gameObject.GetComponent<IHitReactor>();
        if (reactor == null)
        {
            if (isFirstTouch)
            {
                isFirstTouch = false;
                StartCoroutine(Job(() => WaitForSecondsRoutine(ExplosionDelayAfterTouch), () => Explode()));
            }

            Ricochet(collision);
        }
        else
        {
            if (isFirstTouch)
            {
                isFirstTouch = false;
                IHitReactor.HitResult hitResult = reactor.Hit(new IHitReactor.HitInfo(IHitReactor.HitType.Bullet, multipliedHitDamage, velocityBeforePhysicsUpdate.normalized, false));
                Explode();

                SubscribeManager.ForEach(item => item.OnHit(this, reactor, hitResult));
            }
            else
            {
                Ricochet(collision);
            }
        }
        
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collisionException.Contains(collision.gameObject))
            return;

        Bullet bullet = collision.gameObject.GetComponent<Bullet>();
        if (bullet != null && bullet.shootSourceId == shootSourceId)
            return;

        IHitReactor reactor = collision.gameObject.GetComponent<IHitReactor>();
        if (reactor == null)
        {
            if (isFirstTouch)
            {
                isFirstTouch = false;
                StartCoroutine(Job(() => WaitForSecondsRoutine(ExplosionDelayAfterTouch), () => Explode()));
            }

            Ricochet(collision);
        }
        else
        {
            if (isFirstTouch)
            {
                isFirstTouch = false;
                IHitReactor.HitResult hitResult = reactor.Hit(new IHitReactor.HitInfo(IHitReactor.HitType.Bullet, multipliedHitDamage, velocityBeforePhysicsUpdate.normalized, false));
                Explode();

                SubscribeManager.ForEach(item => item.OnHit(this, reactor, hitResult));
            }
            else
            {
                Ricochet(collision);
            }
        }
    }

    private void OnDestroy()
    {
        explosionUnsubscriber?.Dispose();
        SubscribeManager.ForEach(item => item.BeforeDestroy(this));
    }

    public override void Propel(float power)
    {
        startPosition = transform.position;
        startTime = Time.time;

        rigidBody.AddForce(transform.rotation * new Vector3(0, power, 0) * rigidBody.mass, ForceMode2D.Impulse);
    }

    public override void Ricochet(Collider2D collision)
    {
    }

    public override void Ricochet(Collision2D collision)
    {
    }

    public virtual void Explode()
    {
        gameObject.SetActive(false);
        Explosion newExplosion = Instantiate<Explosion>(explosion, transform.position, transform.rotation);
        explosion.SetMaxDamage(hitDamage * explosionDamageMultiplier);
        SubscribeManager.ForEach(item => item.OnExplode(this, newExplosion));
        explosionUnsubscriber = newExplosion.SubscribeManager.Subscribe(this);
    }

    void Explosion.ISubscriber.OnHitExplosion(Explosion explosion, Collider2D collision)
    {

    }

    void Explosion.ISubscriber.OnHitExplosion(Explosion explosion, IHitReactor hitReactor, IHitReactor.HitResult hitResult)
    {
        SubscribeManager.ForEach(item => item.OnHit(this, hitReactor, hitResult));
    }

    void Explosion.ISubscriber.BeforeDistroy(Explosion explosion)
    {
        explosionUnsubscriber?.Dispose();
        Destroy(gameObject);
    }

}
