using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleObjectiveRecordGui : MonoBehaviour
{
    [SerializeField]
    private Text text;

    Animator animator;
    RectTransform rectTransform;
    string description;

    public RectTransform RectTransform => rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        animator = GetComponent<Animator>();
    }
    public void Initialize(string description)
    {
        this.description = description;
        text.text = description;
    }

    public void UpdateProgress(int progress, int progressWhole)
    {
        text.text = string.Format("{0} ({1}/{2})", description, progress, progressWhole);
        if (progress == progressWhole)
            OnClear();
    }

    public void UpdateProgress(float progress)
    {
        text.text = string.Format("{0} ({1:F0}%)", description, progress * 100);
        if (progress == 1f)
            OnClear();
    }

    public void OnClear()
    {
        animator.SetBool("IsOn", true);
    }
}
