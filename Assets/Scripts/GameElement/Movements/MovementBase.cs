using System;
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
            inputDirection = CullDirection(leftStickInput);

        RotateUnit(inputDirection);

        OnUpdateMovement(inputDirection);

        remainDamageVelocityRecoverTime = Mathf.Max(0, remainDamageVelocityRecoverTime - Time.deltaTime);
        remainSkillVelocityRecoverTime = Mathf.Max(0, remainSkillVelocityRecoverTime - Time.deltaTime);

        lastInputDirection = leftStickInput;
    }

    public virtual Vector2 CullDirection(Vector2 leftStickInput)
    {
        return leftStickInput;
    }

    public abstract void OnUpdateMovement(Vector2 inputDirection);

    /// <summary>
    /// Unit �� ȸ���Ѵ�.
    /// </summary>
    /// <param name="inputDirection">�Է� ����</param>
    public void RotateUnit(Vector2 inputDirection)
    {
        Quaternion rotation = rotatingParts.rotation;
        if (inputDirection.x > 0)
            rotation = Quaternion.identity;
        else if (inputDirection.x < 0)
            rotation = Quaternion.Euler(new Vector3(0, 180, 0));

        rotatingParts.rotation = rotation;
    }

    /// <summary>
    /// ���ط� ���� ���(�˹�)�� ���Ѵ�.
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
    public void SetDead()
    {
        OnDead();
    }

    protected virtual void OnDead()
    {
        return;
    }

    /// <summary>
    /// �����ϰ��� �ϴ� �ӵ��� �����ϱ� ���� ���(Momentum)�� ����Ѵ�.
    /// </summary>
    /// <param name="targetVelocity">�����ϰ��� �ϴ� �ӵ�</param>
    /// <returns>���</returns>
    protected float CalcMomentumToChangeVelocity(float targetVelocity)
    {
        return rigidBody.mass * targetVelocity;
    }

    /// <summary>
    /// �����ϰ��� �ϴ� �ӵ��� �����ϱ� ���� ���(Momentum)�� ����Ѵ�.
    /// </summary>
    /// <param name="targetVelocity">�����ϰ��� �ϴ� �ӵ�</param>
    /// <returns>���</returns>
    protected Vector2 CalcMomentumToChangeVelocity(Vector2 targetVelocity)
    {
        return rigidBody.mass * targetVelocity;
    }

}
