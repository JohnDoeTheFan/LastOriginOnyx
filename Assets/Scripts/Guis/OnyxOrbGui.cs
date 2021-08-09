using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnyxOrbGui : MonoBehaviour
{
    [SerializeField]
    float distanceThreshold = 50f;
    [SerializeField]
    [Range(1, 89)]
    float initialAngle = 15f;
    [SerializeField]
    float acceleration = 5000f;
    [SerializeField]
    float maxVelocity = 3000f;
    [SerializeField]
    float minInitialVelocity = 500f;
    [SerializeField]
    float maxInitialVelocity = 1500f;
    [SerializeField]
    [Range(0.001f, 0.5f)]
    float minVelocityRate = 0.1f;
    [SerializeField]
    float dampingDistance = 300;
    [SerializeField]
    float stopTime = 0.3f;
    [SerializeField]
    float stopAndAccelationTimeOverlap = 0.3f;
    [SerializeField]
    float failTimeout = 1f;
    [SerializeField]
    RectTransform onyxGettingEffectPrefab;

    private RectTransform rectTransform;
    private Vector2 targetPosition;
    private Action onArrived;
    private Vector2 speed;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void Update()
    {
        // Move
        rectTransform.anchoredPosition += speed * Time.deltaTime;

        // Check distance;
        if (Vector2.Distance(targetPosition, rectTransform.anchoredPosition) < distanceThreshold)
        {
            GetOrb();
        }
    }

    private IEnumerator StartStopping(float time)
    {
        float startTime = Time.time;
        while(Time.time - startTime < time)
        {
            yield return null;
            speed -= speed.normalized * acceleration * Time.deltaTime;
        }
    }

    private IEnumerator HeadToTarget(float delay)
    {
        yield return new WaitForSeconds(delay);

        while(true)
        {
            yield return null;

            Vector2 direction = targetPosition - rectTransform.anchoredPosition;
            speed += direction.normalized * acceleration * Time.deltaTime;

            float distance = Vector2.Distance(targetPosition, rectTransform.anchoredPosition);
            float maxVelocityMultiplier = distance / dampingDistance;

            maxVelocityMultiplier = Mathf.Max(maxVelocityMultiplier, maxVelocityMultiplier * minVelocityRate);

            speed = LimitVelocity(speed, maxVelocity * maxVelocityMultiplier);
        }
    }

    private IEnumerator ForceGettingAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);

        GetOrb();
    }

    public void Initialize(Vector2 startPosition, Vector2 targetPosition, Action onArrived)
    {
        rectTransform.anchoredPosition = startPosition;
        this.targetPosition = targetPosition;
        this.onArrived = onArrived;

        Vector2 randomRight = MakeRandomNormalVector2(-initialAngle, 0);
        Vector2 randomLeft = MakeRandomNormalVector2(180, 180 + initialAngle);

        float decisionValue = UnityEngine.Random.Range(0f, 1f);
        Vector2 randomDirection = (decisionValue < 0.5)? randomLeft:randomRight;

        speed = randomDirection * UnityEngine.Random.Range(minInitialVelocity, maxInitialVelocity);

        StartCoroutine(StartStopping(stopTime));
        StartCoroutine(HeadToTarget(Mathf.Max(0, stopTime - stopAndAccelationTimeOverlap)));
        StartCoroutine(ForceGettingAfterSeconds(failTimeout));
    }

    static public Vector2 MakeRandomNormalVector2(float degreeMin, float degreeMax)
    {
        float degree = UnityEngine.Random.Range(degreeMin, degreeMax);
        float radian = degree * Mathf.Deg2Rad;

        return new Vector2(Mathf.Cos(radian), Mathf.Sin(radian));
    }

    static public Vector2 LimitVelocity(Vector2 speed, float velocityLimit)
    {
        if (velocityLimit > 0f && speed.magnitude != 0 && speed.magnitude >= velocityLimit)
            speed *= velocityLimit / speed.magnitude;

        return speed;
    }

    private void GetOrb()
    {
        onArrived?.Invoke();
        if (onyxGettingEffectPrefab != null)
        {
            RectTransform newEffect = Instantiate<RectTransform>(onyxGettingEffectPrefab, rectTransform.parent);
            newEffect.anchoredPosition = rectTransform.anchoredPosition;
        }
        Destroy(gameObject);
    }
}