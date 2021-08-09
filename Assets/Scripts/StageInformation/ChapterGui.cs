using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChapterGui : ButtonAsThreeStateToggle<ChapterInformation>
{
    [SerializeField]
    private List<Text> labels;

    public override void Initialize(ChapterInformation coreData)
    {
        foreach(Text label in labels)
            label.text = "Á¦" + coreData.ChapterNum + "±¸¿ª";

        this.coreData = coreData;
    }

}