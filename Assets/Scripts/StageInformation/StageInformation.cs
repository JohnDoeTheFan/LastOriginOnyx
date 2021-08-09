using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using Onyx.Core;
using Onyx.BattleRoom;

[CreateAssetMenu(menuName = "Utility/StageMap/StageInformation")]
public class StageInformation : ScriptableObject
{
    static public Action<StageInformation, Action<bool>> isStageClearedCommunication;
    static public Action<StageInformation, Action> setStageClearedCommunication;

    [SerializeField]
    private int chapterNumber;
    [SerializeField]
    private int stageNumber;
    [SerializeField]
    private StageTypeEnum stageType;
    [SerializeField]
    private StageInformation previousStage;
    [SerializeField]
    private NextStagesStruct nextStages;
    [SerializeField]
    bool isCleared;
    [SerializeField]
    private StageScenesToBeBuild stageScene;
    [SerializeField]
    private string title;
    [SerializeField]
    private string description;
    [SerializeField]
    private int neededPower;
    // Clear reward
    // Four star reward
    // List of rewards
    [SerializeField]
    private List<BattleRoom> battleRoomPrefabs;

    public int ChapterNumber => chapterNumber;
    public int StageNumber => stageNumber;
    public StageTypeEnum StageType => stageType;
    public StageInformation PreviousStage => previousStage;
    public NextStagesStruct NextStages => nextStages;
    public bool IsCleared => isCleared;
    public StageScenesToBeBuild StageScene => stageScene;
    public string Title => title;
    public string Description => description;
    public int NeededPower => neededPower;
    public ReadOnlyCollection<BattleRoom> BattleRoomPrefabs => battleRoomPrefabs.AsReadOnly();

    public enum StageTypeEnum
    {
        Normal,
        Side,
        Ex
    }

    [System.Serializable]
    public struct NextStagesStruct
    {
        public StageInformation nextNormalStage;
        public StageInformation nextSideStage;
        public StageInformation nextExStage;
    }

    public void ReadIsClearedFromServer(Action OnFinished)
    {
        isStageClearedCommunication(this, OnRead);

        void OnRead(bool isCleared)
        {
            this.isCleared = isCleared;
            OnFinished?.Invoke();
        }
    }

    public void WriteIsClearedToServer(Action OnFinished)
    {
        setStageClearedCommunication(this, OnWrite);

        void OnWrite()
        {
            isCleared = true;
            OnFinished?.Invoke();
        }
    }
}
