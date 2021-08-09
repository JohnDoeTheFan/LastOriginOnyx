using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Onyx.Communication
{
    [CreateAssetMenu(menuName = "Utility/SaveData")]
    public class SaveDataAsset : ScriptableObject
    {
        [SerializeField]
        private bool doNotSaveForTest;
        [SerializeField]
        private Vector2 networkDelay;
        [SerializeField]
        private List<StageInfo> stageClearedList;
        [SerializeField]
        private Vector2 greetingPosition;
        [SerializeField]
        private float greetingSize;
        [SerializeField]
        private int onyxValue;
        [SerializeField]
        private int playerLevel = 1;
        [SerializeField]
        private List<int> levelUpCost;

        public Vector2 NetworkDelay => networkDelay;
        public Vector2 GreetingPosition => greetingPosition;
        public float GreetingSize => greetingSize;
        public int OnyxValue => onyxValue;
        public int PlayerLevel => playerLevel;

        public void SetDoNotSaveForTest(bool doNot)
        {
            doNotSaveForTest = doNot;
        }

        public bool IsStageCleared(int chapterNum, int stageNum, int stageType)
        {
            StageInfo stageInfo = stageClearedList.Find((item) => item.chapterNum == chapterNum && item.stageNum == stageNum && item.stageType == stageType);

            if (IsVaild(stageInfo))
                return stageInfo.isCleared;
            else
                return false;

            static bool IsVaild(StageInfo stageInfo)
            {
                return stageInfo.chapterNum != 0 && stageInfo.stageNum != 0;
            }
        }

        public void SetStageCleared(int chapterNum, int stageNum, int stageType)
        {
            int index = stageClearedList.FindIndex((item) => item.chapterNum == chapterNum && item.stageNum == stageNum && item.stageType == stageType);

            if (index != -1)
            {
                StageInfo stageInfo = stageClearedList[index];
                stageInfo.isCleared = true;
                stageClearedList[index] = stageInfo;
            }
            else
            {
                StageInfo newStageInfo = new StageInfo();
                newStageInfo.chapterNum = chapterNum;
                newStageInfo.stageNum = stageNum;
                newStageInfo.stageType = stageType;
                newStageInfo.isCleared = true;
                stageClearedList.Add(newStageInfo);
            }

            SaveAsFile();
        }

        public void SetGreetingData(Vector2 position, float size)
        {
            greetingPosition = position;
            greetingSize = size;

            SaveAsFile();
        }

        public void AddOnyx(int onyxValue)
        {
            this.onyxValue += onyxValue;

            SaveAsFile();
        }

        public bool LevelUpPlayer()
        {
            if (IsNotMaxLevel() && onyxValue >= GetLevelUpCost())
            {
                onyxValue -= GetLevelUpCost();
                playerLevel += 1;

                SaveAsFile();

                return true;
            }
            else
                return false;
        }

        public int GetLevelUpCost()
        {
            int index = playerLevel - 1;
            if (IsNotMaxLevel())
                return levelUpCost[index];
            else
                return int.MaxValue;
        }

        public bool IsNotMaxLevel()
        {
            int index = playerLevel - 1;
            return index < levelUpCost.Count;
        }

        public void SaveAsFile()
        {
            if (doNotSaveForTest)
                return;

            string fullPath = Path.Combine(Application.persistentDataPath, "OnyxSaveData0.dat");
            string jsonData = JsonUtility.ToJson(this);

            try
            {
                File.WriteAllText(fullPath, jsonData);
                Debug.Log("Saved. Path: " + fullPath);
            }
            catch (Exception e)
            {
                Debug.LogError("Save data writing Failed. Path: " + fullPath + ", Exception: " + e);
            }
        }

        public bool LoadFromFile()
        {
            string fullPath = Path.Combine(Application.persistentDataPath, "OnyxSaveData0.dat");

            try
            {
                string jsonData = File.ReadAllText(fullPath);
                Debug.Log("Loaded. Path: " + fullPath);

                JsonUtility.FromJsonOverwrite(jsonData, this);

                return true;
            }
            catch (Exception e)
            {
                Debug.LogError("Save data reading Failed. Path: " + fullPath + ", Exception: " + e);

                return false;
            }
        }

        [Serializable]
        private struct StageInfo
        {
            public int chapterNum;
            public int stageNum;
            public int stageType;
            public bool isCleared;
        }
    }
}