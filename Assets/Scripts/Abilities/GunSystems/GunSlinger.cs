using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using Onyx.Ability;

public abstract class GunSlinger : AbilityBase, Gun2D.ISubscriber, BulletBundle.IBulletBundleReactor
{
    public static Action<GunSlinger> OnStart;

    [SerializeField] protected Gun2D equippedGun;
    [SerializeField] protected Transform hand;
    [SerializeField] protected Sprite shootSkillImage;
    [SerializeField] protected Sprite reloadSkillImage;
    [SerializeField] private AbilitySpecificGui<GunSlinger> abilitySpecificGui;
    [SerializeField] private AudioSource reloadAudioSource;
    [SerializeField] private List<GameObject> killRewards;
    [SerializeField] private MultiplierPerLevel damageMultipliers;
    [SerializeField] private RectTransform bulletEmptyNotifier;

    private bool isFiring = false;
    private LoadingStatus loadingStatus = LoadingStatus.ShouldStopLoad;

    private float remainLoadingTime = 0f;

    protected float gunHandAnchorHeightAdjust;
    protected float gunHandAnchorRadius;
    protected float gunHandAnchorZ;

    protected IDisposable gunUnsubscriber;

    private AbilitySkill attackSkill;

    public Gun2D EquippedGun => equippedGun;
    public float RemainLoadingTime => remainLoadingTime;
    public int SelectedLoaderIndex { protected set; get; } = 0;
    public LoaderSetComponent<Bullet> LoaderSet { private set; get; }
    public ReadOnlyCollection<Loader<Bullet>> AvailableLoaders { private set; get; }
    public ReadOnlyCollection<Loader<Bullet>> NotAvailableLoaders { private set; get; }
    public SubscribeManagerTemplate<ISubscriber> SubscribeManager { private set; get; } = new SubscribeManagerTemplate<ISubscriber>();
    public abstract bool CanChangeGun { get; }
    public abstract float RemainEquipCoolTime { get; }

    private readonly UnsubscriberPack shootRelationPack = new UnsubscriberPack();

    protected void Awake()
    {
        LoaderSet = GetComponent<BulletLoaderSet>();
        AvailableLoaders = new ReadOnlyCollection<Loader<Bullet>>(new List<Loader<Bullet>>());
        NotAvailableLoaders = new ReadOnlyCollection<Loader<Bullet>>(new List<Loader<Bullet>>());
    }

    protected override void Start()
    {
        base.Start();

        gunHandAnchorRadius = Mathf.Abs(hand.localPosition.x);
        gunHandAnchorZ = hand.position.z;
        gunHandAnchorHeightAdjust = hand.localPosition.y;

        ClassifyAvailableBullets();

        StartStateMachine();

        StartCoroutines();

        OnStart?.Invoke(this);

        attackSkill = new ToggleActiveAbilitySKill("Shoot", shootSkillImage, 0, new SkillDescription(), StartCoroutine, StartFire, StopFire);

        skills.Add(attackSkill);
        skills.Add(new ButtonActiveAbilitySKill("Reload", reloadSkillImage, 0.1f, new SkillDescription(), StartCoroutine, StartLoad));
        skills.Add(new ButtonActiveAbilitySKill("Equip", null, RemainEquipCoolTime, new SkillDescription(), StartCoroutine, EquipClosestGun));
        skills.Add(new ButtonActiveAbilitySKill("Unequip", null, 0.1f, new SkillDescription(), StartCoroutine, Unequip));
        skills.Add(new ButtonActiveAbilitySKill("SelectBullet", null, 0f, new SkillDescription(), StartCoroutine, CircularSelectBullet));
    }

    protected virtual void OnDestroy()
    {
        shootRelationPack.UnsubscribeAll();
        SubscribeManager.ForEach(item => item.BeforeDestroy(this));
    }

    public override void InstantiateAbilitySpecificGui(RectTransform abilitySpecificGuiArea)
    {
        if(abilitySpecificGui != null)
        {
            AbilitySpecificGui<GunSlinger>  newGui = Instantiate(abilitySpecificGui, abilitySpecificGuiArea);
            newGui.SetAbility(this);
        }
    }

    void StartStateMachine()
    {
        IEnumerator Firing()
        {
            equippedGun.PullTrigger();
            yield return new WaitWhile(() => isFiring);
            equippedGun.ReleaseTrigger();
        }

        IEnumerator Loading()
        {
            loadingStatus = LoadingStatus.Loading;
            remainLoadingTime = equippedGun.LoadingTime;

            if (reloadAudioSource != null)
                reloadAudioSource.Play();

            yield return new WaitWhile(() => loadingStatus == LoadingStatus.Loading && remainLoadingTime > 0);
            if (remainLoadingTime == 0)
                LoadDone();
        }

        EmptyState noGunState = new EmptyState();
        EmptyState idleState = new EmptyState();
        State fireState = new State(Firing);
        State loadState = new State(Loading);

        noGunState.AddTransition(() => equippedGun != null, idleState);
        idleState.AddTransition(() => equippedGun == null, noGunState);
        idleState.AddTransition(() => isFiring, fireState);
        idleState.AddTransition(() => loadingStatus == LoadingStatus.ShouldStartLoad, loadState);
        fireState.AddTransition(idleState);
        loadState.AddTransition(idleState);

        StartCoroutine(noGunState.Start(this));
    }

    private void StartCoroutines()
    {
        IEnumerator ReduceRemainLoadingTime()
        {
            while (true)
            {
                if(remainLoadingTime == 0)
                    yield return new WaitUntil(() => remainLoadingTime > 0);
                else
                {
                    SubscribeManager.ForEach(item => item.OnUpdateReloadTime(this));

                    float startTime = Time.time;
                    yield return new WaitForSeconds(TargetFrameSeconds);
                    float endTime = Time.time;

                    if (loadingStatus == LoadingStatus.ShouldStopLoad)
                        remainLoadingTime = 0;
                    else
                        remainLoadingTime = Mathf.Max(0, remainLoadingTime - (endTime - startTime));

                    if(remainLoadingTime == 0)
                        SubscribeManager.ForEach(item => item.OnUpdateReloadTime(this));
                }
            }
        }

        StartCoroutine(ReduceRemainLoadingTime());
    }
    public void StartFire()
    {
        if (equippedGun.Loader.IsEmpty && bulletEmptyNotifier != null)
        {
            bulletEmptyNotifier.gameObject.SetActive(false);
            bulletEmptyNotifier.gameObject.SetActive(true);
        }

        if (equippedGun != null && !equippedGun.Loader.IsEmpty)
            StopLoad();

        isFiring = true;
    }

    public void StartLoad()
    {
        if (loadingStatus == LoadingStatus.Loading)
            return;
        if (!AvailableLoaders.IsInRange(SelectedLoaderIndex))
            return;
        if (equippedGun.Loader.IsFull)
            return;

        if (loadingStatus != LoadingStatus.Loading)
        {
            loadingStatus = LoadingStatus.ShouldStartLoad;
        }
    }

    public void StopLoad()
    {
        loadingStatus = LoadingStatus.ShouldStopLoad;
    }

    public void StopFire()
    {
        isFiring = false;
    }

    public void LoadDone()
    {
        Loader<Bullet> loaderToUse = AvailableLoaders[SelectedLoaderIndex];

        List<Bundle<Bullet>> returnedBullets = EquippedGun.Loader.Load(loaderToUse.Pop(EquippedGun.Loader.Capacity));
        AddBullets(returnedBullets);

        SubscribeManager.ForEach(item => item.OnUpdateBullets(this));

        loadingStatus = LoadingStatus.ShouldStopLoad;
    }

    protected void ClassifyAvailableBullets()
    {
        if (LoaderSet == null)
            return;

        List<Loader<Bullet>> newAvailableLoaders = new List<Loader<Bullet>>();
        List<Loader<Bullet>> newNotAvailableLoaders = new List<Loader<Bullet>>();

        ReadOnlyCollection<Bullet> availableBullets = null;
        if (EquippedGun != null)
            availableBullets = EquippedGun.Specification.AvailableBullets;
        else
            availableBullets = new ReadOnlyCollection<Bullet>(new List<Bullet>());

        foreach (var loader in LoaderSet.Loaders)
        {
            if (availableBullets.Contains(loader.Item))
                newAvailableLoaders.Add(loader);
            else
                newNotAvailableLoaders.Add(loader);
        }

        if (AvailableLoaders.IsInRange(SelectedLoaderIndex))
        {
            Loader<Bullet> selectedBulletBackup = AvailableLoaders[SelectedLoaderIndex];

            AvailableLoaders = newAvailableLoaders.AsReadOnly();
            NotAvailableLoaders = newNotAvailableLoaders.AsReadOnly();

            SelectBullet(newAvailableLoaders.FindIndex((loader) => loader == selectedBulletBackup));
        }
        else
        {
            AvailableLoaders = newAvailableLoaders.AsReadOnly();
            NotAvailableLoaders = newNotAvailableLoaders.AsReadOnly();
        }

        SubscribeManager.ForEach(item => item.OnUpdateBullets(this));
    }

    private void AddBullets(List<Bundle<Bullet>> bulletInfos)
    {
        if (bulletInfos.Count == 0)
            return;
        else
        {
            bulletInfos.ForEach(bundle => LoaderSet.Load(bundle));

            bool hadNoAvailableLoader = AvailableLoaders.Count == 0;
            bool shouldLoad = hadNoAvailableLoader;

            ClassifyAvailableBullets();

            if (shouldLoad)
                StartLoad();
        }
    }

    private void SelectBullet(int index)
    {
        if (loadingStatus == LoadingStatus.Loading)
            return;

        if (AvailableLoaders.Count == 0)
            SelectedLoaderIndex = -1;
        if (index > AvailableLoaders.Count - 1)
            SelectedLoaderIndex = 0;
        else if (index < 0)
            SelectedLoaderIndex = 0;
        else
            SelectedLoaderIndex = index;

        SubscribeManager.ForEach(item => item.OnUpdateBullets(this));
    }

    public void CircularSelectBullet()
    {
        CircularSelectBullet(SelectedLoaderIndex + 1);
    }

    public void CircularSelectBullet(int index)
    {
        if (loadingStatus == LoadingStatus.Loading)
            return;

        if (AvailableLoaders.Count == 0)
            SelectedLoaderIndex = -1;
        else if (index > AvailableLoaders.Count - 1)
            SelectedLoaderIndex = 0;
        else if (index < 0)
            SelectedLoaderIndex = AvailableLoaders.Count - 1;
        else
            SelectedLoaderIndex = index;

        SubscribeManager.ForEach(item => item.OnUpdateBullets(this));
    }

    public abstract void EquipClosestGun();
    public abstract void Unequip();
    public override void OnChangeDirection(Quaternion rotation)
    {
        RotateGun(rotation);
    }

    public void RotateGun(Quaternion rotation)
    {
        // Prevent this for two reason.
        // 1: I couldn't finish virtual right stick gui for mobile version. It need more time and idea.
        // 2: It has problem that rotating around center of unit. It should be changed to accept new parameter of transform(not Vector2 or Vector3) then use it to pivot of rotation.
        /*
        Vector3 anchorPosition = new Vector3(transform.position.x, transform.position.y + gunHandAnchorHeightAdjust, gunHandAnchorZ);

        hand.position = anchorPosition + rotation * (transform.rotation * Vector2.right * gunHandAnchorRadius);

        hand.rotation = rotation * transform.rotation;
        */
    }

    public override void OnKillEnemy(Transform enemyTransform)
    {
        if (killRewards.Count != 0)
        {
            int randomIndex = Mathf.FloorToInt(UnityEngine.Random.Range(0, killRewards.Count));
            GameObject reward = Instantiate<GameObject>(killRewards[randomIndex], enemyTransform.position, Quaternion.identity);
            Rigidbody2D rigidBody = reward.GetComponent<Rigidbody2D>();
            if (rigidBody != null)
            {
                float velocityY = 3f;
                float velocityX = UnityEngine.Random.Range(-velocityY / 2, velocityY / 2);
                Vector2 impulse = new Vector2(velocityX, rigidBody.mass * velocityY);
                rigidBody.AddForce(impulse, ForceMode2D.Impulse);
            }
        }
    }

    void Gun2D.ISubscriber.BeforeDestroy(Gun2D gun)
    {
    }

    void Gun2D.ISubscriber.OnFire(Gun2D gun, Bullet bullet)
    {
        bullet.AddCollisionException(gameObject);
        shootRelationPack.Add(new ShootRelation(this, gun, bullet));

        float defaultDamage = 0.1f;

        int level;
        if (AbilityHolder.ShouldUseLevelOfDifficulty)
        {
            level = abilityHolder.LevelOfDifficulty;
        }
        else
        {
            // DOTO: Should damage by gunSlinger.AttackSkill.level not gunSlinger.abilityHolder.Level
            level = abilityHolder.Level;
        }

        float multiplier = damageMultipliers.GetMultiplier(level);
        bullet.SetHitDamage(defaultDamage * multiplier);
    }
    bool BulletBundle.IBulletBundleReactor.AddBullet(Bundle<Bullet> bullets)
    {
        if (CompareTag("Player"))
        {
            LoaderSet.Load(bullets);

            bool hadNoAvailableLoader = AvailableLoaders.Count == 0;
            bool isGunEmpty = equippedGun.Loader.IsEmpty;
            bool shouldLoad = hadNoAvailableLoader && isGunEmpty;

            ClassifyAvailableBullets();

            if (shouldLoad)
                StartLoad();

            return true;
        }
        else
            return false;
    }

    public enum LoadingStatus
    {
        ShouldStartLoad,
        Loading,
        ShouldStopLoad
    }

    public interface ISubscriber
    {
        void AfterUnequip(GunSlinger gunSlinger, Gun2D gun);
        void AfterEquip(GunSlinger gunSlinger, Gun2D gun);
        void BeforeDestroy(GunSlinger gunSlinger);
        void OnUpdateBullets(GunSlinger gunSlinger);
        void OnUpdateReloadTime(GunSlinger gunSlinger);
        void OnChangedClosestGun(GunSlinger gunSlinger, Gun2D closestGun);
    }

    public class SubscriberImpl : ISubscriber
    {
        public virtual void OnChangeEquippableGuns(GunSlinger gunSlinger) { }
        public virtual void BeforeDestroy(GunSlinger gunSlinger) { }
        public virtual void AfterEquip(GunSlinger gunSlinger, Gun2D gun) { }
        public virtual void AfterUnequip(GunSlinger gunSlinger, Gun2D gun) { }
        public virtual void OnUpdateBullets(GunSlinger gunSlinger) { }
        public virtual void OnUpdateReloadTime(GunSlinger gunSlinger) { }
        public virtual void OnChangedClosestGun(GunSlinger gunSlinger, Gun2D closestGun) { }
    }

    public class ShootRelation : IUnsubscriber, Gun2D.ISubscriber, Bullet.ISubscriber
    {
        private readonly GunSlinger gunSlinger;
        private Gun2D gun;
        private readonly IDisposable gunUnsubscriber;
        private readonly Bullet bullet;
        private readonly IDisposable bulletUnsubscriber;
        private Action onUnsubscribe;

        public ShootRelation(GunSlinger gunSlinger, Gun2D gun, Bullet bullet)
        {
            gunUnsubscriber = gun.SubscribeManager.Subscribe(this);
            bulletUnsubscriber = bullet.SubscribeManager.Subscribe(this);

            this.gunSlinger = gunSlinger;
            this.gun = gun;
            this.bullet = bullet;
        }

        void Gun2D.ISubscriber.OnFire(Gun2D gun, Bullet bullet)
        {
            // Must do nothing.
        }

        void Gun2D.ISubscriber.BeforeDestroy(Gun2D gun)
        {
            this.gun = null;
            gunUnsubscriber?.Dispose();
        }

        void Bullet.ISubscriber.OnHit(Bullet bullet, IHitReactor hitReactor, IHitReactor.HitResult hitResult)
        {
            if(gunSlinger.CompareTag("Player") && hitResult.isKilledByHit)
            {
                gunSlinger.abilityHolder.NotifyKillEnemy(hitReactor.GameObject);
            }
        }

        void Bullet.ISubscriber.BeforeDestroy(Bullet bullet)
        {
            Unsubscribe();
        }

        void Bullet.ISubscriber.OnExplode(Bullet bullet, Explosion explosion)
        {
            explosion.AddCollisionException(gunSlinger.gameObject);
        }

        public void Unsubscribe()
        {
            gunUnsubscriber?.Dispose();
            bulletUnsubscriber?.Dispose();
            onUnsubscribe?.Invoke();
        }

        public void SetOnUnsubscribe(Action onUnsubscribe)
        {
            this.onUnsubscribe = onUnsubscribe;
        }
    }
}
