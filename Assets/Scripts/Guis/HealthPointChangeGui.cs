using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthPointChangeGui : FixedWorldPositionGui
{
    [SerializeField]
    private Text text;
    [SerializeField]
    private float moveDistance = 32;
    [SerializeField]
    private float duration = 1f;

    private float accumulatedDeltaTime = 0;

    void Start()
    {
        StartCoroutine(Job(() => WaitForSecondsRoutine(duration), () => Destroy(gameObject)));
    }

    public void Initialize(Camera targetCamera, Vector3 position, int value)
    {
        text.text = value.ToString();

        base.Initialize(targetCamera, rectTransform.parent.GetComponent<RectTransform>(), position);
    }

    protected override Vector2 ScreenPositionAdjust()
    {
        Vector2 retVal = Vector2.up * moveDistance * accumulatedDeltaTime / duration;
        accumulatedDeltaTime += Time.deltaTime;
        return retVal;
    }
}