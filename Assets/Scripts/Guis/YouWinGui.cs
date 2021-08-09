using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class YouWinGui : MonoBehaviourBase
{
    [SerializeField]
    private Text value;

    readonly private float displaySeconds = 3f;
    private bool shouldSkip = false;
    private Action afterDisplayScoreAction;

    private void Update()
    {
        if (Input.anyKeyDown)
            shouldSkip = true;
    }

    public void SetScore(int score)
    {
        StartCoroutine(ScoreCoroutine(score));
    }

    private IEnumerator ScoreCoroutine(int targetScore)
    {
        int currentScore = 0;
        value.text = currentScore.ToString();

        int stepValue = Mathf.RoundToInt(targetScore / TargetFrameSeconds * displaySeconds);
        stepValue = Mathf.Max(1, stepValue);

        while(currentScore < targetScore)
        {
            yield return new WaitForSeconds(TargetFrameSeconds);
            if (shouldSkip)
                currentScore = targetScore;
            else
                currentScore = Mathf.Min(stepValue + currentScore, targetScore);
        }

        if (shouldSkip || targetScore == 0)
            yield return new WaitForSeconds(0.5f);

        afterDisplayScoreAction?.Invoke();
    }

    public void SetAfterDisplayScoreAction(Action action)
    {
        afterDisplayScoreAction = action;
    }
}
