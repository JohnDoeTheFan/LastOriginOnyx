using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

public class Explosion : TangibleComponent
{
    [SerializeField] private bool shouldLerpDamage = true;
    [SerializeField] private float maxDamage = 0.5f;
    [SerializeField] private float minDamage = 0f;
    [SerializeField] private float knockBackPower = 10f;
    [SerializeField] private float stiffenTime = 0.5f;

    private CircleCollider2D circleCollider;
    protected List<GameObject> collisionException = new List<GameObject>();

    public SubscribeManagerTemplate<ISubscriber> SubscribeManager { private set; get; } = new SubscribeManagerTemplate<ISubscriber>();

    public float MinDamageForLerp => minDamage;
    public float Radius => circleCollider.radius;

    private void Awake()
    {
        circleCollider = GetComponent<CircleCollider2D>();
    }

    private void Start()
    {
        StartCoroutine(Job(() => WaitForSecondsRoutine(15/60f),   () => circleCollider.enabled = false));
        StartCoroutine(Job(() => WaitForSecondsRoutine(1f),       () => Destroy(gameObject)));
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collisionException.Contains(collision.gameObject))
            return;

        IHitReactor reactor = collision.gameObject.GetComponent<IHitReactor>();
        IHitReactor.HitResult hitResult = new IHitReactor.HitResult(0, false);
        if (reactor != null)
        {
            Vector2 hitPoint = collision.ClosestPoint(transform.position);

            float damage = CalcDamage(hitPoint);

            Vector2 normalizedDiff = (collision.transform.position - transform.position).normalized;
            Vector2 knockBack = normalizedDiff * knockBackPower;

            hitResult = reactor.Hit(new IHitReactor.HitInfo(IHitReactor.HitType.Bullet, damage, normalizedDiff, false, knockBack, stiffenTime));
            SubscribeManager.ForEach(item => item.OnHitExplosion(this, reactor, hitResult));
        }
        else
            SubscribeManager.ForEach(item => item.OnHitExplosion(this, collision));
    }

    private void OnDestroy()
    {
        SubscribeManager.ForEach(item => item.BeforeDistroy(this));
    }

    public void SetMaxDamage(float maxDamage)
    {
        this.maxDamage = maxDamage;
    }

    private float CalcDamage(Vector3 position)
    {
        if(shouldLerpDamage)
        {
            float distance = Vector2.Distance(transform.position, position);
            return Mathf.Lerp(maxDamage, MinDamageForLerp, distance / Radius);
        }
        else
            return maxDamage;
    }

    public void AddCollisionException(GameObject gameObject)
    {
        collisionException.Add(gameObject);
    }

    public interface ISubscriber
    {
        void OnHitExplosion(Explosion explosion, Collider2D collision);
        void OnHitExplosion(Explosion explosion, IHitReactor hitReactor, IHitReactor.HitResult hitResult);
        void BeforeDistroy(Explosion explosion);
    }
}
