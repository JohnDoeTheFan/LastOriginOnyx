using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ValueBarGui : MonoBehaviour
{
    private float maxValue = 0;
    private float changeValue = 0;
    private float value = 0;

    [SerializeField]
    private float lengthStandardValue = 1;

    [SerializeField]
    private float lerpSpeed = 5;

    [SerializeField]
    private bool shouldLerpByGameSpeed = false;

    [SerializeField]
    private RectTransform backgroundRectTransform;
    [SerializeField]
    private RectTransform changeTransform;
    [SerializeField]
    private RectTransform valueRectTransform;

    public void SetLengthStandardValue(float value)
    {
        lengthStandardValue = value;
        AdjustBackgroundSize();
    }

    public void SetMaxValue(float value)
    {
        maxValue = value;
        AdjustBackgroundSize();
    }

    public void SetValue(float value)
    {
        if (value > lengthStandardValue)
        {
            SetMaxValue(value);
            SetLengthStandardValue(value);
        }

        this.value = value;
        valueRectTransform.SetAnchorMaxX(this.value / lengthStandardValue);
    }

    void AdjustBackgroundSize()
    {
        backgroundRectTransform.SetAnchorMaxX(maxValue / lengthStandardValue);
    }

    private void Update()
    {
        if (Mathf.Approximately(changeValue, value))
            return;
        else 
        { 
            float time = shouldLerpByGameSpeed ? Time.deltaTime : Time.unscaledDeltaTime;
            changeValue = Mathf.Lerp(changeValue, value, time * lerpSpeed);
            float rate = changeValue / lengthStandardValue;

            changeTransform.SetAnchorMaxX(rate);
        }
    }
}