using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [SerializeField] private Transform targetTransform;
    [SerializeField] [Range(0.1f, 20)] float moveTime = 1f;
    [SerializeField] [Range(0.01f, 0.5f)] float accelerationTimePercentage = 0.25f;
    [SerializeField] float waitTime;

    private Vector2 startPosition;
    private Vector2 targetPosition;
    private Rigidbody2D rigidBody;

    private bool isMoving = false;
    private float moveStartTime;

    private void Awake()
    {
        rigidBody = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        if (isMoving)
        {
            if (Time.fixedTime >= moveStartTime + moveTime)
            {
                rigidBody.MovePosition(targetPosition);
                rigidBody.velocity = Vector2.zero;
                SwapTargetPosition();
                isMoving = false;
                StartCoroutine(Wait());
            }
            else
            {
                Vector2 newVelocity = CalcVelocityPerTime(startPosition, targetPosition, moveTime, accelerationTimePercentage, Time.fixedTime - moveStartTime);
                rigidBody.velocity = newVelocity;
            }

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
    }

    private void MoveWithFixed()
    {
        isMoving = true;
        moveStartTime = Time.fixedTime;
    }

    private void SwapTargetPosition()
    {
        Vector2 temp = startPosition;
        startPosition = targetPosition;
        targetPosition = temp;
    }
#if UNITY_EDITOR
    private SpriteRenderer spriteRenderer;

    private void OnDrawGizmos()
    {
        if(spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
        Gizmos.DrawWireCube(targetTransform.position, spriteRenderer.size);
        Gizmos.DrawLine(transform.position, targetTransform.position);
    }
#endif
}
