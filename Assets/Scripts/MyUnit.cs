﻿using Onyx.Core;
using Onyx.Ability;
using Onyx.GameElement;
using Onyx.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace Onyx
{
    /// <summary>
    /// 게임 내에서 플레이어 또는 적의 역할을 수행하는 GameObject를 만들 수 있는 컴포넌트. 
    /// </summary>
    public class MyUnit : MonoBehaviourBase, InputHandler.IInputReceiver, IHitReactor, IAbilityHolder, HealingItem.IHealingItemReactor
    {
        public delegate void OnEndOfStartHandler(MyUnit myUnit);
        private static event OnEndOfStartHandler onEndOfStartEvent;
        public static event OnEndOfStartHandler OnEndOfStartEvent {
            add
            {
                onEndOfStartEvent = null;
                onEndOfStartEvent += value;
            }
            remove
            {

            }
        }

        [SerializeField] Animator modelAnimator;

        [Header("Status")]
        [SerializeField] private float healthPoint = 1;
        [SerializeField] private float maxHealth = 1;

        [Header("GameElements")]
        [SerializeField] private GroundChecker groundChecker;
        [SerializeField] private Sight sight;

        [Header("Dust")]
        [SerializeField] private ParticleSystem dustParticleSystem;
        [SerializeField] private float dustMakingVelocity = 3f;

        [Header("Audios")]
        [SerializeField] private AudioSource bulletHitAudio;

        [Header("Voices")]
        [SerializeField] private AudioClip stageStartVoice;
        [SerializeField] private AudioClip retireVoice;
        [SerializeField] private AudioClip retreatVoice;
        [SerializeField] private AudioClip stageClearVoice;

        [Header("Levels")]
        [SerializeField] private int level;
        [SerializeField] private int levelOfDifficulty;
        [SerializeField] private MultiplierPerLevel healthMultipliers;
        [SerializeField] private int scoreMultiplier;

        [Header("Etc")]
        [SerializeField] private float defaultStiffenTime = 0.5f;
        [SerializeField] private float hitRecoverTime;

        private Rigidbody2D rigidBody;
        private InputHandler inputHandler;
        readonly private List<IAbility> abilities = new List<IAbility>();
        private ClosestObjectInSightManager<InteractableComponent> closestInteractable;
        private MovementBase movement;

        private bool isDead = false;
        private bool preventControl = false;
        private Vector2 leftStick = new Vector2(0, 0);
        private float remainStiffenTime = 0;
        private float remainHitRecoverTime = 0;

        static private readonly int id_Grounded = Animator.StringToHash("Grounded");
        static private readonly int id_IsRunning = Animator.StringToHash("IsRunning");
        static private readonly int id_IsStiffen = Animator.StringToHash("IsStiffen");
        static private readonly int id_Die = Animator.StringToHash("Die");
        static private readonly int id_Revive = Animator.StringToHash("Revive");
        static private readonly int id_Ceremory = Animator.StringToHash("Ceremony");
        static private readonly int id_Hit = Animator.StringToHash("Hit");

        private bool isControlPrevented => preventControl || (remainStiffenTime > 0f);
        public float HealthPoint => healthPoint;
        public bool IsDead => isDead;
        public AudioClip StageStartVoice => stageStartVoice;
        public AudioClip RetireVoice => retireVoice;
        public AudioClip RetreatVoice => retreatVoice;
        public AudioClip StageClearVoice => stageClearVoice;
        public ReadOnlyCollection<IAbility> Abilities => abilities.AsReadOnly();
        public SubscribeManagerTemplate<ISubscriber> SubscribeManager { private set; get; } = new SubscribeManagerTemplate<ISubscriber>();
        public int ScoreMultiplier => scoreMultiplier;
        public MovementBase Movement => movement;

        int IAbilityHolder.Level => level;
        int IAbilityHolder.LevelOfDifficulty => levelOfDifficulty;
        bool IAbilityHolder.ShouldUseLevelOfDifficulty => !CompareTag("Player");
        bool IAbilityHolder.isFacingLeft => movement.IsFacingLeft;
        bool IAbilityHolder.isMovementOccupied => movement.IsMovementOccupied;
        bool IAbilityHolder.IsGrounded => groundChecker.IsGrounded;
        Animator IAbilityHolder.ModelAnimator => modelAnimator;

        Vector3 IHitReactor.GetWorldPosition => transform.position;
        GameObject IHitReactor.GameObject => gameObject;


        private void Awake()
        {
            rigidBody = GetComponent<Rigidbody2D>();
            movement = GetComponent<MovementBase>();
            inputHandler = GetComponent<InputHandler>();

            abilities.AddRange(GetComponents<IAbility>());
        }

        private void Start()
        {
            inputHandler.AddInputReceiverRegisterAwaiter(this);

            foreach (IAbility ability in abilities)
                ability.SetAbilityHolder(this);

            if (CompareTag("Player"))
            {
                // 가장 가까운 상호작용 가능한 물체에 대한 탐색 코루틴을 실행
                closestInteractable = new ClosestObjectInSightManager<InteractableComponent>(sight, transform, OnChangeInteractable);
                StartCoroutine(closestInteractable.UpdateCoroutine(0.1f));
            }

            // 시작 단계 초기화를 끝낸 후, 이를 알림.
            onEndOfStartEvent?.Invoke(this);
        }

        void Update()
        {
            movement.UpdateMovement(leftStick);

            if(groundChecker != null)
                modelAnimator.SetBool(id_Grounded, groundChecker.IsGrounded);
            modelAnimator.SetBool(id_IsRunning, leftStick != Vector2.zero);

            float characterVelocity = rigidBody.velocity.x;
            if (groundChecker != null)
                characterVelocity -= groundChecker.GetGroundVelocity().x;

            if(groundChecker!= null)
            {
                bool shouldMakeDust = groundChecker.IsGrounded && Mathf.Abs(characterVelocity) > dustMakingVelocity;
                if (shouldMakeDust && !dustParticleSystem.isEmitting)
                    dustParticleSystem.Play();
                else if (!shouldMakeDust && dustParticleSystem.isEmitting)
                    dustParticleSystem.Stop();
            }

            remainStiffenTime = Mathf.Max(0, remainStiffenTime - Time.deltaTime);
            if(remainStiffenTime == 0)
                modelAnimator.SetBool(id_IsStiffen, false);

            remainHitRecoverTime = Mathf.Max(0, remainHitRecoverTime - Time.deltaTime);
        }

        public void Die()
        {
            remainStiffenTime = 0f;
            isDead = true;

            inputHandler.AddInputReceiverUnregisterAwaiter(this);
            leftStick = Vector2.zero;

            gameObject.layer = (int)LayerSetting.DeadBody;

            movement.SetDead();

            modelAnimator.SetBool(id_IsStiffen, false);
            modelAnimator.SetTrigger(id_Die);

            foreach (var ability in abilities)
                ability.OnDead();

            SubscribeManager.ForEach(item => item.OnDeath(this));
        }

        public void Revive()
        {
            isDead = false;
            inputHandler.AddInputReceiverRegisterAwaiter(this);
            TakeHeal(1);
            modelAnimator.SetTrigger(id_Revive);
        }

        public void Ceremony()
        {
            modelAnimator.SetTrigger(id_Ceremory);
        }

        public void TeleportAt(Vector3 position)
        {
            rigidBody.velocity = Vector2.zero;
            transform.position = position;
        }

        public float HandleDamage(float damage)
        {
            if (isDead)
            {
                return 0;
            }
            else
            {
                SubscribeManager.ForEach(item => item.OnDamage(this, damage));

                float healthPointBackup = healthPoint;
                healthPoint = Mathf.Max(0, healthPoint - damage);
                SubscribeManager.ForEach(item => item.OnHealthPointChange(this));

                if (healthPoint == 0)
                    Die();

                modelAnimator.SetTrigger(id_Hit);

                return healthPoint - healthPointBackup;
            }
        }

        public void TakeHeal(float heal)
        {
            if (!isDead)
            {
                SubscribeManager.ForEach(item => item.OnHeal(this, heal));

                healthPoint = Mathf.Min(maxHealth, healthPoint + heal);
                SubscribeManager.ForEach(item => item.OnHealthPointChange(this));

                if (healthPoint == 0)
                    Die();
            }
        }

        /// <summary>
        /// 어빌리티의 스킬에 조작을 전달한다.
        /// </summary>
        /// <param name="abilityIndex">어빌리티 번호</param>
        /// <param name="skillIndex">스킬 번호</param>
        /// <param name="isDown">입력 유형</param>
        private void CallAbilitySkill(int abilityIndex, int skillIndex, bool isDown)
        {
            if (abilityIndex < abilities.Count && skillIndex < abilities[abilityIndex].Skills.Count)
            {
                if (isDown)
                    abilities[abilityIndex].Skills[skillIndex].OnSkillTouchDown();
                else
                    abilities[abilityIndex].Skills[skillIndex].OnSkillTouchUp();
            }
        }

        public void PreventControl(bool prevent)
        {
            preventControl = prevent;
        }

        public void SetLevel(int level, List<IAbility.LevelDescription> abilityLevels)
        {
            this.level = level;
            for(int i = 0; i < Mathf.Min(abilities.Count, abilityLevels.Count); i++)
            {
                abilities[i].SetLevel(abilityLevels[i]);
            }
            // TODO: Seperate heathPoint to MaxHealth and currentHealth. Then, Make gui also works like that.
            maxHealth *= healthMultipliers.GetMultiplier(level);
            healthPoint = maxHealth;
            SubscribeManager.ForEach((item) => item.OnHealthPointChange(this));
        }

        public void SetLevelOfDifficulty(int levelOfDifficulty)
        {
            this.levelOfDifficulty = levelOfDifficulty;
            // TODO: Seperate heathPoint to MaxHealth and currentHealth. Then, Make gui also works like that.
            maxHealth *= healthMultipliers.GetMultiplier(levelOfDifficulty);
            healthPoint = maxHealth;
        }

        void OnChangeInteractable(InteractableComponent oldInteractable, InteractableComponent newInteractable)
        {
            if (oldInteractable != null)
                oldInteractable.ActiveInteractIcon(false, gameObject);
            if (newInteractable != null)
                newInteractable.ActiveInteractIcon(true, gameObject);
        }

        bool HealingItem.IHealingItemReactor.Heal(float health)
        {
            if (CompareTag("Player"))
            {
                TakeHeal(health);
                return true;
            }
            else
                return false;
        }
        bool HealingItem.IHealingItemReactor.HealPercent(float percent)
        {
            if (CompareTag("Player"))
            {
                TakeHeal(maxHealth * percent);
                return true;
            }
            else
                return false;
        }

        void InputHandler.IInputReceiver.OnLeftStick(Vector2 leftStick)
        {
            if (isControlPrevented)
            {
                this.leftStick = new Vector2(0, 0);
                return;
            }

            this.leftStick = leftStick;
        }


        void InputHandler.IInputReceiver.OnRightStick(Vector2 rightStick)
        {
            if (isControlPrevented)
                return;

            if (abilities.Count > 0 && abilities[0] != null)
                abilities[0].OnChangeDirection(Quaternion.Euler(0, 0, Vector2.SignedAngle(Vector2.right, rightStick)));
        }

        void InputHandler.IInputReceiver.OnInteractButtonDown()
        {
            if (isControlPrevented)
                return;

            if (closestInteractable.Closest != null)
                closestInteractable.Closest.Interact(gameObject);
        }

        void InputHandler.IInputReceiver.OnPreservedButtonDown()
        {
        }


        void InputHandler.IInputReceiver.OnAbility2Skill1ButtonDown()
        {
            if (isControlPrevented)
                return;

            CallAbilitySkill(2, 1, true);
        }
        void InputHandler.IInputReceiver.OnAbility2Skill0ButtonDown()
        {
            if (isControlPrevented)
                return;

            CallAbilitySkill(2, 0, true);
        }


        void InputHandler.IInputReceiver.OnAbility1Skill1ButtonDown()
        {
            if (isControlPrevented)
                return;

            CallAbilitySkill(1, 1, true);
        }
        void InputHandler.IInputReceiver.OnAbility1Skill1ButtonUp()
        {
            if (isControlPrevented)
                return;

            CallAbilitySkill(1, 1, false);
        }
        void InputHandler.IInputReceiver.OnAbility1Skill0ButtonDown()
        {
            if (isControlPrevented)
                return;

            CallAbilitySkill(1, 0, true);
        }
        void InputHandler.IInputReceiver.OnAbility1Skill0ButtonUp()
        {
            if (isControlPrevented)
                return;

            CallAbilitySkill(1, 0, false);
        }

        void InputHandler.IInputReceiver.OnAbility0Skill1ButtonDown()
        {
            if (isControlPrevented)
                return;

            CallAbilitySkill(0, 1, true);
        }
        void InputHandler.IInputReceiver.OnAbility0Skill1ButtonUp()
        {
            if (isControlPrevented)
                return;

            CallAbilitySkill(0, 1, false);
        }
        void InputHandler.IInputReceiver.OnAbility0Skill0ButtonDown()
        {
            if (isControlPrevented)
                return;

            CallAbilitySkill(0, 0, true);
        }
        void InputHandler.IInputReceiver.OnAbility0Skill0ButtonUp()
        {
            if (isControlPrevented)
                return;

            CallAbilitySkill(0, 0, false);
        }

        IHitReactor.HitResult IHitReactor.Hit(IHitReactor.HitInfo hitInfo)
        {
            if (remainHitRecoverTime != 0)
                return new IHitReactor.HitResult(0, false);

            IHitReactor.HitReaction hitReaction = CollectAbilitiesHitReaction(hitInfo);
            IHitReactor.HitInfo reactedHitInfo = MakeReactedHit(hitInfo, hitReaction);

            foreach (var ability in abilities)
                ability.OnHit(reactedHitInfo);

            PlayHitAudio(reactedHitInfo);

            float acceptedDamage = 0;
            if (reactedHitInfo.damage != 0)
            {
                acceptedDamage = HandleDamage(reactedHitInfo.damage);
            }
                

            if(! IsDead)
            {
                if (reactedHitInfo.knockBackVelocity.sqrMagnitude > 0)
                    movement.SetOverridingVelocity(new Vector2(reactedHitInfo.knockBackVelocity.x, reactedHitInfo.knockBackVelocity.y));

                if (reactedHitInfo.stiffenTime > 0)
                {
                    remainStiffenTime = reactedHitInfo.stiffenTime;
                    modelAnimator.SetBool(id_IsStiffen, true);
                }

                if (acceptedDamage != 0 && hitRecoverTime > 0)
                    remainHitRecoverTime += hitRecoverTime;
            }

            return new IHitReactor.HitResult(acceptedDamage, acceptedDamage != 0 && isDead);
        }

        private IHitReactor.HitInfo MakeReactedHit(IHitReactor.HitInfo hitInfo, IHitReactor.HitReaction hitReaction)
        {
            IHitReactor.HitInfo reactedHitInfo = hitInfo;
            if (hitReaction.isBlocked)
            {
                float damage = 0;
                float stiffenTime = 0f;
                reactedHitInfo = new IHitReactor.HitInfo(hitInfo.type, damage, hitInfo.direction, hitInfo.isPenetration, hitInfo.knockBackVelocity, stiffenTime);
            }
            else
            {
                reactedHitInfo = new IHitReactor.HitInfo(hitInfo.type, hitInfo.damage, hitInfo.direction, hitInfo.isPenetration, hitInfo.knockBackVelocity, hitInfo.stiffenTime);
            }

            return reactedHitInfo;
        }

        private void PlayHitAudio(IHitReactor.HitInfo reactedHitInfo)
        {
            AudioSource hitAudio = reactedHitInfo.type switch
            {
                IHitReactor.HitType.Bullet => bulletHitAudio,
                IHitReactor.HitType.Trap => bulletHitAudio,
                _ => bulletHitAudio
            };
            hitAudio.Play();
        }

        private IHitReactor.HitReaction CollectAbilitiesHitReaction(IHitReactor.HitInfo hitInfo)
        {
            IHitReactor.HitReaction hitReaction = new IHitReactor.HitReaction(false);
            foreach (var ability in abilities)
            {
                IHitReactor.HitReaction currentHitReaction = ability.ReactBeforeHit(hitInfo);
                if (currentHitReaction.isBlocked)
                    hitReaction = new IHitReactor.HitReaction(true);
            }

            return hitReaction;
        }

        void IAbilityHolder.NotifyKillEnemy(GameObject enemy)
        {
            foreach (IAbility ability in abilities)
                ability.OnKillEnemy(enemy.transform);
        }

        void IAbilityHolder.SetVelocity(Vector2 velocity, float recoverTime)
        {
            movement.SetOverridingVelocity(velocity, recoverTime);
        }

        void IAbilityHolder.AddVelocity(Vector2 velocity, float recoverTime)
        {
            movement.AddAddtionalVelocity(velocity, recoverTime);
        }

        Vector2 IAbilityHolder.GetVelocityToStopOverGroundVelocity()
        {
            Vector2 velocityOverGroundVelocity = rigidBody.velocity - groundChecker.GetGroundVelocity();
            return velocityOverGroundVelocity * -1;
        }

        void IAbilityHolder.OccupyMovement(bool occupy)
        {
            movement.IsMovementOccupied = occupy;
        }

        public interface ISubscriber
        {
            void OnDeath(MyUnit myUnit);
            void OnHealthPointChange(MyUnit myUnit);
            void OnDamage(MyUnit myUnit, float damage);
            void OnHeal(MyUnit myUnit, float heal);
        }
    }

}
