using Onyx.Core;
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
        public static event OnEndOfStartHandler OnEndOfStartEvent;

        [Header("Physics")]
        [SerializeField, Range(1, 10)]      private float maxVelocity = 3f;
        [SerializeField, Range(0.001f, 5)]  private float velocityReachTime = 0.1f;
        [SerializeField, Range(0.001f, 1f)] private float velocityInAirRate = 0.2f;
        [SerializeField, Range(1, 10)]      private float overSpeedRecoverVelocity = 5f;
        [SerializeField, Range(0.001f, 5)]  private float overSpeedRecoverVelocityReachTime = 0.1f;
        [SerializeField, Range(0, 5f)]      private float hitRecoverTime = 0.2f;

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
        [SerializeField] private AudioSource runAudioSource;
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

        [Header("Gui")]
        [SerializeField] Canvas unitGuiCanvas;

        private Rigidbody2D rigidBody;
        private InputHandler inputHandler;
        private Animator animator;
        readonly private List<IAbility> abilities = new List<IAbility>();
        private ClosestObjectInSightManager<InteractableComponent> closestInteractable;

        private bool isDead = false;
        private bool preventControl = false;
        private Vector2 leftStick = new Vector2(0, 0);
        private float remainHitRecoverTime = 0;
        private float remainSkillMomentumRecoverTime = 0;
        private Vector2 velocityChangeByDamage;
        private Vector2 velocityChangeBySkill;
        private bool isMovementOccupied;

        public float HealthPoint => healthPoint;
        public bool IsDead => isDead;
        public AudioClip StageStartVoice => stageStartVoice;
        public AudioClip RetireVoice => retireVoice;
        public AudioClip RetreatVoice => retreatVoice;
        public AudioClip StageClearVoice => stageClearVoice;
        public ReadOnlyCollection<IAbility> Abilities => abilities.AsReadOnly();
        public SubscribeManagerTemplate<ISubscriber> SubscribeManager { private set; get; } = new SubscribeManagerTemplate<ISubscriber>();
        public int ScoreMultiplier => scoreMultiplier;

        Vector3 IHitReactor.GetWorldPosition => transform.position;
        int IAbilityHolder.Level => level;
        int IAbilityHolder.LevelOfDifficulty => levelOfDifficulty;
        bool IAbilityHolder.ShouldUseLevelOfDifficulty => !CompareTag("Player");
        bool IAbilityHolder.isFacingLeft => rigidBody.transform.rotation.y != 0;
        bool IAbilityHolder.isMovementOccupied => isMovementOccupied;
        GameObject IHitReactor.GameObject => gameObject;

        bool IAbilityHolder.IsGrounded => groundChecker.IsGrounded;

        void Start()
        {
            animator = GetComponent<Animator>();
            rigidBody = GetComponent<Rigidbody2D>();
            inputHandler = GetComponent<InputHandler>();
            inputHandler.AddInputReceiverRegisterAwaiter(this);
            abilities.AddRange(GetComponents<IAbility>());
            foreach (IAbility ability in abilities)
                ability.SetAbilityHolder(this);

            if (CompareTag("Player"))
            {
                // 가장 가까운 상호작용 가능한 물체에 대한 탐색 코루틴을 실행
                closestInteractable = new ClosestObjectInSightManager<InteractableComponent>(sight, transform, OnChangeInteractable);
                StartCoroutine(closestInteractable.UpdateCoroutine(0.1f));
            }

            // 시작 단계 초기화를 끝낸 후, 이를 알림.
            OnEndOfStartEvent?.Invoke(this);
        }

        void Update()
        {
            UpdateMovement();

            animator.SetBool("Grounded", groundChecker.IsGrounded);
            animator.SetFloat("Speed", Mathf.Abs(rigidBody.velocity.x));

            bool shouldMakeDust = groundChecker.IsGrounded && Mathf.Abs(rigidBody.velocity.x) > dustMakingVelocity;
            if (shouldMakeDust && !dustParticleSystem.isEmitting)
                dustParticleSystem.Play();
            else if (!shouldMakeDust && dustParticleSystem.isEmitting)
                dustParticleSystem.Stop();
        }

        /// <summary>
        /// 캐릭터의 운동을 수행한다. 매 업데이트 마다 호출.
        /// </summary>
        void UpdateMovement()
        {
            Vector2 inputDirection = Vector2.zero;
            if (leftStick.x > 0)
                inputDirection = Vector2.right;
            else if (leftStick.x < 0)
                inputDirection = Vector2.left;

            if (isMovementOccupied)
                inputDirection = Vector2.zero;

            RotateUnit(inputDirection);

            float currentAbsVelocityX = Mathf.Abs(rigidBody.velocity.x);
            bool isStopped = currentAbsVelocityX == 0;
            bool wasOverSpeed = !Mathf.Approximately(currentAbsVelocityX, maxVelocity) && currentAbsVelocityX > maxVelocity;

            AddDamageMomentum();
            if (remainHitRecoverTime == 0 && remainSkillMomentumRecoverTime == 0)
                AddControlMomentum(inputDirection, isStopped, wasOverSpeed);
            AddSkillMomentum();

            remainHitRecoverTime = Mathf.Max(0, remainHitRecoverTime - Time.deltaTime);
            remainSkillMomentumRecoverTime = Mathf.Max(0, remainSkillMomentumRecoverTime - Time.deltaTime);

            if (wasOverSpeed)
                AddOverSpeedRecoverMomentum();
            else
                AddMomentumToLimitVelocity();
        }

        /// <summary>
        /// Unit 을 회전한다.
        /// </summary>
        /// <param name="inputDirection">입력 방향</param>
        private void RotateUnit(Vector2 inputDirection)
        {
            Quaternion rotation = transform.rotation;
            if (inputDirection == Vector2.right)
                rotation = Quaternion.identity;
            else if (inputDirection == Vector2.left)
                rotation = Quaternion.Euler(new Vector3(0, 180, 0));

            transform.rotation = rotation;
            if (unitGuiCanvas != null)
                unitGuiCanvas.transform.localRotation = rotation;
        }

        /// <summary>
        /// 입력에 따른 운동량을 가한다.
        /// </summary>
        /// <param name="inputDirection">현재 입력</param>
        /// <param name="isStopped">현재 정지 여부</param>
        /// <param name="isOverSpeed">현재 과속 여부</param>
        public void AddControlMomentum(Vector2 inputDirection, bool isStopped, bool isOverSpeed)
        {
            float velocityToAdd = maxVelocity;
            if (velocityReachTime > Time.deltaTime)
                velocityToAdd *= Time.deltaTime / velocityReachTime;

            float impulse = CalcMomentumToChangeVelocity(velocityToAdd);
            if (!groundChecker.IsGrounded)
                impulse *= velocityInAirRate;

            bool isInput = inputDirection != Vector2.zero;
            if (isInput)
            {
                bool isDecelerating = rigidBody.velocity.x * inputDirection.x < 0;
                bool isOverspeedRegisting = isOverSpeed && isDecelerating;
                bool shouldAddImpulse = !isOverSpeed || isOverspeedRegisting;

                if (shouldAddImpulse)
                    rigidBody.AddForce(inputDirection * impulse, ForceMode2D.Impulse);
            }
            else if (!isStopped)
            {
                StopUnit(impulse);
            }
        }

        /// <summary>
        /// Unit 을 멈추기 위해 운동량을 가한다.
        /// </summary>
        /// <param name="maximumStoppingImpulse">멈출 때 사용될 수 있는 최대 힘</param>
        private void StopUnit(float maximumStoppingImpulse)
        {
            Vector2 stoppingImpulseDirection = Vector2.zero;
            if (rigidBody.velocity.x > 0)
                stoppingImpulseDirection = Vector2.left;
            else if (rigidBody.velocity.x < 0)
                stoppingImpulseDirection = Vector2.right;

            float impluseToStop = CalcMomentumToChangeVelocity(Mathf.Abs(rigidBody.velocity.x));

            float stoppingImpulse = Mathf.Min(impluseToStop, maximumStoppingImpulse);

            rigidBody.AddForce(stoppingImpulseDirection * stoppingImpulse, ForceMode2D.Impulse);
        }

        /// <summary>
        /// 과속 상태에서 벗어나기 위한 운동량을 가한다. 입력에 무관하게 가해진다.
        /// </summary>
        private void AddOverSpeedRecoverMomentum()
        {
            float velocityToAdd = overSpeedRecoverVelocity;
            if (overSpeedRecoverVelocityReachTime > Time.deltaTime)
                velocityToAdd *= Time.deltaTime / overSpeedRecoverVelocityReachTime;

            float impulse = CalcMomentumToChangeVelocity(velocityToAdd);

            Vector2 naturalImpulseDirection = Vector2.zero;
            if (rigidBody.velocity.x > 0)
                naturalImpulseDirection = Vector2.left;
            else if (rigidBody.velocity.x < 0)
                naturalImpulseDirection = Vector2.right;

            rigidBody.AddForce(naturalImpulseDirection * impulse, ForceMode2D.Impulse);
        }

        /// <summary>
        /// 입력으로 인한 운동량으로 인해 과속이 되지 않게 제한한다.
        /// </summary>
        private void AddMomentumToLimitVelocity()
        {
            float newVelocity = Mathf.Abs(rigidBody.velocity.x);
            if (newVelocity > maxVelocity)
            {
                float limitingImpulse = CalcMomentumToChangeVelocity(newVelocity - maxVelocity);

                Vector2 limitDirection = Vector2.zero;
                if (rigidBody.velocity.x > 0)
                    limitDirection = Vector2.left;
                else if (rigidBody.velocity.x < 0)
                    limitDirection = Vector2.right;

                rigidBody.AddForce(limitDirection * limitingImpulse, ForceMode2D.Impulse);
            }
        }

        /// <summary>
        /// 도달하고자 하는 속도로 가속하기 위한 운동량(Momentum)을 계산한다.
        /// </summary>
        /// <param name="targetVelocity">도달하고자 하는 속도</param>
        /// <returns>운동량</returns>
        float CalcMomentumToChangeVelocity(float targetVelocity)
        {
            return rigidBody.mass * targetVelocity;
        }

        /// <summary>
        /// 피해로 인한 운동량(넉백)을 가한다.
        /// </summary>
        private void AddDamageMomentum()
        {
            if(velocityChangeByDamage != Vector2.zero)
            {
                rigidBody.AddForce(velocityChangeByDamage * rigidBody.mass, ForceMode2D.Impulse);
                velocityChangeByDamage = Vector2.zero;
            }
        }

        private void AddSkillMomentum()
        {
            if(velocityChangeBySkill != Vector2.zero)
            {
                rigidBody.AddForce(velocityChangeBySkill * rigidBody.mass, ForceMode2D.Impulse);
                velocityChangeBySkill = Vector2.zero;
            }
        }

        public void Die()
        {
            isDead = true;
            inputHandler.AddInputReceiverUnregisterAwaiter(this);
            leftStick = Vector2.zero;
            animator.SetTrigger("Die");
            gameObject.layer = (int)LayerSetting.DeadBody;

            SubscribeManager.ForEach(item => item.OnDeath(this));
        }

        public void Revive()
        {
            isDead = false;
            inputHandler.AddInputReceiverRegisterAwaiter(this);
            TakeHeal(1);
            animator.SetTrigger("Revive");
        }

        public void Ceremony()
        {
            animator.SetTrigger("Ceremony");
        }

        public void TeleportAt(Vector3 position)
        {
            rigidBody.velocity = Vector2.zero;
            transform.position = position;
        }

        public float TakeDamage(float damage)
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

                animator.SetTrigger("Hit");

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

        public void PlayRunSound()
        {
            if (runAudioSource != null)
                runAudioSource.Play();
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
            if (preventControl)
            {
                this.leftStick = new Vector2(0, 0);
                return;
            }

            this.leftStick = leftStick;
        }


        void InputHandler.IInputReceiver.OnRightStick(Vector2 rightStick)
        {
            if (preventControl)
                return;

            if (abilities.Count > 0 && abilities[0] != null)
                abilities[0].OnChangeDirection(Quaternion.Euler(0, 0, Vector2.SignedAngle(transform.rotation * Vector2.right, rightStick)));
        }

        void InputHandler.IInputReceiver.OnInteractButtonDown()
        {
            if (preventControl)
                return;

            if (closestInteractable.Closest != null)
                closestInteractable.Closest.Interact(gameObject);
        }

        void InputHandler.IInputReceiver.OnPreservedButtonDown()
        {
        }


        void InputHandler.IInputReceiver.OnAbility2Skill1ButtonDown()
        {
            if (preventControl)
                return;

            CallAbilitySkill(2, 1, true);
        }
        void InputHandler.IInputReceiver.OnAbility2Skill0ButtonDown()
        {
            if (preventControl)
                return;

            CallAbilitySkill(2, 0, true);
        }


        void InputHandler.IInputReceiver.OnAbility1Skill1ButtonDown()
        {
            if (preventControl)
                return;

            CallAbilitySkill(1, 1, true);
        }
        void InputHandler.IInputReceiver.OnAbility1Skill1ButtonUp()
        {
            if (preventControl)
                return;

            CallAbilitySkill(1, 1, false);
        }
        void InputHandler.IInputReceiver.OnAbility1Skill0ButtonDown()
        {
            if (preventControl)
                return;

            CallAbilitySkill(1, 0, true);
        }
        void InputHandler.IInputReceiver.OnAbility1Skill0ButtonUp()
        {
            if (preventControl)
                return;

            CallAbilitySkill(1, 0, false);
        }

        void InputHandler.IInputReceiver.OnAbility0Skill1ButtonDown()
        {
            if (preventControl)
                return;

            CallAbilitySkill(0, 1, true);
        }
        void InputHandler.IInputReceiver.OnAbility0Skill1ButtonUp()
        {
            if (preventControl)
                return;

            CallAbilitySkill(0, 1, false);
        }
        void InputHandler.IInputReceiver.OnAbility0Skill0ButtonDown()
        {
            if (preventControl)
                return;

            CallAbilitySkill(0, 0, true);
        }
        void InputHandler.IInputReceiver.OnAbility0Skill0ButtonUp()
        {
            if (preventControl)
                return;

            CallAbilitySkill(0, 0, false);
        }


        IHitReactor.HitResult IHitReactor.Hit(IHitReactor.HitType type, float damage, Vector3 force)
        {
            AudioSource hitAudio = type switch
            {
                IHitReactor.HitType.Bullet => bulletHitAudio,
                IHitReactor.HitType.Trap => bulletHitAudio,
                _ => bulletHitAudio
            };
            hitAudio.Play();

            float acceptedDamage = TakeDamage(damage);

            if(force.sqrMagnitude > 0)
            {
                velocityChangeByDamage += new Vector2(force.x, force.y);
                remainHitRecoverTime = hitRecoverTime;
            }

            return new IHitReactor.HitResult(damage, acceptedDamage != 0 && isDead);
        }

        void IAbilityHolder.NotifyKillEnemy(GameObject enemy)
        {
            foreach (IAbility ability in abilities)
                ability.OnKillEnemy(enemy.transform);
        }

        void IAbilityHolder.AddVelocity(Vector2 velocity)
        {
            remainSkillMomentumRecoverTime = hitRecoverTime;
            velocityChangeBySkill = velocity;
        }

        void IAbilityHolder.OccupyMovement(bool occupy)
        {
            isMovementOccupied = occupy;
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
