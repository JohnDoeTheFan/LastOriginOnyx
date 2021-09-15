using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MovementBase : MonoBehaviour
{
    [SerializeField] Transform rotatingParts;
    [SerializeField, Range(0, 5f)] private float damageVelocityRecoverTime = 0.2f;

    protected Rigidbody2D rigidBody;

    protected float remainDamageVelocityRecoverTime = 0;
    protected float remainSkillVelocityRecoverTime = 0;
    protected Vector2 velocityChangeByDamage;
    protected Vector2 velocityChangeBySkill;
    protected Vector2 lastInputDirection = Vector2.zero;

    public bool IsMovementOccupied { get; set; }
    public bool IsFacingLeft => rotatingParts.rotation.y != 0;

    public virtual void Awake()
    {
        rigidBody = GetComponent<Rigidbody2D>();
    }

    public void UpdateMovement(Vector2 leftStickInput)
    {
        Vector2 inputDirection = Vector2.zero;

        if (!IsMovementOccupied)
        {
            if (leftStickInput.x > 0)
                inputDirection = Vector2.right;
            else if (leftStickInput.x < 0)
                inputDirection = Vector2.left;
        }

        RotateUnit(inputDirection);

        OnUpdateMovement(inputDirection);

        remainDamageVelocityRecoverTime = Mathf.Max(0, remainDamageVelocityRecoverTime - Time.deltaTime);
        remainSkillVelocityRecoverTime = Mathf.Max(0, remainSkillVelocityRecoverTime - Time.deltaTime);

        lastInputDirection = leftStickInput;
    }

    public abstract void OnUpdateMovement(Vector2 inputDirection);

    /// <summary>
    /// Unit 을 회전한다.
    /// </summary>
    /// <param name="inputDirection">입력 방향</param>
    public void RotateUnit(Vector2 inputDirection)
    {
        Quaternion rotation = rotatingParts.rotation;
        if (inputDirection == Vector2.right)
            rotation = Quaternion.identity;
        else if (inputDirection == Vector2.left)
            rotation = Quaternion.Euler(new Vector3(0, 180, 0));

        rotatingParts.rotation = rotation;
    }

    /// <summary>
    /// 피해로 인한 운동량(넉백)을 가한다.
    /// </summary>
    protected void ProcessDamageVelocity()
    {
        if (velocityChangeByDamage != Vector2.zero)
        {
            rigidBody.AddForce(velocityChangeByDamage * rigidBody.mass, ForceMode2D.Impulse);
            velocityChangeByDamage = Vector2.zero;
        }
    }

    protected void ProcessSkillVelocity()
    {
        if (velocityChangeBySkill != Vector2.zero)
        {
            rigidBody.AddForce(velocityChangeBySkill * rigidBody.mass, ForceMode2D.Impulse);
            velocityChangeBySkill = Vector2.zero;
        }
    }

    public void AddDamageVelocity(Vector2 damageVelocity)
    {
        velocityChangeByDamage += damageVelocity;
        remainDamageVelocityRecoverTime = damageVelocityRecoverTime;
    }

    public void AddDamageVelocity(Vector2 damageVelocity, float damageVelocityRecoverTime)
    {
        velocityChangeByDamage += damageVelocity;
        remainDamageVelocityRecoverTime = damageVelocityRecoverTime;
    }

    public void AddSkillVelocity(Vector2 skillVelocity, float skillVelocityRecoverTime)
    {
        remainSkillVelocityRecoverTime = skillVelocityRecoverTime;
        velocityChangeBySkill = skillVelocity;
    }

    /// <summary>
    /// 도달하고자 하는 속도로 가속하기 위한 운동량(Momentum)을 계산한다.
    /// </summary>
    /// <param name="targetVelocity">도달하고자 하는 속도</param>
    /// <returns>운동량</returns>
    protected float CalcMomentumToChangeVelocity(float targetVelocity)
    {
        return rigidBody.mass * targetVelocity;
    }

}
