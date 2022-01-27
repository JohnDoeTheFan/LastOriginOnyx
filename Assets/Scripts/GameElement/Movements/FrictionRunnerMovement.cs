using Onyx.GameElement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrictionRunnerMovement : OneAxisMovementBase
{
    [SerializeField] private GroundChecker groundChecker;

    [Header("Physics")]
    [SerializeField, Range(1, 10)] private float maxVelocity = 3f;
    [SerializeField, Range(0.001f, 5)] private float velocityReachTime = 0.1f;
    [SerializeField, Range(0.001f, 1f)] private float velocityInAirRate = 0.2f;

    bool shouldStop = false;

    public override void OnUpdateMovement(Vector2 inputDirection)
    {
        float currentAbsVelocityX = Mathf.Abs(rigidBody.velocity.x);
        bool isStopped = currentAbsVelocityX - groundChecker.GetGroundVelocity().x == 0;
        if ((lastInputDirection != Vector2.zero && inputDirection != Vector2.zero) || remainAdditionalVelocityRecoverTime != 0 || remainOverridingVelocityRecoverTime != 0)
            shouldStop = true;

        if (_hasOverridingVelocityToProcess)
        {
            _additionalVelocity = Vector2.zero;
            ProcessOverridingVelocity();
        }
        else if(_additionalVelocity != Vector2.zero)
        {
            ProcessAdditionalVelocity();
        }
        else if(remainAdditionalVelocityRecoverTime == 0 && remainOverridingVelocityRecoverTime == 0)
        {
            AddControlMomentum(inputDirection, isStopped);
        }
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
            bool shouldReduceSpeed = rigidBody.velocity.x * maxVelocityUnderEnvironment > 0 && Mathf.Abs(rigidBody.velocity.x) > Mathf.Abs(maxVelocityUnderEnvironment);
            if (shouldReduceSpeed)
            {
                float diffWithMaximumVelocity = maxVelocityUnderEnvironment - rigidBody.velocity.x;

                float velocityIncreaseForTick = diffWithMaximumVelocity;
                if (velocityReachTime > Time.deltaTime)
                    velocityIncreaseForTick *= Time.deltaTime / velocityReachTime;

                float velocityToAdd = velocityIncreaseForTick;
                float impulse = CalcMomentumToChangeVelocity(velocityToAdd);

                rigidBody.AddForce(new Vector2(impulse, 0), ForceMode2D.Impulse);
            }
            else
            {
                float diffWithMaximumVelocity = maxVelocityUnderEnvironment - rigidBody.velocity.x;

                if (diffWithMaximumVelocity * inputDirection.x > 0)
                {
                    float velocityIncreaseForTick = diffWithMaximumVelocity;
                    if (velocityReachTime > Time.deltaTime)
                        velocityIncreaseForTick *= Time.deltaTime / velocityReachTime;

                    float velocityToAdd = velocityIncreaseForTick;
                    float impulse = CalcMomentumToChangeVelocity(velocityToAdd);

                    rigidBody.AddForce(new Vector2(impulse, 0), ForceMode2D.Impulse);
                }
            }


        }
        else if (shouldStop)
        {
            StopUnit();
            shouldStop = false;
        }
    }

    protected override void ProcessOverridingVelocity()
    {
        Vector2 pureVelocity = rigidBody.velocity - groundChecker.GetGroundVelocity();
        Vector2 stoppingVelocity = pureVelocity * -1;
        Vector2 VelocityToAdd = stoppingVelocity + _overridingVelocity;
        rigidBody.AddForce(VelocityToAdd * rigidBody.mass, ForceMode2D.Impulse);
        _overridingVelocity = Vector2.zero;
        _hasOverridingVelocityToProcess = false;
    }

    /// <summary>
    /// Unit 을 멈추기 위해 운동량을 가한다.
    /// </summary>
    /// <param name="maximumStoppingImpulse">멈출 때 사용될 수 있는 최대 힘</param>
    private void StopUnit()
    {
        Vector2 groundVelocity = groundChecker.GetGroundVelocity();

        float velocityToStop = (rigidBody.velocity.x - groundVelocity.x) * -1;

        float impluseToStop = CalcMomentumToChangeVelocity(velocityToStop);

        rigidBody.AddForce(new Vector2(impluseToStop, 0), ForceMode2D.Impulse);
    }
}
