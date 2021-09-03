using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [SerializeField] private Transform targetTransform;
    [SerializeField][Range(0.1f, 10)] float moveTime = 1f;
    [SerializeField] float waitTime;
    [SerializeField] float accuracy = 0.1f;

    private Vector2 startPosition;
    private Vector2 targetPosition;
    private Vector2 moveDistance;
    private Rigidbody2D rigidBody;

    private void Awake()
    {
        rigidBody = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        startPosition = transform.position;
        targetPosition = targetTransform.position;
        moveDistance = targetPosition - startPosition;
        StartCoroutine(Wait());
    }

    private IEnumerator Wait()
    {
        yield return new WaitForSeconds(waitTime);
        StartCoroutine(Move());
    }

    private IEnumerator Move()
    {
        float division = Mathf.Max(Time.deltaTime, moveTime);

        Vector2 moveDistanceOfTick = moveDistance / division;

        rigidBody.velocity = moveDistanceOfTick;

        yield return new WaitForSeconds(moveTime);

        rigidBody.velocity = Vector2.zero;
        rigidBody.MovePosition(targetPosition);

        SwapPosition();

        StartCoroutine(Wait());
    }

    private bool IsArrived()
    {
        Vector2 currentPosition = transform.position;
        Vector2 currentDistance = targetPosition - currentPosition;

        return currentDistance.magnitude < accuracy;
    }

    private void SwapPosition()
    {
        Vector2 temp = startPosition;
        startPosition = targetPosition;
        targetPosition = temp;

        moveDistance = targetPosition - startPosition;
    }

}
