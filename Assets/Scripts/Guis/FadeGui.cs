using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadeGui : MonoBehaviour
{
    bool isFading = false;
    bool isFadeIn = false;

    bool IsFadeOut { set { isFadeIn = !value; } get { return !isFadeIn; } }

    public enum FinishStatus
    {
        Finished,
        Interrupted
    }
    private readonly List<Action<FinishStatus>> finishNotifiers = new List<Action<FinishStatus>>();

    private Image image;

    [Range(0.1f, 10f)]
    public float fadeTime = 1f;
    [Range(0.01f, 0.1f)]
    public float updateTime = 0.03f;
    public bool startFromFadeOut = true;

    private void Awake()
    {
        image = GetComponent<Image>();
        Color newColor = image.color;
        if (startFromFadeOut)
        {
            isFadeIn = false;
            newColor.a = 1f;
        }
        else
        {
            isFadeIn = true;
            newColor.a = 0f;
        }
        image.color = newColor;
        isFading = false;
    }
        
    public void ForceFadeIn()
    {
        isFading = false;
        isFadeIn = true;
        Color newColor = image.color;
        newColor.a = 0f;
        image.color = newColor;
    }

    public void ForceFadeOut()
    {
        isFading = false;
        isFadeIn = false;
        Color newColor = image.color;
        newColor.a = 1f;
        image.color = newColor;
    }

    public void StartFadeIn(Action<FinishStatus> finishNotifier)
    {
        StartFade(true, finishNotifier);
    }

    public void StartFadeOut(Action<FinishStatus> finishNotifier)
    {
        StartFade(false, finishNotifier);
    }

    private void StartFade(bool isFadeIn, Action<FinishStatus> finishNotifier)
    {
        if (this.isFadeIn == isFadeIn)
        {
            if (isFading)
                finishNotifiers.Add(finishNotifier);
            else
                finishNotifier(FinishStatus.Finished);
        }
        else
        {
            this.isFadeIn = isFadeIn;

            if (isFading)
                NotifyAndFlush(FinishStatus.Interrupted);
            else
            {
                isFading = true;

                StartCoroutine(FadingCoroutine());
            }

            if (finishNotifier != null)
                finishNotifiers.Add(finishNotifier);
        }
    }

    IEnumerator FadingCoroutine()
    {
        yield return new WaitForSecondsRealtime(updateTime);

        if(isFading)
        {
            float alphaModifierMultiplier = updateTime / fadeTime;
            float alphaModifier = -1f * alphaModifierMultiplier;
            float targetAlpha = 0f;
            if (IsFadeOut)
            {
                alphaModifier = 1f * alphaModifierMultiplier;
                targetAlpha = 1f;
            }

            float alpha = AddAlpha(alphaModifier);

            if (alpha == targetAlpha)
            {
                isFading = false;
                NotifyAndFlush(FinishStatus.Finished);
            }
            else
                StartCoroutine(FadingCoroutine());
        }
    }

    public float AddAlpha(float modifier)
    {
        Color currentColor = image.color;
        float newAlpha = Mathf.Max(0, Mathf.Min(1, currentColor.a + modifier));
        Color newColor = new Color(currentColor.r, currentColor.g, currentColor.b, newAlpha);
        image.color = newColor;
        return currentColor.a;
    }

    public void NotifyAndFlush(FinishStatus finishStatus)
    {
        List<Action<FinishStatus>> copyOfFinishNotifiers = new List<Action<FinishStatus>>( finishNotifiers );
        finishNotifiers.Clear();
        copyOfFinishNotifiers.ForEach(item => item(finishStatus));
    }
}
