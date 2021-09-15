using Onyx.GameElement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrictionRunnerMovement : MovementBase
{
    [SerializeField] private GroundChecker groundChecker;

    [Header("Physics")]
    [SerializeField, Range(1, 10)] private float maxVelocity = 3f;
    [SerializeField, Range(0.001f, 5)] private float velocityReachTime = 0.1f;
    [SerializeField, Range(0.001f, 1f)] private float velocityInAirRate = 0.2f;

    public override void OnUpdateMovement(Vector2 inputDirection)
    {
        float currentAbsVelocityX = Mathf.Abs(rigidBody.velocity.x);
        bool isStopped = currentAbsVelocityX == 0;

        ProcessDamageVelocity();
        if (remainDamageVelocityRecoverTime == 0 && remainSkillVelocityRecoverTime == 0)
            AddControlMomentum(inputDirection, isStopped);
        ProcessSkillVelocity();
    }

    private void AddControlMomentum(Vector2 inputDirection, bool isStopped)
    {
        bool isInput = inputDirection != Vector2.zero;
        if (isInput)
        {
            Vector2 groundVelocity = groundChecker.GetGroundVelocity();

            float rotatedMaxVelocity = maxVelocity;
            if (inputDirection.x < 0)
                rotatedMaxVelocity *= -1;

            float maxVelocityUnderEnvironment = rotatedMaxVelocity + groundVelocity.x;
            float diffWithMaximumVelocity = maxVelocityUnderEnvironment - rigidBody.velocity.x;

            float velocityIncreaseForTick = diffWithMaximumVelocity;
            if (velocityReachTime > Time.deltaTime)
                velocityIncreaseForTick *= Time.deltaTime / velocityReachTime;

            float velocityToAdd = velocityIncreaseForTick;
            float impulse = CalcMomentumToChangeVelocity(velocityToAdd);

            rigidBody.AddForce(new Vector2(impulse, 0), ForceMode2D.Impulse);
        }
        else if (lastInputDirection != Vector2.zero)
        {
            StopUnit2();
        }
    }

    /// <summary>
    /// Unit 을 멈추기 위해 운동량을 가한다.
    /// </summary>
    /// <param name="maximumStoppingImpulse">멈출 때 사용될 수 있는 최대 힘</param>
    private void StopUnit2()
    {
        Vector2 groundVelocity = groundChecker.GetGroundVelocity();

        float velocityToStop = (rigidBody.velocity.x - groundVelocity.x) * -1;

        float impluseToStop = CalcMomentumToChangeVelocity(velocityToStop);

        rigidBody.AddForce(new Vector2(impluseToStop, 0), ForceMode2D.Impulse);
    }
}
