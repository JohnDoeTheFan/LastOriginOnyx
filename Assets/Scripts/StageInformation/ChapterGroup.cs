using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChapterGroup : ThreeStateToggleGroup<ChapterInformation>
{
    [SerializeField]
    private float toggleOffsetY;

    public override void Initialize(ChapterInformation coreData)
    {
        ChapterInformation current = coreData;
        ButtonAsThreeStateToggle<ChapterInformation> newestAvailableToggle = null;
        int counter = 0;
        while (current != null)
        {
            ButtonAsThreeStateToggle<ChapterInformation> newToggle = Instantiate<ButtonAsThreeStateToggle<ChapterInformation>>(threeStateTogglePrefab, transform);
            newToggle.RectTransform.anchoredPosition = new Vector2(0, toggleOffsetY * counter++);
            newToggle.Initialize(current);

            RegisterToggle(newToggle);
            if (current.PreviousChapter == null || current.PreviousChapter.IsCleared)
                newestAvailableToggle = newToggle;
            else
                newToggle.Interactable = false;

            current = current.NextChapter;
        }

        if (newestAvailableToggle != null)
        {
            newestAvailableToggle.IsOn = true;
        }
    }
}
