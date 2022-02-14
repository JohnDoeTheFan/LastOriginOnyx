using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Onyx.Ability;
using System;

public class MeleeAttacker : AbilityBase, MeleeAttack.ISubscriber
{
    [SerializeField] private string skill1Name;
    [SerializeField] private Sprite skill1Sprite;
    [SerializeField] private List<ComboInformation> groundCombos;
    [SerializeField] private List<ComboInformation> airCombos;
    [SerializeField] private float comboCoolTime = 0.25f;

    [Header("Rewards")]
    [SerializeField] GameObject reward;
    [SerializeField] float rewardProbability;
    [SerializeField] private MultiplierPerLevel damageMultiplierPerLevel;

    private int comboCountStringToHash = Animator.StringToHash("ComboCount");
    private int comboCount = -1;

    private float currentComboStartTime;
    private Coroutine comboResetTimer;

    private bool isOccupiedByThis;
    private bool isResetting;

    private bool shouldStopComboByDamage;

    private List<ComboInformation> currentCombos;

    protected override void Start()
    {
        skills.Add(new ButtonActiveAbilitySKill(skill1Name, skill1Sprite, 0f, new SkillDescription(), StartCoroutine, OnSkill1, IsAvailable));
        HashSet<MeleeAttack> processedSet = new HashSet<MeleeAttack>();

        List<ComboInformation> allCombos = new List<ComboInformation>(groundCombos);
        allCombos.AddRange(airCombos);

        foreach (ComboInformation combo in allCombos)
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
        if (isResetting)
            return false;
        if (abilityHolder.isMovementOccupied && !isOccupiedByThis)
            return false;

        if(currentCombos != null)
        {
            if (comboCount + 1 < currentCombos.Count)
            {
                if (!currentCombos[comboCount + 1].canUseInAir && !abilityHolder.IsGrounded)
                    return false;
            }
        }
        else
        {
            if (abilityHolder.IsGrounded)
                return groundCombos.Count > 0 ? true : false;
            else
                return airCombos.Count > 0 ? true : false;
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
            if (abilityHolder.IsGrounded)
                currentCombos = groundCombos;
            else
                currentCombos = airCombos;

            shouldStopComboByDamage = false;

            StartNextCombo();
        }
        else
        {
            ComboInformation currentCombo = currentCombos[comboCount];
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
        if (comboCount + 1 < currentCombos.Count)
        {
            if (comboResetTimer != null)
                StopCoroutine(comboResetTimer);

            comboCount++;
            ComboInformation currentCombo = currentCombos[comboCount];

            abilityHolder.OccupyMovement(true);
            isOccupiedByThis = true;

            currentComboStartTime = Time.time;

            if(currentCombo.canUseInAir && currentCombo.keepUntilGrounded)
                comboResetTimer = StartCoroutine(ResetComboCount(currentCombo.duration, ()=> abilityHolder.IsGrounded));
            else
                comboResetTimer = StartCoroutine(ResetComboCount(currentCombo.duration));

            foreach (DataOnTime<Move> moveOnTime in currentCombo.moveOnTimes)
                StartCoroutine(Job(() => WaitUntilOrForSecondsRoutine(()=>shouldStopComboByDamage, moveOnTime.time), () => { if (!shouldStopComboByDamage) AddMove(moveOnTime.data); }));
                
            foreach (DataOnTime<MeleeAttack> attackOnTime in currentCombo.attackOnTime)
            {
                if (currentCombo.canUseInAir && currentCombo.keepUntilGrounded)
                    StartCoroutine(Job(() => WaitUntilOrForSecondsRoutine(() => shouldStopComboByDamage, attackOnTime.time), () => { if (!shouldStopComboByDamage) attackOnTime.data.Activate(() => abilityHolder.IsGrounded); }));
                else
                    StartCoroutine(Job(() => WaitUntilOrForSecondsRoutine(() => shouldStopComboByDamage, attackOnTime.time), () => { if (!shouldStopComboByDamage) attackOnTime.data.Activate(); }));
            }

            abilityHolder.ModelAnimator.SetInteger(comboCountStringToHash, comboCount);
        }
    }

    private IEnumerator ResetComboCount(float time)
    {
        yield return ResetComboCount(time, () => true);
    }

    private IEnumerator ResetComboCount(float time, Func<bool> condition)
    {
        yield return new WaitUntilOrForSeconds(()=>shouldStopComboByDamage, time);
        yield return new WaitUntil(()=>condition() || shouldStopComboByDamage);
        isResetting = true;
        StartCoroutine(Job(() => WaitForSecondsRoutine(comboCoolTime), () => isResetting = false));
        comboCount = -1;
        abilityHolder.ModelAnimator.SetInteger(comboCountStringToHash, comboCount);
        abilityHolder.OccupyMovement(false);
        isOccupiedByThis = false;
        comboResetTimer = null;
        currentCombos = null;
    }

    private void AddMove(Move move)
    {
        Vector2 velocityWithFacingDirection = move.velocity;
        if (abilityHolder.isFacingLeft)
            velocityWithFacingDirection.x *= -1;

        if (move.shouldStopBeforeMove)
            abilityHolder.SetVelocity(velocityWithFacingDirection, move.recoverTime);
        else
            abilityHolder.AddVelocity(velocityWithFacingDirection, move.recoverTime);
    }

    public override void OnHit(IHitReactor.HitInfo hitInfo)
    {
        shouldStopComboByDamage = true;
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
    private struct Move
    {
        public bool shouldStopBeforeMove;
        public Vector2 velocity;
        public float recoverTime;
    }

    [System.Serializable]
    private struct ComboInformation
    {
        public float duration;
        public float damageMultiplier;
        public float nextComboInputStart;
        public float nextComboInputDuration;
        public bool canUseInAir;
        public bool keepUntilGrounded;
        public List<DataOnTime<Move>> moveOnTimes;
        public List<DataOnTime<MeleeAttack>> attackOnTime;
    }
}

