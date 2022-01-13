using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using UnityEngine;

namespace Onyx.Communication
{
    [CreateAssetMenu(menuName = "Utility/SaveData")]
    public class SaveDataAsset : ScriptableObject
    {
        [SerializeField] private SaveData saveData;
        [SerializeField] private bool doNotSaveForTest;
        [SerializeField] private Vector2 networkDelay;
        [SerializeField] private List<int> levelUpCost;
        [SerializeField] private BioroidList bioroidInformations;

        public Vector2 NetworkDelay => networkDelay;
        public Vector2 GreetingPosition => saveData.greetingPosition;
        public float GreetingSize => saveData.greetingSize;
        public int OnyxValue => saveData.onyxValue;
        public int PlayerLevel => saveData.playerLevel;
        public int AideBioroidId => saveData.aideBioroidId;
        public ReadOnlyCollection<int> OwningBioroidsIds => saveData.owningBioroidsIds.AsReadOnly();

        public void SetDoNotSaveForTest(bool doNot)
        {
            doNotSaveForTest = doNot;
        }

        public bool IsStageCleared(int chapterNum, int stageNum, int stageType)
        {
            StageInfo stageInfo = saveData.stageClearedList.Find((item) => item.chapterNum == chapterNum && item.stageNum == stageNum && item.stageType == stageType);

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
            int index = saveData.stageClearedList.FindIndex((item) => item.chapterNum == chapterNum && item.stageNum == stageNum && item.stageType == stageType);

            if (index != -1)
            {
                StageInfo stageInfo = saveData.stageClearedList[index];
                stageInfo.isCleared = true;
                saveData.stageClearedList[index] = stageInfo;
            }
            else
            {
                StageInfo newStageInfo = new StageInfo();
                newStageInfo.chapterNum = chapterNum;
                newStageInfo.stageNum = stageNum;
                newStageInfo.stageType = stageType;
                newStageInfo.isCleared = true;
                saveData.stageClearedList.Add(newStageInfo);
            }

            SaveAsFile();
        }

        public void SetGreetingData(Vector2 position, float size)
        {
            saveData.greetingPosition = position;
            saveData.greetingSize = size;

            SaveAsFile();
        }

        public void AddOnyx(int onyxValue)
        {
            saveData.onyxValue += onyxValue;

            SaveAsFile();
        }


        public bool LevelUpPlayer()
        {
            if (IsNotMaxLevel() && saveData.onyxValue >= GetLevelUpCost())
            {
                saveData.onyxValue -= GetLevelUpCost();
                saveData.playerLevel += 1;

                SaveAsFile();

                return true;
            }
            else
                return false;
        }

        public int GetLevelUpCost()
        {
            int index = saveData.playerLevel - 1;
            if (IsNotMaxLevel())
                return levelUpCost[index];
            else
                return int.MaxValue;
        }

        public bool IsNotMaxLevel()
        {
            int index = saveData.playerLevel - 1;
            return index < levelUpCost.Count;
        }

        public bool UnlockBioroid(int bioroidId)
        {
            List<BioroidInformation> bioroids = new List<BioroidInformation>(bioroidInformations.Bioroids);
            int foundedIndex = bioroids.FindIndex((item) => item.Id == bioroidId);
            if(foundedIndex == -1)
                return false;

            if (saveData.onyxValue < bioroids[foundedIndex].UnlockCost)
                return false;

            if (saveData.playerLevel < bioroids[foundedIndex].UnlockLevel)
                return false;

            if (saveData.owningBioroidsIds.Contains(bioroidId))
                return false;

            saveData.onyxValue -= bioroids[foundedIndex].UnlockCost;
            saveData.owningBioroidsIds.Add(bioroidId);

            SaveAsFile();
            return true;
        }

        public int GetBioroidUnlockCost(int bioroidId)
        {
            List<BioroidInformation> bioroids = new List<BioroidInformation>(bioroidInformations.Bioroids);
            int foundedIndex = bioroids.FindIndex((item) => item.Id == bioroidId);
            if (foundedIndex == -1)
                return -1;
            else
                return bioroids[foundedIndex].UnlockCost;
        }

        public void SetAideBioroidId(int aideBioroidId)
        {
            saveData.aideBioroidId = aideBioroidId;

            SaveAsFile();
        }

        public void SaveAsFile()
        {
            if (doNotSaveForTest)
                return;

            string fullPath = Path.Combine(Application.persistentDataPath, "OnyxSaveData0.dat");
            string jsonData = JsonUtility.ToJson(saveData);

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
                
                saveData = JsonUtility.FromJson<SaveData>(jsonData);

                return true;
            }
            catch (Exception e)
            {
                Debug.LogError("Save data reading Failed. Path: " + fullPath + ", Exception: " + e);

                return false;
            }
        }

        internal void PrepareVersionCheck()
        {
            saveData.saveDataVersion = 0;
        }

        public void UpdateSaveData0To1(SaveDataAsset defaultSaveData)
        {
            if(saveData.saveDataVersion == 0)
            {
                bioroidInformations = defaultSaveData.bioroidInformations;
                saveData.owningBioroidsIds.Clear();
                saveData.owningBioroidsIds.AddRange(defaultSaveData.saveData.owningBioroidsIds);

                saveData.aideBioroidId = defaultSaveData.saveData.aideBioroidId;

                saveData.saveDataVersion = 1;
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

        [System.Serializable]
        private struct SaveData
        {
            public int saveDataVersion;
            public List<StageInfo> stageClearedList;
            public Vector2 greetingPosition;
            public float greetingSize;
            public int onyxValue;
            public int playerLevel;
            public List<int> owningBioroidsIds;
            public int aideBioroidId;

            public static SaveData Default = new SaveData
            {
                saveDataVersion = 1,
                stageClearedList = new List<StageInfo>(),
                greetingPosition = new Vector2(-2.5f, 1.1f),
                greetingSize = 2.4f,
                onyxValue = 0,
                playerLevel = 1,
                owningBioroidsIds = new List<int> { 25 },
                aideBioroidId = 25,
            };
        }
    }
}