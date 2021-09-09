using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Onyx.Ability;
using System;

public class MeleeAttacker : AbilityBase, MeleeAttack.ISubscriber
{
    [SerializeField] private string skill1Name;
    [SerializeField] private Sprite skill1Sprite;
    [SerializeField] List<ComboInformation> combos;
    [Header("Rewards")]
    [SerializeField] GameObject reward;
    [SerializeField] float rewardProbability;
    [SerializeField] private MultiplierPerLevel damageMultiplierPerLevel;

    private int comboCountStringToHash = Animator.StringToHash("ComboCount");
    private int comboCount = -1;

    private float currentComboStartTime;
    private Coroutine comboResetTimer;

    private bool isOccupiedByThis;
    private bool isInitializing;

    protected override void Start()
    {
        skills.Add(new ButtonActiveAbilitySKill(skill1Name, skill1Sprite, 0f, new SkillDescription(), StartCoroutine, OnSkill1, IsAvailable));
        HashSet<MeleeAttack> processedSet = new HashSet<MeleeAttack>();

        foreach (ComboInformation combo in combos)
        {
            foreach (DataOnTime<MeleeAttack> attackOnTime in combo.attackOnTime)
            {
                if (!processedSet.Contains(attackOnTime.data))
                {
                    attackOnTime.data.subscriberManager.Subscribe(this);
                    float damage = 0.1f * combo.damageMultiplier * damageMultiplierPerLevel.GetMultiplier(abilityHolder.Level);
                    attackOnTime.data.SetDamage(damage);

                    processedSet.Add(attackOnTime.data);

                }
            }
        }
    }

    public bool IsAvailable()
    {
        if (isInitializing)
            return false;
        if (abilityHolder.isMovementOccupied && !isOccupiedByThis)
            return false;

        if (comboCount + 1 < combos.Count)
        {
            if (!combos[comboCount + 1].canUseInAir && !abilityHolder.IsGrounded)
                return false;
        }

        return true;
    }

    public override void InstantiateAbilitySpecificGui(RectTransform abilitySpecificGuiArea)
    {
    }

    private void OnSkill1()
    {
        bool isFirstCombo = comboCount == -1;
        if (isFirstCombo)
        {
            StartNextCombo();
        }
        else
        {
            ComboInformation currentCombo = combos[comboCount];
            float nextComboInputStartTime = currentComboStartTime + currentCombo.nextComboInputStart;
            float nextComboInputEndTime = nextComboInputStartTime + currentCombo.nextComboInputDuration;
            bool canStartNextCombo = IsInTimeRange(Time.time, nextComboInputStartTime, nextComboInputEndTime);
            if (canStartNextCombo)
            {
                StartNextCombo();
            }
        }

    }

    private bool IsInTimeRange(float time, float startTime, float endTime)
    {
        return startTime <= time && time < endTime;
    }

    private void StartNextCombo()
    {
        if (comboCount + 1 < combos.Count)
        {
            if (comboResetTimer != null)
                StopCoroutine(comboResetTimer);

            comboCount++;
            ComboInformation currentCombo = combos[comboCount];

            abilityHolder.OccupyMovement(true);
            isOccupiedByThis = true;

            currentComboStartTime = Time.time;
            comboResetTimer = StartCoroutine(ResetComboCount(currentCombo.duration));

            foreach (DataOnTime<Vector2> velocityOnTime in currentCombo.velocityOnTimes)
                StartCoroutine(Job(() => WaitForSecondsRoutine(velocityOnTime.time), () => AddVelocity(velocityOnTime.data)));

            foreach (DataOnTime<MeleeAttack> attackOnTime in currentCombo.attackOnTime)
                StartCoroutine(Job(() => WaitForSecondsRoutine(attackOnTime.time), () =>
                {
                    attackOnTime.data.Activate();
                }));

            abilityHolder.ModelAnimator.SetInteger(comboCountStringToHash, comboCount);
        }
    }

    private IEnumerator ResetComboCount(float time)
    {
        yield return new WaitForSeconds(time);
        isInitializing = true;
        StartCoroutine(Job(() => WaitForSecondsRoutine(0.25f), () => isInitializing = false));
        comboCount = -1;
        abilityHolder.ModelAnimator.SetInteger(comboCountStringToHash, comboCount);
        abilityHolder.OccupyMovement(false);
        isOccupiedByThis = false;
        comboResetTimer = null;
    }

    private void AddVelocity(Vector2 velocity)
    {
        Vector2 velocityWithFacingDirection = velocity;
        if (abilityHolder.isFacingLeft)
            velocityWithFacingDirection.x *= -1;

        abilityHolder.AddVelocity(velocityWithFacingDirection, 0.2f);
    }

    void MeleeAttack.ISubscriber.OnHit(MeleeAttack attack, IHitReactor hitReactor, IHitReactor.HitResult hitResult)
    {
        if(hitResult.isKilledByHit && reward != null)
        {
            if(UnityEngine.Random.value < rewardProbability)
            {
                GameObject newReward = Instantiate(reward, hitReactor.GetWorldPosition, Quaternion.identity);
                Rigidbody2D rigidBody = newReward.GetComponent<Rigidbody2D>();
                if (rigidBody != null)
                {
                    float velocityY = 3f;
                    float velocityX = UnityEngine.Random.Range(-velocityY / 2, velocityY / 2);
                    Vector2 impulse = new Vector2(velocityX, rigidBody.mass * velocityY);
                    rigidBody.AddForce(impulse, ForceMode2D.Impulse);
                }
            }
        }
    }

    [System.Serializable]
    private struct DataOnTime<T>
    {
        public float time;
        public T data;
    }

    [System.Serializable]
    private struct ComboInformation
    {
        public float duration;
        public float damageMultiplier;
        public float nextComboInputStart;
        public float nextComboInputDuration;
        public bool canUseInAir;
        public List<DataOnTime<Vector2>> velocityOnTimes;
        public List<DataOnTime<MeleeAttack>> attackOnTime;
    }
}

