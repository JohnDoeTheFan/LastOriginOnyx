using Onyx.Core;
using Onyx.Ability;
using Onyx.GameElement;
using Onyx.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace Onyx
{
    public class MyUnit : MonoBehaviourBase, InputHandler.IInputReceiver, IHitReactor, IAbilityHolder, HealingItem.IHealingItemReactor
    {
        public static Action<MyUnit> OnEndOfStart;

        [SerializeField, Range(1, 10)]
        private float maxVelocity = 3f;
        [SerializeField, Range(0.001f, 5)]
        private float velocityReachTime = 0.1f;
        [SerializeField, Range(0.001f, 1f)]
        private float velocityInAirRate = 0.2f;
        [SerializeField, Range(1, 10)]
        private float overSpeedRecoverVelocity = 5f;
        [SerializeField, Range(0.001f, 5)]
        private float overSpeedRecoverVelocityReachTime = 0.1f;
        [SerializeField]
        private float dustMakingVelocity = 3f;
        [SerializeField]
        private float healthPoint = 1;
        [SerializeField]
        private float maxHealth = 1;
        [SerializeField]
        private GroundChecker groundChecker;
        [SerializeField]
        private Sight sight;
        [SerializeField]
        private ParticleSystem dustParticleSystem;
        [Header("Audios")]
        [SerializeField]
        private AudioSource runAudioSource;
        [SerializeField]
        private AudioSource bulletHitAudio;
        [Header("Voices")]
        [SerializeField]
        private AudioClip stageStartVoice;
        [SerializeField]
        private AudioClip retireVoice;
        [SerializeField]
        private AudioClip retreatVoice;
        [SerializeField]
        private AudioClip stageClearVoice;
        [Header("Levels")]
        [SerializeField]
        private int level;
        [SerializeField]
        private int levelOfDifficulty;
        [SerializeField]
        private MultiplierPerLevel healthMultipliers;
        [SerializeField]
        private int scoreMultiplier;

        private Rigidbody2D rigidBody;
        private InputHandler inputHandler;
        private Animator animator;
        readonly private List<IAbility> abilities = new List<IAbility>();
        private ClosestObjectInSightManager<InteractableComponent> closestInteractable;

        private bool isDead = false;
        private bool preventControl = false;
        private Vector2 leftStick = new Vector2(0, 0);

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

        // Start is called before the first frame update
        void Start()
        {
            animator = GetComponent<Animator>();
            rigidBody = GetComponent<Rigidbody2D>();
            inputHandler = GetComponent<InputHandler>();
            inputHandler.AddInputReceiverRegisterAwaiter(this);
            abilities.AddRange(GetComponents<IAbility>());
            foreach (IAbility ability in abilities)
            {
                ability.SetAbilityHolder(this);
            }

            if (CompareTag("Player"))
            {
                closestInteractable = new ClosestObjectInSightManager<InteractableComponent>(sight, transform, OnChangeInteractable);
                StartCoroutine(closestInteractable.UpdateCoroutine(0.1f));
            }

            OnEndOfStart(this);
        }

        // Update is called once per frame
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

        void UpdateMovement()
        {
            Vector2 inputDirection = Vector2.zero;
            if (leftStick.x > 0)
            {
                inputDirection = Vector2.right;
                transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
            }
            else if (leftStick.x < 0)
            {
                inputDirection = Vector2.left;
                transform.rotation = Quaternion.Euler(new Vector3(0, 180, 0));
            }

            float currentVelocityX = Mathf.Abs(rigidBody.velocity.x);
            bool isStopped = rigidBody.velocity.x == 0;
            bool isOverSpeed = !Mathf.Approximately(currentVelocityX, maxVelocity) && currentVelocityX > maxVelocity + 1;
            bool isInput = inputDirection != Vector2.zero;
            bool isDecelerating = rigidBody.velocity.x * inputDirection.x < 0;

            AddControlImpulse(inputDirection, isStopped, isInput, isOverSpeed, isDecelerating);

            if (isOverSpeed)
                AddOverSpeedRecoverImpulse(isOverSpeed);
            else
                LimitNewControlVelocity(isOverSpeed);
        }

        public void AddControlImpulse(Vector2 inputDirection, bool isStopped, bool isInput, bool isOverSpeed, bool isDecelerating)
        {
            float velocityToAdd = maxVelocity * Time.deltaTime / velocityReachTime;
            float impulse = CalcImpulseForVelocity(velocityToAdd);
            float airImpulse = impulse * velocityInAirRate;

            if (isInput)
            {
                bool isOverspeedRegisting = isOverSpeed && isDecelerating;
                bool shouldAddImpulse = !isOverSpeed || isOverspeedRegisting;

                if (shouldAddImpulse)
                {
                    if (groundChecker.IsGrounded)
                        rigidBody.AddForce(inputDirection * impulse, ForceMode2D.Impulse);
                    else
                        rigidBody.AddForce(inputDirection * airImpulse, ForceMode2D.Impulse);
                }
            }
            else if (!isStopped)
            {
                Vector2 stoppingImpulseDirection = Vector2.zero;
                if (rigidBody.velocity.x > 0)
                    stoppingImpulseDirection = Vector2.left;
                else if (rigidBody.velocity.x < 0)
                    stoppingImpulseDirection = Vector2.right;

                float maximumStoppingImpulse = impulse;
                if (!groundChecker.IsGrounded)
                    maximumStoppingImpulse = airImpulse;

                float impluseToStop = CalcImpulseForVelocity(Mathf.Abs(rigidBody.velocity.x));

                float stoppingImpulse = Mathf.Min(impluseToStop, maximumStoppingImpulse);

                rigidBody.AddForce(stoppingImpulseDirection * stoppingImpulse, ForceMode2D.Impulse);
            }
        }

        private void AddOverSpeedRecoverImpulse(bool isOverSpeed)
        {
            float velocityToAdd = overSpeedRecoverVelocity * Time.deltaTime / overSpeedRecoverVelocityReachTime;
            float impulse = CalcImpulseForVelocity(velocityToAdd);

            Vector2 naturalImpulseDirection = Vector2.zero;
            if (rigidBody.velocity.x > 0)
                naturalImpulseDirection = Vector2.left;
            else if (rigidBody.velocity.x < 0)
                naturalImpulseDirection = Vector2.right;

            if (groundChecker.IsGrounded)
                rigidBody.AddForce(naturalImpulseDirection * impulse, ForceMode2D.Impulse);
            else
                rigidBody.AddForce(naturalImpulseDirection * impulse, ForceMode2D.Impulse);
        }

        private void LimitNewControlVelocity(bool isOverSpeed)
        {
            float newVelocity = Mathf.Abs(rigidBody.velocity.x);
            if (newVelocity > maxVelocity)
            {
                float limitingImpulse = CalcImpulseForVelocity(newVelocity - maxVelocity);

                Vector2 limitDirection = Vector2.zero;
                if (rigidBody.velocity.x > 0)
                    limitDirection = Vector2.left;
                else if (rigidBody.velocity.x < 0)
                    limitDirection = Vector2.right;

                rigidBody.AddForce(limitDirection * limitingImpulse, ForceMode2D.Impulse);
            }
        }

        float CalcImpulseForVelocity(float velocity)
        {
            return rigidBody.mass * velocity;
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

        private void CallAbilitySkill(int abilityIndex, int skillIndex, bool isDown)
        {
            if (abilities.Count > abilityIndex && abilities[abilityIndex] != null && skillIndex < abilities[abilityIndex].Skills.Count)
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
                _ => bulletHitAudio
            };
            hitAudio.Play();

            float acceptedDamage = TakeDamage(damage);
            rigidBody.AddForce(force);

            return new IHitReactor.HitResult(damage, acceptedDamage != 0 && isDead);
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
