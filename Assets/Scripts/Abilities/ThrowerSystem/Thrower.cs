using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Onyx.Ability;

public class Thrower : AbilityBase, ThrowBulletBundle.IThrowBulletBundleReactor
{
    [SerializeField] private Sprite skillImage;
    [SerializeField] private Transform throwStartTransform;
    [SerializeField] private Bullet bullet;
    [SerializeField] private int bulletCount;
    [SerializeField] private int maxBulletCount;
    [SerializeField] private float throwForce;
    [SerializeField] private float rewardDropRate;
    [SerializeField] private GameObject killReward;
    [SerializeField] private AbilitySpecificGui<Thrower> abilitySpecificGui;
    [SerializeField] private MultiplierPerLevel damageMultipliers;

    public Bullet Bullet => bullet;
    public int BulletCount => bulletCount;
    public int MaxBulletCount => maxBulletCount;

    private UnsubscriberPack bulletUnsubscriberPack = new UnsubscriberPack();
    public SubscribeManagerTemplate<ISubscriber> SubscribeManager { private set; get; } = new SubscribeManagerTemplate<ISubscriber>();

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        skills.Add(new ButtonActiveAbilitySKill("Throw", skillImage, 0.1f, new SkillDescription(), StartCoroutine, Throw));
    }

    public override void InstantiateAbilitySpecificGui(RectTransform abilitySpecificGuiArea)
    {
        if (abilitySpecificGui != null)
        {
            AbilitySpecificGui<Thrower> newGui = Instantiate(abilitySpecificGui, abilitySpecificGuiArea);
            newGui.SetAbility(this);
        }
    }

    public void Throw()
    {
        if(bulletCount > 0)
        {
            Bullet newBullet = Instantiate<Bullet>(bullet, throwStartTransform.position, throwStartTransform.rotation);
            bulletUnsubscriberPack.Add(new BulletSubscriber(this, newBullet));

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

            newBullet.Propel(throwForce);

            bulletCount--;
            SubscribeManager.ForEach(item => item.OnBulletCountChanged(this));
        }
    }

    public override void OnKillEnemy(Transform enemyTransform)
    {
        if (killReward != null && UnityEngine.Random.value < rewardDropRate)
        {
            GameObject reward = Instantiate<GameObject>(killReward, enemyTransform.position, Quaternion.identity);
            Rigidbody2D rigidBody = reward.GetComponent<Rigidbody2D>();
            if (rigidBody != null)
            {
                float velocityY = 3f;
                float velocityX = UnityEngine.Random.Range(-velocityY / 2, velocityY / 2);
                Vector2 impulse = new Vector2(velocityX * rigidBody.mass, velocityY * rigidBody.mass);
                rigidBody.AddForce(impulse, ForceMode2D.Impulse);
            }
        }
    }

    bool ThrowBulletBundle.IThrowBulletBundleReactor.AddBullet()
    {
        if (bulletCount < maxBulletCount)
        {
            bulletCount++;
            SubscribeManager.ForEach(item => item.OnBulletCountChanged(this));
            return true;
        }
        else
        {
            return false;
        }

    }

    private class BulletSubscriber : IUnsubscriber, Bullet.ISubscriber
    {
        readonly private Thrower thrower;
        readonly private IDisposable unsubscriber;
        private Action onUnsubscribe;

        public BulletSubscriber(Thrower thrower, Bullet bullet)
        {
            unsubscriber = bullet.SubscribeManager.Subscribe(this);

            this.thrower = thrower;
        }

        void Bullet.ISubscriber.BeforeDestroy(Bullet bullet)
        {
            Unsubscribe();
        }

        void Bullet.ISubscriber.OnExplode(Bullet bullet, Explosion explosion)
        {
        }

        void Bullet.ISubscriber.OnHit(Bullet bullet, IHitReactor hitReactor, IHitReactor.HitResult hitResult)
        {
            if (thrower.CompareTag("Player") && hitResult.isKilledByHit)
            {
                thrower.abilityHolder.NotifyKillEnemy(hitReactor.GameObject);
            }
        }

        public void SetOnUnsubscribe(Action onUnsubscribe)
        {
            this.onUnsubscribe = onUnsubscribe;
        }

        public void Unsubscribe()
        {
            unsubscriber?.Dispose();
            onUnsubscribe?.Invoke();
        }
    }

    public interface ISubscriber
    {
        void OnBulletCountChanged(Thrower thrower);
    }
}
