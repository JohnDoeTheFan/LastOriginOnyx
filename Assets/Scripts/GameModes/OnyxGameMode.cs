using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Onyx;
using Onyx.BattleRoom;
using Onyx.Ability;
using Onyx.Input;
using Onyx.GameElement;
using Onyx.Core;

public class OnyxGameMode : RunAndGunGameMode
{
    [Header("Onyx/Gui")]
    [SerializeField]
    private Text chapterNameText;
    [SerializeField]
    private Text stageNameText;
    [SerializeField]
    private SequentialActivator missionStartSequence;
    [SerializeField]
    private SequentialActivator battleStartSequence;
    [SerializeField]
    private RectTransform missionCompleteGui;
    [SerializeField]
    private BattleProgressGui battleProgressGui;
    [SerializeField]
    private Canvas mainGuiCanvas;
    [SerializeField]
    private RectTransform commandPanelCanvas;
    [SerializeField]
    private CommandPanel commandPanel;
    [SerializeField]
    private VirtualJoyStick virtualJoyStick;
    [SerializeField]
    private RectTransform abilitySpecificGuiArea;
    [SerializeField]
    private StageSceneTransitionGui stageSceneTransitionGui;
    [SerializeField]
    private RectTransform damageEffectGui;
    [SerializeField]
    private RectTransform onyxOrbCanvas;
    [SerializeField]
    private OnyxOrbGui onyxOrbGuiPrefab;

    [Header("Onyx/Main")]
    [SerializeField]
    private GameObject backgroundHolder;
    [SerializeField]
    private Follower followerWithCamera;
    [SerializeField]
    private BattleRoom startRoom;
    [SerializeField]
    private BattleRoom endRoomPrefab;
    [SerializeField]
    private BattleRoom testRoom;
    [SerializeField]
    private AudioClip bgm;
    [SerializeField]
    private AudioSource voiceAudioSource;

    private bool isLeftButtonDown;
    private bool isRightButtonDown;
    private float orthographicSizeBackup;
    private ReadOnlyCollection<IAbility> playerAbility;
    private readonly UnsubscriberPack battleRoomSubscribers = new UnsubscriberPack();
    private MyUnit playerUnitCache;
    private Vector2 lastScreenSize;

    // TODO: Reworks control hierarchy
    // GameControl  CharacterControl
    //   false          whatever    : User can't do anything.
    //   true           false       : User can interact guis(Except character control guis.).
    //   true           true        : User can interact every things.
    private bool systemControl = true;
    readonly private bool userControl = true;

    private void Awake()
    {
        InitInputHandler();
        InitUnit();
        InitTriggerVolume();
        InitGunSlinger();
        InitBattleRoom();
        InitInputSource();
        InitWorldCanvas();
    }

    protected override void Start()
    {
        InstantiateBattleRooms();

        orthographicSizeBackup = mainCamera.orthographicSize;

        CameraSettingForBattleRoom(startRoom);

        mainGuiCanvas.gameObject.SetActive(false);
        playerControlEnable = false;
        OnyxGameInstance.instance.BgmAudioSource.clip = bgm;
        OnyxGameInstance.instance.BgmAudioSource.Play();

        stageSceneTransitionGui.StartDissolveIn(true, AfterSceneTransition);

        void AfterSceneTransition()
        {
            if (OnyxGameInstance.instance.ChapterInfoForStageScene != null)
                chapterNameText.text = OnyxGameInstance.instance.ChapterInfoForStageScene.ChapterName;
            else
                chapterNameText.text = "TestChapter";
            if (OnyxGameInstance.instance.StageInfoForStageScene != null)
                stageNameText.text = OnyxGameInstance.instance.StageInfoForStageScene.Title;
            else
                stageNameText.text = "TestStage";
            missionStartSequence.PlaySequences(AfterMissionStartSequence);
        }

        void AfterMissionStartSequence()
        {
            Destroy(missionStartSequence.gameObject);
            mainGuiCanvas.gameObject.SetActive(true);
            commandPanel.Unlock(playerAbility);
            playerControlEnable = true;
            voiceAudioSource.clip = playerUnitCache.StageStartVoice;
            voiceAudioSource.Play();
        }
    }

    private void Update()
    {
        if (isGameOvered)
        {
            if (Input.anyKeyDown)
            {
                BackToStageMapScene();
            }
        }
        if(systemControl)
        {
            if (Input.GetButtonDown("Cancel"))
            {
                if (Time.timeScale == 0f)
                    ResumeGame();
                else
                    PauseGame();
            }
        }

        // TODO: Check screen size change.
    }

    public void OnDestroy()
    {
        battleRoomSubscribers.UnsubscribeAll();
    }

    public void InitBattleRoom()
    {
        BattleRoom.OnStart = battleRoom =>
        {
            battleRoomSubscribers.Add(new BattleRoomSubscriber(battleRoom, OnEnterBattleRoom, OnExitBattleRoom));
        };
    }

    private void InstantiateBattleRooms()
    {
        BattleRoom lastRoom = startRoom;

        if(testRoom != null)
        {
            lastRoom.AttachRoom(testRoom);

            lastRoom = testRoom;
        }
        else if(OnyxGameInstance.instance.StageInfoForStageScene != null)
        {
            ReadOnlyCollection<BattleRoom> battleRoomPrefabs = OnyxGameInstance.instance.StageInfoForStageScene.BattleRoomPrefabs;

            foreach (BattleRoom battleRoomPrefab in battleRoomPrefabs)
            {
                BattleRoom newBattleRoom = Instantiate<BattleRoom>(battleRoomPrefab);

                Vector2 newRoomPosition = CalcNewBattleRoomPosition(lastRoom, newBattleRoom);
                newBattleRoom.transform.Translate(newRoomPosition);

                lastRoom.AttachRoom(newBattleRoom);

                lastRoom = newBattleRoom;
            }
        }

        Vector2 lastRoomPosition = CalcNewBattleRoomPosition(lastRoom, endRoomPrefab);
        BattleRoom endRoom = Instantiate<BattleRoom>(endRoomPrefab, lastRoomPosition, Quaternion.identity);
        lastRoom.AttachRoom(endRoom);

        static Vector2 CalcNewBattleRoomPosition(BattleRoom lastRoom, BattleRoom newRoom)
        {
            Vector2 lastRoomTileMapSize = lastRoom.CalcTileMapSize();
            Vector2 newRoomTileMapSize = newRoom.CalcTileMapSize();

            Vector2 newRoomPosition = new Vector2
            {
                x = lastRoom.transform.position.x + (lastRoomTileMapSize.x / 2f) + (newRoomTileMapSize.x / 2f),
                y = 0
            };

            return newRoomPosition;
        }
    }

    protected override void InitTriggerVolume()
    {
        TriggerVolume.OnEnterTriggerVolume = (type, gameObject) =>
        {
            MyUnit myUnit = gameObject.GetComponent<MyUnit>();
            if (myUnit != null)
            {
                switch (type)
                {
                    case TriggerVolume.Type.Death:
                        myUnit.Die();
                        break;
                    case TriggerVolume.Type.Win:
                        if (myUnit.CompareTag("Player"))
                        {
                            OnStageCleared(myUnit);
                        }
                        break;
                    default:
                        break;
                }
            }
        };
    }

    protected override void InitUnit()
    {
        MyUnit.OnEndOfStart = myUnit =>
        {
            if (myUnit.CompareTag("Player"))
            {
                playerUnitCache = myUnit;
                playerAbility = myUnit.Abilities;

                myUnit.SubscribeManager.Subscribe(new PlayerUnitSubscriber(OnDeathPlayerUnit, OnPlayerUnitDamageOrHeal, playerDurabilityGui));

                foreach (IAbility ability in myUnit.Abilities)
                    ability.InstantiateAbilitySpecificGui(abilitySpecificGuiArea);

                myUnit.SetLevel(OnyxGameInstance.instance.PlayerLevel, new List<IAbility.LevelDescription>());
            }
            else
            {
                myUnit.SubscribeManager.Subscribe(new UnitSubscriber(OnDeathEnemyUnit, OnUnitDamageOrHeal));
                myUnit.SetLevelOfDifficulty(OnyxGameInstance.instance.StageInfoForStageScene.NeededPower);
            }
                
        };
    }

    private void InitInputSource()
    {
        if (virtualJoyStick == null)
        {
            InputSource.GetLeftStickHorizontal = CalcValue;


            float CalcValue()
            {
                if (isLeftButtonDown && isRightButtonDown)
                    return 0;
                else if (isLeftButtonDown)
                    return -1;
                else if (isRightButtonDown)
                    return 1;
                else
                    return 0;
            }
        }
        else
        {
            InputSource.GetLeftStickHorizontal = ()=>virtualJoyStick.Value.x;
        }
    }
    private void InitWorldCanvas()
    {
        WorldSpaceCanvasCameraSetter.GetCamera = () => mainCamera;
    }

    private void OnStageCleared(MyUnit playerUnit)
    {
        playerUnit.Ceremony();
        voiceAudioSource.clip = playerUnit.StageClearVoice;
        voiceAudioSource.Play();

        missionCompleteGui.gameObject.SetActive(true);
        mainGuiCanvas.gameObject.SetActive(false);
        commandPanel.Lock();
        playerControlEnable = false;
        systemControl = false;

        float safefyTime = 2f;
        if (testRoom == null && OnyxGameInstance.instance.StageInfoForStageScene != null)
            SaveStageFirstCleared(safefyTime);
        else
            SaveOnyxValue(ReadyForSceneTransition, safefyTime);
    }

    void SaveStageFirstCleared(float safetyTime)
    {
        bool isClearInfoSaveFinished = false;
        bool isOnyxValueSaveFinished = false;
        OnyxGameInstance.instance.StageInfoForStageScene.WriteIsClearedToServer(() => isClearInfoSaveFinished = true);
        OnyxGameInstance.instance.AddOnyx(Score, () => isOnyxValueSaveFinished = true);

        StartCoroutine(Job(() => WaitForSecondsRoutine(safetyTime), AfterSafetyTime));
        void AfterSafetyTime()
        {
            StartCoroutine(Job(() => WaitUntilRoutine(() => isClearInfoSaveFinished && isOnyxValueSaveFinished), ReadyForSceneTransition));
        }
    }

    void SaveOnyxValue(Action afterSave, float safetyTime = 0f)
    {
        bool isSaveFinished = false;
        OnyxGameInstance.instance.AddOnyx(Score, () => isSaveFinished = true);

        StartCoroutine(Job(() => WaitForSecondsRoutine(safetyTime), AfterSafetyTime));
        void AfterSafetyTime()
        {
            StartCoroutine(Job(() => WaitUntilRoutine(() => isSaveFinished), afterSave));
        }
    }

    void ReadyForSceneTransition()
    {
        systemControl = true;
        isGameOvered = true;
    }

    protected virtual void OnPlayerUnitDamageOrHeal(MyUnit unit, float value)
    {
        OnUnitDamageOrHeal(unit, value);

        if (value < 0)
        {
            damageEffectGui.gameObject.SetActive(false);
            damageEffectGui.gameObject.SetActive(true);
        }
    }

    protected override void OnDeathPlayerUnit(MyUnit myUnit)
    {
        voiceAudioSource.clip = myUnit.RetireVoice;
        voiceAudioSource.Play();

        RectTransform gameOverGui = Instantiate<RectTransform>(GameOverGuiPrefab, screenCanvas.transform);
        commandPanel.Lock();
        playerControlEnable = false;
        systemControl = false;

        SaveOnyxValue(ReadyForSceneTransition, 2f);
    }
    protected override void OnDeathEnemyUnit(MyUnit myUnit)
    {
        StartCoroutine(Job(() => WaitForSecondsRoutine(1.5f), () => Destroy(myUnit.gameObject)));

        for(int i = 0; i < myUnit.ScoreMultiplier; i++)
        {
            OnyxOrbGui orbGui = Instantiate<OnyxOrbGui>(onyxOrbGuiPrefab, onyxOrbCanvas);
            Vector3 unitViewportPosition = mainCamera.WorldToViewportPoint(myUnit.transform.position);
            Vector3 unitCanvasPosition = unitViewportPosition * onyxOrbCanvas.sizeDelta;

            Vector2 scoreAnchor = onyxOrbCanvas.sizeDelta * scoreGui.rectTransform.anchorMin;

            orbGui.Initialize(unitCanvasPosition, scoreAnchor + scoreGui.rectTransform.anchoredPosition, () => Score += 10);
        }

    }

    public void SaveAndBackToStageMapScene()
    {
        voiceAudioSource.clip = playerUnitCache.RetreatVoice;
        voiceAudioSource.Play();

        commandPanel.Lock();
        playerControlEnable = false;
        systemControl = false;

        SaveOnyxValue(BackToStageMapScene, 1f);
    }

    public void BackToStageMapScene()
    {
        stageSceneTransitionGui.StartDissolveOut(false, AfterSceneTransition);

        static void AfterSceneTransition()
        {
            Time.timeScale = 1;
            OnyxGameInstance.instance.isFromBatteScene = true;
            SceneManager.LoadScene((int)ScenesToBeBuild.StageMapScene);
        }
    }

    void OnEnterBattleRoom(BattleRoom battleRoom, bool shouldPlayBriefing)
    {
        CameraSettingForBattleRoom(battleRoom);
        battleProgressGui.SetBattleRoom(battleRoom);

        fadeGui.StartFadeIn(AfterFadeIn);

        void AfterFadeIn(FadeGui.FinishStatus status)
        {
            if (shouldPlayBriefing)
                battleStartSequence.PlaySequences(AfterBattleAnimator);
            else
                playerControlEnable = true;
        }

        void AfterBattleAnimator()
        {
            playerControlEnable = true;
        }
    }

    void CameraSettingForBattleRoom(BattleRoom battleRoom)
    {
        float ratioOfCommandPanel = commandPanel.GetSize().y / commandPanelCanvas.rect.height;

        Vector2 tileMapSize = battleRoom.CalcTileMapSize();
        Vector2 shrinkSize = new Vector2(tileMapSize.x, tileMapSize.y + tileMapSize.y * ratioOfCommandPanel);
        float newOrthographicSize = CalcShrinkedOrthographicSize(orthographicSizeBackup, shrinkSize);
        mainCamera.orthographicSize = newOrthographicSize;

        float mainCameraHeight = mainCamera.orthographicSize * 2;
        float commandPanelHeight = mainCameraHeight * ratioOfCommandPanel;
        followerWithCamera.SetMargin(new Vector2(0, -1 * commandPanelHeight / 2));

        MinMax2 tileMapBound = battleRoom.CalcTileMapBound();
        followerWithCamera.limit = CalcCameraFollowerLimit(tileMapBound, newOrthographicSize, mainCamera.aspect, followerWithCamera.Margin);
        followerWithCamera.SetShouldLimit(true);

        for (int i = 0; i < backgroundHolder.transform.childCount; i++)
            Destroy(backgroundHolder.transform.GetChild(i).gameObject);
        Instantiate(battleRoom.Background, backgroundHolder.transform);
    }

    float CalcShrinkedOrthographicSize(float orthographicSize, Vector2 limitSize)
    {
        if (limitSize.y < orthographicSize * 2)
            orthographicSize = limitSize.y / 2;
        if (limitSize.x < mainCamera.aspect * orthographicSize * 2)
            orthographicSize = limitSize.x / mainCamera.aspect / 2;

        return orthographicSize;
    }

    MinMax2 CalcCameraFollowerLimit(MinMax2 bounds, float orthographicSize, float cameraAspect, Vector2 cameraMargin)
    {
        MinMax2 output = new MinMax2();
        output.min.x = bounds.min.x + orthographicSize * cameraAspect;
        output.min.y = bounds.min.y + orthographicSize;
        output.max.x = bounds.max.x - orthographicSize * cameraAspect;
        output.max.y = bounds.max.y - orthographicSize;

        if (cameraMargin.y < 0)
            output.min.y += cameraMargin.y * 2;
        else if (cameraMargin.y > 0)
            output.max.y += cameraMargin.y;

        return output;
    }

    void OnExitBattleRoom(BattleRoomPortal exitingPortal, GameObject user)
    {
        if (exitingPortal.DestinationPortal != null)
        {
            playerControlEnable = false;
            fadeGui.StartFadeOut(AfterFadeOut);
        }

        void AfterFadeOut(FadeGui.FinishStatus status)
        {
            user.transform.position = exitingPortal.DestinationPortal.transform.position;
            exitingPortal.DestinationPortal.Enter();
        }
    }

    public void OnLeftButtonDown()
    {
        isLeftButtonDown = true;
    }

    public void OnLeftButtonUp()
    {
        isLeftButtonDown = false;
    }

    public void OnRightButtonDown()
    {
        isRightButtonDown = true;
    }
    public void OnRightButtonUp()
    {
        isRightButtonDown = false;
    }

    class BattleRoomSubscriber : UniUnsubscriber, BattleRoom.ISubscriber
    {
        readonly Action<BattleRoom, bool> OnEnter;
        readonly Action<BattleRoomPortal, GameObject> OnExit;

        public BattleRoomSubscriber(BattleRoom battleRoom, Action<BattleRoom, bool> OnEnter, Action<BattleRoomPortal, GameObject> OnExit)
        {
            InitUniSubscriber(battleRoom.SubscribeManager.Subscribe(this));

            this.OnEnter = OnEnter;
            this.OnExit = OnExit;
        }

        void BattleRoom.ISubscriber.OnUpdateBattleMission(BattleRoom battleRoom)
        {
        }

        void BattleRoom.ISubscriber.OnEnter(BattleRoom battleRoom, bool shouldPlayBriefing)
        {
            OnEnter(battleRoom, shouldPlayBriefing);
        }

        void BattleRoom.ISubscriber.OnExit(BattleRoomPortal exitingPortal, GameObject user)
        {
            OnExit(exitingPortal, user);
        }

        void BattleRoom.ISubscriber.OnClearBattleRoom(BattleRoom battleRoom)
        {
        }

        void BattleRoom.ISubscriber.BeforeDestroy(BattleRoomPortal battleRoomPortal)
        {
            Unsubscribe();
        }
    }


}
