using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [SerializeField] private Transform targetTransform;
    [SerializeField] [Range(1f, 10)] float moveTime = 1f;
    [SerializeField] [Range(0.1f, 0.5f)] float accelerationTimePercentage = 0.25f;
    [SerializeField] float smoothTime;
    [SerializeField] float waitTime;

    private Vector2 startPosition;
    private Vector2 targetPosition;
    private Rigidbody2D rigidBody;

    private void Awake()
    {
        rigidBody = GetComponent<Rigidbody2D>();
    }

    bool isMoving = false;
    float moveStartTime;
    Vector2 velocity = new Vector2();

    private void FixedUpdate()
    {
        if (isMoving)
        {
            Vector2 moveDistance = targetPosition - startPosition;
            float accelerationTime = moveTime * accelerationTimePercentage;
            Vector2 constantVelocity = moveDistance / (moveTime - accelerationTime);
            //Vector2 newPosition = Vector2.SmoothDamp(rigidBody.position, targetPosition, ref velocity, smoothTime, constantVelocity.magnitude, Time.fixedDeltaTime);
            //Vector2 newPosition = Vector2.Lerp(startPosition, targetPosition, (Time.fixedTime - moveStartTime) / moveTime);
            //Vector2 newPosition = CalcPositionPerTime(startPosition, targetPosition, moveTime, accelerationTimePercentage, Time.fixedTime - moveStartTime);

            /*
            //Debug.Log(newPosition.x +", " + newPosition.y);
            //rigidBody.MovePosition(newPosition);
            if (newPosition == targetPosition)
            {
                isMoving = false;
                SwapPosition();
                Debug.Log(Time.fixedTime - moveStartTime);
                StartCoroutine(Wait());
            }
            */

            if (Time.fixedTime >= moveStartTime + moveTime)
            {
                rigidBody.MovePosition(targetPosition);
                SwapTargetPosition();
                isMoving = false;
                rigidBody.velocity = Vector2.zero;
                StartCoroutine(Wait());
            }
            else
            {
                Vector2 newVelocity = CalcVelocityPerTime(startPosition, targetPosition, moveTime, accelerationTimePercentage, Time.fixedTime - moveStartTime);
                rigidBody.velocity = newVelocity;
            }

        }
    }

    private Vector2 CalcPositionPerTime(Vector2 startPosition, Vector2 targetPosition, float moveTime, float accelerationTimePercentage, float passedTime)
    {
        Vector2 moveDistance = targetPosition - startPosition;
        float accelerationTime = moveTime * accelerationTimePercentage;
        float constantVelocityTime = moveTime - (accelerationTime * 2);

        Vector2 constantVelocity = moveDistance / (moveTime - accelerationTime);

        if (passedTime <= accelerationTime)
        {
            Vector2 acceleration = constantVelocity / accelerationTime;
            Vector2 averageVelocity = acceleration * passedTime / 2;
            return startPosition + averageVelocity * passedTime;
        }
        else if(passedTime <= accelerationTime + constantVelocityTime)
        {
            float constantMovedTime = passedTime - accelerationTime;
            Vector2 constantMovement = constantVelocity * constantMovedTime;
            return startPosition + (constantVelocity * accelerationTime / 2) + constantMovement;
        }
        else if(passedTime <= moveTime)
        {
            Vector2 constantMovement = constantVelocity * constantVelocityTime;

            Vector2 deceleration = - constantVelocity / accelerationTime;
            float deceleratedTime = passedTime - (accelerationTime + constantVelocityTime);
            Vector2 deceleratedVelocity = constantVelocity + (deceleration * deceleratedTime);

            Vector2 averageVelocity = (constantVelocity + deceleratedVelocity) / 2;
            Vector2 deceleratedPosition = averageVelocity * deceleratedTime;

            return startPosition + (constantVelocity * accelerationTime / 2) + constantMovement + deceleratedPosition;
        }
        else
        {
            return targetPosition;
        }
    }
    private Vector2 CalcVelocityPerTime(Vector2 startPosition, Vector2 targetPosition, float moveTime, float accelerationTimePercentage, float passedTime)
    {
        Vector2 moveDistance = targetPosition - startPosition;
        float accelerationTime = moveTime * accelerationTimePercentage;
        float constantVelocityTime = moveTime - (accelerationTime * 2);

        Vector2 constantVelocity = moveDistance / (moveTime - accelerationTime);

        if (passedTime <= accelerationTime)
        {
            Vector2 acceleration = constantVelocity / accelerationTime;
            Vector2 averageVelocity = acceleration * passedTime / 2;
            return averageVelocity;
        }
        else if (passedTime <= accelerationTime + constantVelocityTime)
        {
            return constantVelocity;
        }
        else if (passedTime <= moveTime)
        {
            Vector2 deceleration = -constantVelocity / accelerationTime;
            float deceleratedTime = passedTime - (accelerationTime + constantVelocityTime);
            Vector2 deceleratedVelocity = constantVelocity + (deceleration * deceleratedTime);
            Vector2 averageVelocity = (constantVelocity + deceleratedVelocity) / 2;

            return averageVelocity;
        }
        else
        {
            return Vector2.zero;
        }

    }

    private void Start()
    {
        startPosition = transform.position;
        targetPosition = targetTransform.position;
        StartCoroutine(Wait());
    }

    private IEnumerator Wait()
    {
        yield return new WaitForSeconds(waitTime);

        MoveWithFixed();
        // StartCoroutine(MoveWithVelocity());
    }

    private void MoveWithFixed()
    {
        isMoving = true;
        moveStartTime = Time.fixedTime;
    }

    private IEnumerator MoveWithVelocity()
    {
        Vector2 moveDistance = targetPosition - rigidBody.position;
        float accelerationTime = moveTime * accelerationTimePercentage;
        float constantVelocityMoveTime = moveTime - (accelerationTime * 2);

        Vector2 targetVelocity = moveDistance / (moveTime - accelerationTime);

        yield return StartCoroutine(Accelerate(accelerationTime, targetVelocity));

        yield return new WaitForSeconds(constantVelocityMoveTime);

        yield return StartCoroutine(Accelerate(accelerationTime, Vector2.zero));

        SwapTargetPosition();

        StartCoroutine(Wait());
    }

    private IEnumerator Accelerate(float accelerationTime, Vector2 targetVelocity)
    {
        Vector2 startVelocity = rigidBody.velocity;
        float startTime = Time.time;
        float endTime = startTime + accelerationTime;
        while (Time.time < endTime)
        {
            float passedTime = (Time.time - startTime);
            float timePercentage = passedTime / accelerationTime;
            rigidBody.velocity = Vector2.Lerp(startVelocity, targetVelocity, timePercentage);
            yield return null;
        }
        rigidBody.velocity = targetVelocity;
    }

    private void SwapTargetPosition()
    {
        Vector2 temp = startPosition;
        startPosition = targetPosition;
        targetPosition = temp;
    }

}
