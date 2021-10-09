using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Onyx.Communication;
using Onyx.Communication.Protocol;
using System.Collections.ObjectModel;

public class OnyxGameInstance : MonoBehaviour
{
    public static OnyxGameInstance instance;

    [SerializeField] private AudioSource bgmAudioSource;
    [SerializeField] private StageInformation stageInfoForStageScene;
    [SerializeField] private ChapterInformation chapterInfoForStageScene;
    [SerializeField] private BioroidInformation bioroidInfoForStageScene;
    [SerializeField] private ChapterInformation chapterInfoForSignIn;

    [Header("SaveData")]
    [SerializeField] private bool doNotSaveOrLoadForTest;
    [SerializeField] private SaveDataAsset saveDataAsset;
    [SerializeField] private SaveDataAsset defaultSaveDataAsset;
    [SerializeField] private Vector2 greetingCameraPosition;
    [SerializeField] private float greetingCameraSize = 5;
    [SerializeField] private Vector2 greetingCameraSizeMinMax = new Vector2(1, 5);
    [SerializeField] private int onyxValue;
    [SerializeField] private int playerLevel;
    [SerializeField] private List<int> owningBioroidsIds;
    [SerializeField] private int aideBioroidId;

    private OnyxClient onyxClient;
    private bool isSignInSuccess;

    public bool isFromTitleScene;
    public bool isFromBatteMapScene;
    public bool isFromBatteScene;

    public AudioSource BgmAudioSource => bgmAudioSource;
    public StageInformation StageInfoForStageScene => stageInfoForStageScene;
    public ChapterInformation ChapterInfoForStageScene => chapterInfoForStageScene;
    public BioroidInformation BioroidInfoForStageScene => bioroidInfoForStageScene;
    public Vector2 GreetingCameraPosition => greetingCameraPosition;
    public float GreetingCameraSize => greetingCameraSize;
    public OnyxClient OnyxClient => onyxClient;
    public Vector2 GreetingCameraSizeMinMax => greetingCameraSizeMinMax;
    public int OnyxValue => onyxValue;
    public int PlayerLevel => playerLevel;
    public ReadOnlyCollection<int> OwningBioroidsIds => owningBioroidsIds.AsReadOnly();
    public int AideBioroidId => aideBioroidId;

    public bool IsSignInSuccess => isSignInSuccess;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        if (instance == null)
        {
            instance = this;
            StageInformation.isStageClearedCommunication = IsStageClearedCommunication;
            StageInformation.setStageClearedCommunication = SetStageClearedCommunication;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            if( bgmAudioSource != null )
                Destroy(bgmAudioSource.gameObject);
        }
    }


    public void ResetSaveData()
    {
        defaultSaveDataAsset.SaveAsFile();
    }

    public void SignIn(string key, Action afterSignIn)
    {
        if (doNotSaveOrLoadForTest)
        {
            JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(defaultSaveDataAsset), saveDataAsset);
            saveDataAsset.SetDoNotSaveForTest(true);
        }
        else
        {
            saveDataAsset.PrepareVersionCheck();
            if (saveDataAsset.LoadFromFile())
            {
                saveDataAsset.UpdateSaveData0To1(defaultSaveDataAsset);
            }
            else
            {
                defaultSaveDataAsset.SaveAsFile();

                if (!saveDataAsset.LoadFromFile())
                {
                    Debug.Log("SignIn Failed");
                    return;
                }
            }

            saveDataAsset.SetDoNotSaveForTest(false);
        }


        CacheUid(key, AfterCacheUid);

        void AfterCacheUid()
        {
            StartCoroutine(ReadAllStageClearedFromServer(chapterInfoForSignIn, OnSuccessReadAllStageCleared, OnFailedReadAllStageCleared));
        }

        void OnSuccessReadAllStageCleared()
        {
            onyxClient.LoadGreeting(new LoadGreetingRequest(), OnSuccessLoadGreeting);
        }

        void OnSuccessLoadGreeting(Vector2 position, float size)
        {
            greetingCameraPosition = position;
            greetingCameraSize = Mathf.Clamp(size, greetingCameraSizeMinMax.x, greetingCameraSizeMinMax.y);

            onyxClient.GetOnyxValue(new GetOnyxValueRequest(), OnSuccessGetOnyxValue);
        }

        void OnSuccessGetOnyxValue(int onyxValue)
        {
            this.onyxValue = onyxValue;

            onyxClient.GetPlayerLevel(new GetPlayerLevelRequest(), OnSuccessGetPlayerLevel);
        }

        void OnSuccessGetPlayerLevel(int playerLevel)
        {
            this.playerLevel = playerLevel;

            onyxClient.GetOwningBioroidsIds(new GetOwningBioroidsIdsRequest(), OnSuccessGetOwningBioroidsIds);
        }

        void OnSuccessGetOwningBioroidsIds(List<int> owningBioroidsIds)
        {
            this.owningBioroidsIds.Clear();
            this.owningBioroidsIds.AddRange(owningBioroidsIds);

            onyxClient.GetAideBioroidId(new GetAideBioroidIdRequest(), OnSuccessGetAideBioroidId);
        }

        void OnSuccessGetAideBioroidId(int aideBioroidId)
        {
            this.aideBioroidId = aideBioroidId;
            OnFinish();
        }

        void OnFinish()
        {
            isSignInSuccess = true;
            afterSignIn();
        }

        void OnFailedReadAllStageCleared()
        {
            Debug.Log("Reading all stage cleared failed");
        }

    }

    public void SetStageInfoForStageScene(StageInformation stageInfo)
    {
        stageInfoForStageScene = stageInfo;
    }
    public void SetChapterInfoForStageScene(ChapterInformation chapterInfo)
    {
        chapterInfoForStageScene = chapterInfo;
    }
    public void SetBioroidInfoForStageScene(BioroidInformation bioroidInfo)
    {
        bioroidInfoForStageScene = bioroidInfo;
    }

    public void SaveGreetingCameraInfo(Vector2 position, float size, Action afterSave)
    {
        greetingCameraPosition = position;
        greetingCameraSize = size;

        OnyxClient.SaveGreeting(new SaveGreetingRequest(position, size), OnSuccess);
        void OnSuccess(bool isSuccess)
        {
            afterSave();
            return;
        }
    }

    public void SetStageClearedCommunication(StageInformation stageInformation, Action callback)
    {
        int stageTypeToInteger = stageInformation.StageType switch
        {
            StageInformation.StageTypeEnum.Normal => 0,
            StageInformation.StageTypeEnum.Side => 1,
            StageInformation.StageTypeEnum.Ex => 2,
            _ => throw new NotImplementedException(),
        };

        SetStageClearedRequest request = new SetStageClearedRequest(stageInformation.ChapterNumber, stageInformation.StageNumber, stageTypeToInteger);
        OnyxClient.SetIsStageCleared(request, AfterSave);

        void AfterSave(bool isSuccess)
        {
            callback();
        }
    }

    public void AddOnyx(int onyxValue, Action callBack)
    {
        AddOnyxValueRequest request = new AddOnyxValueRequest(onyxValue);
        OnyxClient.AddOnyxValue(request, AfterAdd);

        void AfterAdd(int onyxValue)
        {
            this.onyxValue = onyxValue;
            callBack();
        }
    }

    public void LevelUpPlayer(Action callBack)
    {
        LevelUpPlayerRequest request = new LevelUpPlayerRequest();
        OnyxClient.LevelUpPlayer(request, AfterLevelUp);

        void AfterLevelUp(int playerLevel, int onyxValue)
        {
            this.playerLevel = playerLevel;
            this.onyxValue = onyxValue;
            callBack();
        }
    }

    private void CacheUid(string key, Action afterCache)
    {
        onyxClient = new OnyxClient(new AssetReadCommunication(saveDataAsset), this, new CommunicationRetryConfirm(), new ApplicationQuitAlert());
        onyxClient.CacheUid(new UidRequest(key), OnLoginSuccess);

        void OnLoginSuccess(int uid)
        {
            Debug.Log("Sign in " + uid);
            afterCache();
        }
    }

    private void IsStageClearedCommunication(StageInformation stageInformation, Action<bool> callback)
    {
        int stageTypeToInteger = stageInformation.StageType switch
        {
            StageInformation.StageTypeEnum.Normal => 0,
            StageInformation.StageTypeEnum.Side => 1,
            StageInformation.StageTypeEnum.Ex => 2,
            _ => throw new NotImplementedException(),
        };
        IsStageClearedRequest request = new IsStageClearedRequest(stageInformation.ChapterNumber, stageInformation.StageNumber, stageTypeToInteger);
        OnyxClient.GetIsStageCleared(request, callback);
    }

    private IEnumerator ReadAllStageClearedFromServer(ChapterInformation firstChapter, Action onSuccess, Action onFailed)
    {
        List<ChapterInformation> chapters = ParseChapters(firstChapter);

        List<StageInformation> stages = new List<StageInformation>();
        foreach (ChapterInformation chapter in chapters)
            stages.AddRange(ParseStageInformations(chapter.FirstStage));

        List<bool> isFinishedList = new List<bool>();

        for (int i = 0; i < stages.Count; i++)
        {
            isFinishedList.Add(false);
            int capturedIndex = i;
            stages[i].ReadIsClearedFromServer(() => isFinishedList[capturedIndex] = true);
        }

        yield return new WaitUntilOrForSeconds(() => isFinishedList.TrueForAll(item => item), 5f);

        if (isFinishedList.TrueForAll(item => item))
            onSuccess();
        else
            onFailed();
    }

    private List<StageInformation> ParseStageInformations(StageInformation stageInformation)
    {
        List<StageInformation> stageInformations = new List<StageInformation>
        {
            stageInformation
        };

        if (stageInformation.NextStages.nextNormalStage != null)
            stageInformations.AddRange(ParseStageInformations(stageInformation.NextStages.nextNormalStage));
        if (stageInformation.NextStages.nextSideStage != null)
            stageInformations.AddRange(ParseStageInformations(stageInformation.NextStages.nextSideStage));
        if (stageInformation.NextStages.nextExStage != null)
            stageInformations.AddRange(ParseStageInformations(stageInformation.NextStages.nextExStage));

        return stageInformations;
    }

    private List<ChapterInformation> ParseChapters(ChapterInformation chapterInformation)
    {
        List<ChapterInformation> chapterInformations = new List<ChapterInformation>();

        ChapterInformation current = chapterInformation;
        while (current != null)
        {
            chapterInformations.Add(current);
            current = current.NextChapter;
        }

        return chapterInformations;
    }

    public void UnlockBioroid(int bioroidId, Action callBack)
    {
        UnlockCombatantEmbargoRequest request = new UnlockCombatantEmbargoRequest(bioroidId);
        OnyxClient.UnlockCombatantEmbargo(request, AfterLevelUp);

        void AfterLevelUp(bool isSucceed, int onyxValue)
        {
            if (isSucceed)
                owningBioroidsIds.Add(bioroidId);
            this.onyxValue = onyxValue;
            callBack();
        }
    }

    public void SetAideBioroidId(int aideBioroidId, Action callBack)
    {
        SetAideBioroidIdRequest request = new SetAideBioroidIdRequest(aideBioroidId);
        OnyxClient.SetAideBioroidId(request, AfterSetAideBioroidId);

        void AfterSetAideBioroidId(bool isSucceed)
        {
            this.aideBioroidId = aideBioroidId;
            callBack();
        }
    }

    public class CommunicationRetryConfirm : OnyxClient.IRetryConfirm
    {
        readonly bool result = false;
        bool OnyxClient.IRetryConfirm.Result => result;

        IEnumerator OnyxClient.IRetryConfirm.Confirm()
        {
            Debug.Log("Retry!");
            yield return new WaitForSeconds(0.1f);
        }
    }

    public class ApplicationQuitAlert : OnyxClient.IQuitAlert
    {
        IEnumerator OnyxClient.IQuitAlert.Alert()
        {
            Debug.Log("Quit!");
            yield return new WaitForSeconds(1f);
        }
    }
}
