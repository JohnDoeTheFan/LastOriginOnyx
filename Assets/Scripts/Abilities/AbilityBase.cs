using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Onyx.Ability
{
    public abstract class AbilityBase : TangibleComponent, IAbility, AbilitySkill.IAbilityInformation
    {
        [SerializeField]
        protected string abilityName;
        [SerializeField]
        protected Sprite abilityImage;

        protected int level;
        protected RectTransform abilitySpecificGuiArea;
        protected List<AbilitySkill> skills = new List<AbilitySkill>();
        protected IAbilityHolder abilityHolder;

        List<AbilitySkill> IAbility.Skills => skills;
        string IAbility.AbilityName => abilityName;
        Sprite IAbility.AbilityImage => abilityImage;

        public int Level => level;

        public IAbilityHolder AbilityHolder => abilityHolder;

        protected virtual void Start()
        {
            // Do nothing.
        }

        public void SetLevel(IAbility.LevelDescription level)
        {
            this.level = level.abilityLevel;
            for(int i = 0; i < Mathf.Min(skills.Count, level.skillLevels.Count); i++)
                skills[i].SetLevel(level.skillLevels[i]);
        }

        public void SetAbilityHolder(IAbilityHolder abilityHolder)
        {
            this.abilityHolder = abilityHolder;
        }

        public virtual void OnChangeDirection(Quaternion rotation) { }
        public virtual void OnKillEnemy(Transform enemyTransform) { }

        public abstract void InstantiateAbilitySpecificGui(RectTransform abilitySpecificGuiArea);

    }

    public interface IAbility
    {
        string AbilityName { get; }
        Sprite AbilityImage { get; }
        int Level { get; }
        List<AbilitySkill> Skills { get; }
        IAbilityHolder AbilityHolder { get; }

        void SetLevel(LevelDescription level);
        void SetAbilityHolder(IAbilityHolder abilityHolder);
        void OnChangeDirection(Quaternion rotation);
        void OnKillEnemy(Transform enemyTransform);
        void InstantiateAbilitySpecificGui(RectTransform abilitySpecificGuiArea);

        public struct LevelDescription
        {
            public int abilityLevel;
            public List<int> skillLevels;

            public LevelDescription(int abilityLevel, List<int> skillLevels)
            {
                this.abilityLevel = abilityLevel;
                this.skillLevels = skillLevels;
            }
        }
    }

    public interface IAbilityHolder
    {
        int Level { get; }
        int LevelOfDifficulty { get; }
        bool ShouldUseLevelOfDifficulty { get; }
        bool isFacingLeft { get; }
        bool isMovementOccupied { get; }
        bool IsGrounded { get; }

        void AddVelocity(Vector2 velocity, float recoverTime = 0f);
        void NotifyKillEnemy(GameObject enemy);
        void OccupyMovement(bool occupy);
    }

    public abstract class AbilitySkill
    {
        readonly private string skillName;
        readonly private Sprite skillImage;
        readonly private bool isPassive;
        readonly private bool isSingleLevel;
        protected int level;
        readonly private List<float> coolTimePerLevel;
        private float remainCoolTime;
        readonly private SkillDescription skillDescription;
        readonly private Func<IEnumerator, Coroutine> startCoroutine;
        protected IAbilityInformation abilityInformation;

        public SubscribeManagerTemplate<ISubscriber> SubscribeManager { get; } = new SubscribeManagerTemplate<ISubscriber>();

        // SingleLevel
        protected AbilitySkill(string skillName, Sprite skillImage, bool isPassive, float coolTime, SkillDescription skillDescription, Func<IEnumerator, Coroutine> startCoroutine)
        {
            this.skillName = skillName;
            this.skillImage = skillImage;
            this.isPassive = isPassive;
            isSingleLevel = true;
            level = 0;
            coolTimePerLevel = new List<float>() { coolTime };
            this.skillDescription = skillDescription;
            this.startCoroutine = startCoroutine;

            startCoroutine(ReduceCoolTime());
            startCoroutine(CheckAvailable());
        }

        // MultiLevel
        protected AbilitySkill(string skillName, Sprite skillImage, bool isPassive, int level, List<float> coolTimePerLevel, SkillDescription skillDescription, Func<IEnumerator, Coroutine> startCoroutine)
        {
            this.skillName = skillName;
            this.skillImage = skillImage;
            this.isPassive = isPassive;
            isSingleLevel = false;
            this.level = level;
            if (coolTimePerLevel != null && coolTimePerLevel.Count != 0)
                this.coolTimePerLevel = new List<float>(coolTimePerLevel);
            else
                this.coolTimePerLevel = new List<float>() { 0 };
            this.skillDescription = skillDescription;
            this.startCoroutine = startCoroutine;
        }

        public string SkillName => skillName;
        public Sprite SkillImage => skillImage;
        public bool IsPassive => isPassive;
        public bool IsSingleLevel => isSingleLevel;
        public int Level => level;
        public SkillDescription GetDescription => skillDescription;

        protected Func<IEnumerator, Coroutine> StartCoroutine => startCoroutine;

        public float RemainCoolTime => remainCoolTime;
        public float CoolTime => coolTimePerLevel[Mathf.Clamp(level, 0, coolTimePerLevel.Count - 1)];

        public virtual bool IsAvailable { get { return true; } }
        public virtual void OnSkillTouchDown() { }
        public virtual void OnSkillTouchHolding(float deltaTime) { }
        public virtual void OnSkillTouchUp() { }
        public virtual void OnSkillStartDrag(Camera targetCamera, Canvas targetCanvas, Vector2 position) { }
        public virtual void OnSkillDragging(Camera targetCamera, Canvas targetCanvas, Vector2 position) { }
        public virtual void OnSkillDrop(Camera targetCamera, Canvas targetCanvas, Vector2 position) { }

        protected void SetCoolTime()
        {
            remainCoolTime = CoolTime;
        }

        IEnumerator ReduceCoolTime()
        {
            float coolTimeReduceInterval = 0.1f;

            while (true)
            {
                remainCoolTime = Mathf.Max(0, remainCoolTime - coolTimeReduceInterval);
                yield return new WaitForSeconds(coolTimeReduceInterval);

                SubscribeManager.ForEach(item => item.OnRemainCoolTimeChanged(this));
            }
        }

        IEnumerator CheckAvailable()
        {
            float interval = 0.1f;
            bool lastAvailable = true;

            while (true)
            {
                yield return new WaitForSeconds(interval);
                if(lastAvailable != IsAvailable)
                {
                    lastAvailable = IsAvailable;
                    SubscribeManager.ForEach(item => item.OnAvailableChanged(this));
                }

            }
        }

        public void SetLevel(int level)
        {
            this.level = level;
        }

        public interface ISubscriber
        {
            void OnRemainCoolTimeChanged(AbilitySkill abilitySkill);
            void OnAvailableChanged(AbilitySkill abilitySkill);
        }

        public interface IAbilityInformation
        {
            int Level { get; }
        }
    }

    public abstract class PassiveAbilitySkill : AbilitySkill
    {
        protected PassiveAbilitySkill(string skillName, Sprite skillImage, float coolTime, SkillDescription skillDescription, Func<IEnumerator, Coroutine> startCoroutine)
            : base(
                skillName: skillName,
                skillImage: skillImage,
                isPassive: true,
                coolTime: coolTime,
                skillDescription: skillDescription,
                startCoroutine: startCoroutine
                )
        {

        }

        protected PassiveAbilitySkill(string skillName, Sprite skillImage, int level, List<float> coolTimePerLevel, SkillDescription skillDescription, Func<IEnumerator, Coroutine> startCoroutine)
            : base(
                skillName: skillName,
                skillImage: skillImage,
                isPassive: true,
                level: level,
                coolTimePerLevel: coolTimePerLevel,
                skillDescription: skillDescription,
                startCoroutine: startCoroutine
                )
        {

        }
    }

    public abstract class ActiveAbilitySKill : AbilitySkill
    {
        protected ActiveAbilitySKill(string skillName, Sprite skillImage, float coolTime, SkillDescription skillDescription, Func<IEnumerator, Coroutine> startCoroutine)
            : base(
                skillName: skillName,
                skillImage: skillImage,
                isPassive: false,
                coolTime: coolTime,
                skillDescription: skillDescription,
                startCoroutine: startCoroutine
                )

        {
        }

        protected ActiveAbilitySKill(string skillName, Sprite skillImage, int level, List<float> coolTimePerLevel, SkillDescription skillDescription, Func<IEnumerator, Coroutine> startCoroutine)
              : base(
                skillName: skillName,
                skillImage: skillImage,
                isPassive: false,
                level: level,
                coolTimePerLevel: coolTimePerLevel,
                skillDescription: skillDescription,
                startCoroutine: startCoroutine
                )
        {
        }

    }
    public class ButtonActiveAbilitySKill : ActiveAbilitySKill
    {
        readonly private Action onSkillTouchDown;
        readonly private Func<bool> isAvailable;

        public ButtonActiveAbilitySKill(string skillName, Sprite skillImage, float coolTime, SkillDescription skillDescription, Func<IEnumerator, Coroutine> startCoroutine, Action onSkillTouchDown, Func<bool> isAvailable)
            : base(
                skillName: skillName,
                skillImage: skillImage,
                coolTime: coolTime,
                skillDescription: skillDescription,
                startCoroutine: startCoroutine
                )
        {
            this.onSkillTouchDown = onSkillTouchDown;
            this.isAvailable = isAvailable;
        }

        public ButtonActiveAbilitySKill(string skillName, Sprite skillImage, int level, List<float> coolTimePerLevel, SkillDescription skillDescription, Func<IEnumerator, Coroutine> startCoroutine, Action onSkillTouchDown, Func<bool> isAvailable)
              : base(
                skillName: skillName,
                skillImage: skillImage,
                level: level,
                coolTimePerLevel: coolTimePerLevel,
                skillDescription: skillDescription,
                startCoroutine: startCoroutine
                )
        {
            this.onSkillTouchDown = onSkillTouchDown;
            this.isAvailable = isAvailable;
        }

        public override bool IsAvailable => isAvailable();

        public override void OnSkillTouchDown()
        {
            if (IsAvailable && RemainCoolTime == 0)
            {
                onSkillTouchDown?.Invoke();
                SetCoolTime();
            }
        }
    }

    public class ToggleActiveAbilitySKill : ActiveAbilitySKill
    {
        bool isDowned = false;
        readonly private Action onSkillTouchDown;
        readonly private Action onSkillTouchUp;

        public ToggleActiveAbilitySKill(string skillName, Sprite skillImage, float coolTime, SkillDescription skillDescription, Func<IEnumerator, Coroutine> startCoroutine, Action onSkillTouchDown, Action onSkillTouchUp)
            : base(
                skillName: skillName,
                skillImage: skillImage,
                coolTime: coolTime,
                skillDescription: skillDescription,
                startCoroutine: startCoroutine
                )
        {
            this.onSkillTouchDown = onSkillTouchDown;
            this.onSkillTouchUp = onSkillTouchUp;
        }

        public ToggleActiveAbilitySKill(string skillName, Sprite skillImage, int level, List<float> coolTimePerLevel, SkillDescription skillDescription, Func<IEnumerator, Coroutine> startCoroutine, Action onSkillTouchDown, Action onSkillTouchUp)
              : base(
                skillName: skillName,
                skillImage: skillImage,
                level: level,
                coolTimePerLevel: coolTimePerLevel,
                skillDescription: skillDescription,
                startCoroutine: startCoroutine
                )
        {
            this.onSkillTouchDown = onSkillTouchDown;
            this.onSkillTouchUp = onSkillTouchUp;
        }

        public override void OnSkillTouchDown()
        {
            if (RemainCoolTime == 0)
            {
                onSkillTouchDown?.Invoke();
                isDowned = true;
            }
        }
        public override void OnSkillTouchUp()
        {
            if (isDowned)
            {
                onSkillTouchUp?.Invoke();
                SetCoolTime();
            }
        }
    }

    public struct SkillDescription
    {
        readonly string mainDescription;
    }

}
