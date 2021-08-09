using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

public partial class Bullet : TangibleComponent, IHitReactor
{
    [SerializeField]
    private Type type;
    [SerializeField]
    protected float hitDamage = 0.1f;
    [SerializeField]
    protected float hitDamageMultiplier = 1f;
    [Header("KillOption")]
    [SerializeField]
    private bool shouldKilledByDistance = true;
    [SerializeField]
    private bool shouldKilledByTime = true;
    [SerializeField]
    private float distanceLimit = 100;
    [SerializeField]
    private float timeLimit = 5f;

    [HideInInspector]
    public int shootSourceId;
    protected List<GameObject> collisionException = new List<GameObject>();

    protected Vector3 startPosition;
    protected float startTime;
    protected Rigidbody2D rigidBody;

    public float multipliedHitDamage => hitDamage * hitDamageMultiplier;

    public SubscribeManagerTemplate<ISubscriber> SubscribeManager { private set; get; } = new SubscribeManagerTemplate<ISubscriber>();

    Vector3 IHitReactor.GetWorldPosition => transform.position;

    protected void Awake()
    {
        rigidBody = GetComponent<Rigidbody2D>();
    }

    protected void Update()
    {
        if (shouldKilledByDistance && Vector3.Distance(startPosition, transform.position) > distanceLimit)
            Destroy(gameObject);

        if (shouldKilledByTime && Time.time - startTime > timeLimit)
            Destroy(gameObject);
    }

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
            SubscribeManager.ForEach(item => item.OnHit(this, collision));
            Ricochet(collision);
        }
        else
        {
            IHitReactor.HitResult hitResult = reactor.Hit(IHitReactor.HitType.Bullet, multipliedHitDamage);
            SubscribeManager.ForEach(item => item.OnHit(this, reactor, hitResult));
            Ricochet(collision);
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
            SubscribeManager.ForEach(item => item.OnHit(this, collision));
            Ricochet(collision);
        }
        else
        {
            IHitReactor.HitResult hitResult = reactor.Hit(IHitReactor.HitType.Bullet, multipliedHitDamage);
            SubscribeManager.ForEach(item => item.OnHit(this, reactor, hitResult));
            Ricochet(collision);
        }
    }

    private void OnDestroy()
    {
        SubscribeManager.ForEach(item => item.BeforeDestroy(this));
    }

    public virtual void Propel(float power)
    {
        startPosition = transform.position;
        startTime = Time.time;

        rigidBody.gravityScale = 0;

        StartCoroutine(AccelerateInNextFrame(transform.rotation * new Vector3(0, power, 0)));

        IEnumerator AccelerateInNextFrame(Vector3 power)
        {
            yield return new WaitForEndOfFrame();
            rigidBody.AddForce(power);
        }
    }

    public virtual void Ricochet(Collider2D collision)
    {
        GetComponent<Collider2D>().enabled = false;

        rigidBody.gravityScale = 0.5f;
        rigidBody.velocity = Vector2.zero;
        rigidBody.AddForce((UnityEngine.Random.insideUnitCircle + Vector2.up) * 100);
        rigidBody.AddTorque(UnityEngine.Random.Range(-1000, 1000));
    }

    public virtual void Ricochet(Collision2D collision)
    {
        GetComponent<Collider2D>().enabled = false;

        rigidBody.gravityScale = 0.5f;
        rigidBody.velocity = Vector2.zero;
        rigidBody.AddForce((UnityEngine.Random.insideUnitCircle + Vector2.up) * 100);
        rigidBody.AddTorque(UnityEngine.Random.Range(-1000, 1000));
    }

    public void AddCollisionException(GameObject gameObject)
    {
        collisionException.Add(gameObject);
    }

    public void SetHitDamage(float damage)
    {
        hitDamage = damage;
    }

    IHitReactor.HitResult IHitReactor.Hit(IHitReactor.HitType type, float damage, Vector3 force)
    {
        return new IHitReactor.HitResult(0, false);
    }

    public enum Type
    {
        AssaultBullet,
        GrenadeBullet,
        ShotBullet,
        SniperBullet,
        HealBullet
    }

    public interface ISubscriber
    {
        void OnHit(Bullet bullet, Collider2D collider);

        void OnHit(Bullet bullet, Collision2D collision);

        void OnHit(Bullet bullet, IHitReactor hitReactor, IHitReactor.HitResult hitResult);

        void BeforeDestroy(Bullet bullet);

        void OnExplode(Bullet bullet, Explosion explosion);
    }
}