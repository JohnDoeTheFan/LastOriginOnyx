using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Utility/StageMap/ChapterInformation")]
public class ChapterInformation : ScriptableObject
{
    [SerializeField]
    private int chapterNum;
    [SerializeField]
    private string chapterName;
    [SerializeField]
    private ChapterInformation previousChapter;
    [SerializeField]
    private ChapterInformation nextChapter;
    [SerializeField]
    private StageInformation firstStage;
    [SerializeField]
    private StageInformation clearanceDecisionStage;

    public int ChapterNum => chapterNum;
    public string ChapterName => chapterName;
    public ChapterInformation PreviousChapter => previousChapter;
    public ChapterInformation NextChapter => nextChapter;
    public StageInformation FirstStage => firstStage;
    public bool IsCleared => clearanceDecisionStage.IsCleared;
}
