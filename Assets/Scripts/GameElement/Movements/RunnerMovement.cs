using Onyx.GameElement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RunnerMovement : OneAxisMovementBase
{
    [SerializeField] private GroundChecker groundChecker;

    [Header("Physics")]
    [SerializeField, Range(1, 10)] private float maxVelocity = 3f;
    [SerializeField, Range(0.001f, 5)] private float velocityReachTime = 0.1f;
    [SerializeField, Range(0.001f, 1f)] private float velocityInAirRate = 0.2f;
    [SerializeField, Range(1, 10)] private float overSpeedRecoverVelocity = 5f;
    [SerializeField, Range(0.001f, 5)] private float overSpeedRecoverVelocityReachTime = 0.1f;

    public override void OnUpdateMovement(Vector2 inputDirection)
    {
        float currentAbsVelocityX = Mathf.Abs(rigidBody.velocity.x);
        bool isStopped = currentAbsVelocityX == 0;
        bool wasOverSpeed = !Mathf.Approximately(currentAbsVelocityX, maxVelocity) && currentAbsVelocityX > maxVelocity;

        ProcessDamageVelocity();
        if (remainDamageVelocityRecoverTime == 0 && remainSkillVelocityRecoverTime == 0)
            AddControlMomentum(inputDirection, isStopped, wasOverSpeed);
        ProcessSkillVelocity();

        if (remainSkillVelocityRecoverTime == 0)
        {
            if (wasOverSpeed)
                AddOverSpeedRecoverMomentum();
            else
                AddMomentumToLimitVelocity();
        }
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

}
