using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

public class Gun2D : TangibleComponent
{
    // Serialized fields
    [Space]
    [SerializeField]
    private GunSpecification spec;
    [Space]
    [SerializeField]
    private ShootType currentShootType;
    [SerializeField]
    private Transform muzzle;
    [SerializeField]
    private Bullet defaultBullet;
    [SerializeField]
    private Loader<Bullet> loader;
    [SerializeField]
    private float loadingTime = 1.5f;
    [SerializeField]
    private Material focusMaterial;

    // Fields
    private bool isTriggerPulled = false;
    private bool isTriggering = false;
    private SpriteRenderer spriteRenderer;
    private AudioSource fireSound;
    private Material defaultMaterial;

    // Settable properties
    public float RemainCoolTime { private set; get; } = 0f;
    public SubscribeManagerTemplate<ISubscriber> SubscribeManager { private set; get; } = new SubscribeManagerTemplate<ISubscriber>();

    // Properties
    public GunSpecification Specification => spec;
    public Loader<Bullet> Loader => loader;
    public bool IsCool => RemainCoolTime <= 0;
    public float LoadingTime => loadingTime;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        fireSound = GetComponent<AudioSource>();
    }

    // Unity Messages
    private void Start()
    {
        StartStateMachine();
        StartCoroutines();
    }

    private void OnDestroy()
    {
        SubscribeManager.ForEach(item => item.BeforeDestroy(this));
    }

    void StartStateMachine()
    {
        IEnumerator SemiAuto()
        {
            Fire();
            yield return new WaitUntil(() => IsCool);
        }

        IEnumerator Burst()
        {
            for (int i = 0; i < spec.BurstNum && !Loader.IsEmpty; i++)
            {
                Fire();
                yield return new WaitUntil(() => IsCool);
            }
        }

        IEnumerator FullAuto()
        {
            while (true)
            {
                Fire();

                yield return new WaitUntil(() => IsCool);

                if (!isTriggering)
                    yield break;
            }
        }

        var IdleState = new EmptyState();
        var semiAutoState = new State(SemiAuto);
        var burstState = new State(Burst);
        var fullAutoState = new State(FullAuto);

        IdleState.AddTransition(() => isTriggerPulled && currentShootType == ShootType.SemiAuto, semiAutoState);
        IdleState.AddTransition(() => isTriggerPulled && currentShootType == ShootType.Burst, burstState);
        IdleState.AddTransition(() => isTriggerPulled && currentShootType == ShootType.FullAuto, fullAutoState);
        semiAutoState.AddTransition(IdleState);
        burstState.AddTransition(IdleState);
        fullAutoState.AddTransition(IdleState);

        StartCoroutine(IdleState.Start(this));
    }

    void StartCoroutines()
    {
        IEnumerator ReduceCooltime()
        {
            while (true)
            {
                yield return new WaitForSeconds(TargetFrameSeconds);
                RemainCoolTime = Mathf.Max(0, RemainCoolTime - TargetFrameSeconds);
            }
        }

        StartCoroutine(ReduceCooltime());
    }

    // Public methods
    public void PullTrigger()
    {
        isTriggerPulled = true;
        isTriggering = true;

        StartCoroutine(Job(WaitForEndOfFrameRoutine, () => isTriggerPulled = false));
    }

    public void ReleaseTrigger()
    {
        isTriggering = false;
    }

    public virtual void Fire()
    {
        if(IsCool)
        {
            Bullet bulletToShot = null;

            if (Loader.HasEnough(1))
                bulletToShot = Loader.Pop(1).item;
            else
                bulletToShot = defaultBullet;

            if(bulletToShot != null)
            {
                Bullet newBullet = Instantiate<Bullet>(bulletToShot, muzzle.position, muzzle.rotation);
                SubscribeManager.ForEach(item => item.OnFire(this, newBullet));
                newBullet.shootSourceId = GetInstanceID();
                newBullet.Propel(spec.BulletSpeed);
                fireSound.Play();

                SetCoolTime();
            }
        }
    }

    // Private methods
    private void SetCoolTime()
    {
        RemainCoolTime = 60f / spec.BulletPerMinute;
    }

    public void SetFocus(bool focus)
    {
        if(spriteRenderer != null)
        {
            if (focus)
            {
                defaultMaterial = spriteRenderer.material;
                spriteRenderer.material = focusMaterial;
            }
            else
            {
                if (defaultMaterial != null)
                    spriteRenderer.material = defaultMaterial;
            }
        }
    }

    // Definitions
    public enum ShootType
    {
        SemiAuto,
        Burst,
        FullAuto
    }

    [System.Serializable]
    public class GunSpecification
    {
        public enum GunModelName
        {
            RandomGun,
            ShotGun,
            AssaultRifle,
            AssaultRifle2,
            SniperRifle,
            GrenadeLauncher
        }

        [SerializeField]
        private GunModelName modelName;
        [SerializeField]
        private int burstNum = 3;
        [SerializeField]
        private int bulletPerMinute = 100;
        [SerializeField]
        private float bulletSpeed = 500;
        [SerializeField]
        private List<ShootType> shootTypes;
        [SerializeField]
        private List<Bullet> availableBullets;

        public GunModelName ModelName => modelName;
        public ReadOnlyCollection<ShootType> ShootTypes => shootTypes.AsReadOnly();
        public ReadOnlyCollection<Bullet> AvailableBullets => availableBullets.AsReadOnly();
        public int BurstNum => burstNum;
        public int BulletPerMinute => bulletPerMinute;
        public float BulletSpeed => bulletSpeed;
    }

    public interface ISubscriber
    {
        void BeforeDestroy(Gun2D gun);

        void OnFire(Gun2D gun, Bullet bullet);
    }
}
