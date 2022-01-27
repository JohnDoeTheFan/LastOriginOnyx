using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingMovement : MovementBase
{
    [SerializeField, Range(1, 10)] float maxVelocity = 2;
    [SerializeField, Range(0.001f, 5)] private float velocityReachTime = 0.1f;

    bool isDead = false;

    public override void OnUpdateMovement(Vector2 inputDirection)
    {
        bool isStopped = rigidBody.velocity == Vector2.zero;

        ProcessAdditionalVelocity();

        if(! isDead)
        {
            if (remainAdditionalVelocityRecoverTime == 0 && remainOverridingVelocityRecoverTime == 0)
                AddControlMomentum(inputDirection, isStopped);
            if(_hasOverridingVelocityToProcess)
                ProcessOverridingVelocity();
        }
    }

    private void AddControlMomentum(Vector2 inputDirection, bool isStopped)
    {
        bool isInput = inputDirection != Vector2.zero;
        if (isInput)
        {
            Vector2 rotatedMaxVelocity = maxVelocity * inputDirection.normalized;

            Vector2 diffWithMaximumVelocity = rotatedMaxVelocity - rigidBody.velocity;

            Vector2 velocityIncreaseForTick = diffWithMaximumVelocity;
            if (velocityReachTime > Time.deltaTime)
                velocityIncreaseForTick *= Time.deltaTime / velocityReachTime;

            Vector2 velocityToAdd = velocityIncreaseForTick;
            Vector2 impulse = CalcMomentumToChangeVelocity(velocityToAdd);

            rigidBody.AddForce(impulse, ForceMode2D.Impulse);
        }
        else if (!isStopped)
        {
            StopUnit();
        }
    }

    /// <summary>
    /// Unit 을 멈추기 위해 운동량을 가한다.
    /// </summary>
    /// <param name="maximumStoppingImpulse">멈출 때 사용될 수 있는 최대 힘</param>
    private void StopUnit()
    {
        Vector2 velocityToStop = rigidBody.velocity * -1;
        Vector2 impluseToStop = CalcMomentumToChangeVelocity(velocityToStop);

        rigidBody.AddForce(impluseToStop, ForceMode2D.Impulse);
    }

    protected override void OnDead()
    {
        rigidBody.gravityScale = 1;
        isDead = true;
    }
}
