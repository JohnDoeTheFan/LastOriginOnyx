using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MovementBase : MonoBehaviour
{
    [SerializeField] Transform rotatingParts;
    [SerializeField, Range(0, 5f)] private float _defaultRecoverTime = 0.2f;

    protected Rigidbody2D rigidBody;

    protected float remainAdditionalVelocityRecoverTime = 0;
    protected float remainOverridingVelocityRecoverTime = 0;
    protected Vector2 _additionalVelocity;
    protected bool _hasOverridingVelocityToProcess = false;
    protected Vector2 _overridingVelocity;
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

        remainAdditionalVelocityRecoverTime = Mathf.Max(0, remainAdditionalVelocityRecoverTime - Time.deltaTime);
        remainOverridingVelocityRecoverTime = Mathf.Max(0, remainOverridingVelocityRecoverTime - Time.deltaTime);

        lastInputDirection = leftStickInput;
    }

    public virtual Vector2 CullDirection(Vector2 leftStickInput)
    {
        return leftStickInput;
    }

    public abstract void OnUpdateMovement(Vector2 inputDirection);

    /// <summary>
    /// Unit 을 회전한다.
    /// </summary>
    /// <param name="inputDirection">입력 방향</param>
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
    /// 피해로 인한 운동량(넉백)을 가한다.
    /// </summary>
    protected void ProcessAdditionalVelocity()
    {
        if (_additionalVelocity != Vector2.zero)
        {
            rigidBody.AddForce(_additionalVelocity * rigidBody.mass, ForceMode2D.Impulse);
            _additionalVelocity = Vector2.zero;
        }
    }

    protected virtual void ProcessOverridingVelocity()
    {
        Vector2 stoppingVelocity = rigidBody.velocity * -1;
        Vector2 VelocityToAdd = stoppingVelocity + _overridingVelocity;
        rigidBody.AddForce(VelocityToAdd * rigidBody.mass, ForceMode2D.Impulse);
        _overridingVelocity = Vector2.zero;
        _hasOverridingVelocityToProcess = false;
    }

    public void AddAddtionalVelocity(Vector2 additionalVelocity)
    {
        _additionalVelocity += additionalVelocity;
        remainAdditionalVelocityRecoverTime = _defaultRecoverTime;
    }

    public void AddAddtionalVelocity(Vector2 additionalVelocity, float recoverTime)
    {
        _additionalVelocity += additionalVelocity;
        remainAdditionalVelocityRecoverTime = recoverTime;
    }

    public void SetOverridingVelocity(Vector2 overridingVelocity)
    {
        _hasOverridingVelocityToProcess = true;
        _overridingVelocity = overridingVelocity;
        remainOverridingVelocityRecoverTime = _defaultRecoverTime;
    }

    public void SetOverridingVelocity(Vector2 overridingVelocity, float recoverTime)
    {
        _hasOverridingVelocityToProcess = true;
        _overridingVelocity = overridingVelocity;
        remainOverridingVelocityRecoverTime = recoverTime;
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
    /// 도달하고자 하는 속도로 가속하기 위한 운동량(Momentum)을 계산한다.
    /// </summary>
    /// <param name="targetVelocity">도달하고자 하는 속도</param>
    /// <returns>운동량</returns>
    protected float CalcMomentumToChangeVelocity(float targetVelocity)
    {
        return rigidBody.mass * targetVelocity;
    }

    /// <summary>
    /// 도달하고자 하는 속도로 가속하기 위한 운동량(Momentum)을 계산한다.
    /// </summary>
    /// <param name="targetVelocity">도달하고자 하는 속도</param>
    /// <returns>운동량</returns>
    protected Vector2 CalcMomentumToChangeVelocity(Vector2 targetVelocity)
    {
        return rigidBody.mass * targetVelocity;
    }

}
