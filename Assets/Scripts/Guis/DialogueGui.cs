using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogueGui : MonoBehaviour
{
    [SerializeField]
    private Text speakerNameText;
    [SerializeField]
    private Text lineContentText;
    [SerializeField]
    private AudioClip updateSound;
    [SerializeField]
    private RectTransform finishMark;
    [SerializeField]
    [Range(0.01f, 0.5f)]
    private float defaultUpdateDuration = 0.1f;

    private bool isPreserved = false;
    public bool IsAvailable => !isPreserved;

    public interface ILine
    {
        string SpeakerName { get; }
        string Content { get; }
        float UpdateDuration { get; }
    }

    bool shouldSkipCurrentLine = false;

    public bool StartLine(ILine line, Action onEnd)
    {
        if (IsAvailable)
        {
            gameObject.SetActive(true);
            shouldSkipCurrentLine = false;
            finishMark.gameObject.SetActive(false);
            speakerNameText.text = line.SpeakerName;
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

}
