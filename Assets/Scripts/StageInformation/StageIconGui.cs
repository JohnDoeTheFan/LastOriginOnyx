using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StageIconGui : ButtonAsThreeStateToggle<StageInformation>
{
    [SerializeField]
    private List<Text> labels;
    [SerializeField]
    private RectTransform clearStar;
    [SerializeField]
    private Image previousLine;
    [SerializeField]
    private Image lowerLine;

    public override void Initialize(StageInformation coreData)
    {
        this.coreData = coreData;

        string labelText = coreData.ChapterNumber + "-" + coreData.StageNumber;
        if (coreData.StageType == StageInformation.StageTypeEnum.Side)
            labelText += "B";
        else if(coreData.StageType == StageInformation.StageTypeEnum.Ex)
            labelText += "Ex";

        if (coreData.IsCleared)
            clearStar.gameObject.SetActive(true);

        foreach(Text label in labels)
            label.text = labelText;

        if (coreData.PreviousStage != null)
        {
            if (! coreData.PreviousStage.IsCleared)
                Interactable = false;
        }
    }
}
