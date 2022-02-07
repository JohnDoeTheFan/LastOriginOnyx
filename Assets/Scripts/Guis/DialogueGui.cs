using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogueGui : MonoBehaviour
{
    public event Action ClickedSkipButton;

    [SerializeField] private Text speakerNameText;
    [SerializeField] private Text lineContentText;
    [SerializeField] private AudioClip updateSound;
    [SerializeField] private Image leftPortrait;
    [SerializeField] private Image centerPortrait;
    [SerializeField] private Image rightPortrait;
    [SerializeField] private RectTransform finishMark;
    [SerializeField] [Range(0.01f, 1f)] private float defaultFadeDuration = 0.5f;
    [SerializeField] [Range(0.01f, 0.5f)] private float defaultUpdateDuration = 0.1f;
    [SerializeField] [Range(30, 60)]private int targetFps = 60;

    private bool isPreserved = false;
    public bool IsAvailable => !isPreserved;

    public float TargetFrameTime => 1.0f / targetFps;

    public interface ILine
    {
        Sprite Portrait { get; }
        PortraitPosition PortraitPosition { get; }
        string SpeakerName { get; }
        string Content { get; }
        float UpdateDuration { get; }
    }

    public enum PortraitPosition
    {
        Left,
        Center,
        Right
    }

    bool shouldSkipCurrentLine = false;

    public void Clear()
    {
        leftPortrait.sprite = null;
        centerPortrait.sprite = null;
        rightPortrait.sprite = null;

        Color transparent = new Color(1, 1, 1, 0);
        leftPortrait.color = transparent;
        centerPortrait.color = transparent;
        rightPortrait.color = transparent;
    }

    public bool StartLine(ILine line, Action onEnd)
    {
        if (IsAvailable)
        {
            gameObject.SetActive(true);
            shouldSkipCurrentLine = false;
            finishMark.gameObject.SetActive(false);
            speakerNameText.text = line.SpeakerName;
            StartCoroutine(PortraitUpdateRoutine(line, null, defaultFadeDuration));
            StartCoroutine(LineContentUpdateRoutine(line, onEnd, line.UpdateDuration > 0 ? line.UpdateDuration : defaultUpdateDuration));

            return true;
        }
        else
        {
            return false;
        }
    }

    public void SkipLine()
    {
        shouldSkipCurrentLine = true;
    }

    public void HideGui()
    {
        Clear();

        gameObject.SetActive(false);
    }

    private IEnumerator LineContentUpdateRoutine(ILine line, Action onEnd, float updateDuration)
    {
        isPreserved = true;
        lineContentText.text = string.Empty;
        foreach(char ch in line.Content)
        {
            lineContentText.text += ch;
            if(ch != ' ')
            {
                // Play sound
                yield return new WaitUntilOrForSeconds(() => shouldSkipCurrentLine, updateDuration);
                if (shouldSkipCurrentLine)
                {
                    lineContentText.text = line.Content;
                    break;
                }
            }
        }

        finishMark.gameObject.SetActive(true);
        isPreserved = false;
        onEnd();
    }

    private IEnumerator PortraitUpdateRoutine(ILine line, Action onEnd, float fadeDuration)
    {
        float half = 0.5f;
        leftPortrait.color = new Color(half, half, half, leftPortrait.color.a);
        centerPortrait.color = new Color(half, half, half, centerPortrait.color.a);
        rightPortrait.color = new Color(half, half, half, rightPortrait.color.a);

        Image targetImage = line.PortraitPosition switch
        {
            PortraitPosition.Left => leftPortrait,
            PortraitPosition.Center => centerPortrait,
            PortraitPosition.Right => rightPortrait,
            _ => throw new NotImplementedException(),
        };

        targetImage.color = new Color(1f, 1f, 1f, targetImage.color.a);

        if (targetImage.sprite == line.Portrait)
            yield break;

        if(targetImage.sprite != null)
            yield return Fade(targetImage, 1, 0, fadeDuration, TargetFrameTime, () => shouldSkipCurrentLine);

        targetImage.sprite = line.Portrait;
        if (targetImage.sprite != null)
            yield return Fade(targetImage, 0, 1, fadeDuration, TargetFrameTime, () => shouldSkipCurrentLine);
    }

    private static IEnumerator Fade(Image image, float from, float to, float duration, float frameTime, Func<bool> shouldSkipImmediately)
    {
        float alpha = from;
        float fadeValue = (to - from) / duration * frameTime;
        while (!shouldSkipImmediately() && alpha != to)
        {
            yield return new WaitUntilOrForSeconds(shouldSkipImmediately, frameTime);
            alpha = Mathf.Clamp(alpha + fadeValue, Mathf.Min(to, from), Mathf.Max(to, from));
            SetAlpha(image, alpha);
        }
        SetAlpha(image, to);
    }

    private static void SetAlpha(Image image, float newAlpha)
    {
        Color newColor = image.color;
        newColor.a = newAlpha;
        image.color = newColor;
    }

    public void NotifyClickedSkipButton()
    {
        ClickedSkipButton?.Invoke();
    }
}
