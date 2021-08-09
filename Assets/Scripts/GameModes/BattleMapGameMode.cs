using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.Collections.ObjectModel;
using TMPro;
using Onyx.Core;

public class BattleMapGameMode : MonoBehaviourBase
{
    [SerializeField]
    private Text chapterNum;
    [SerializeField]
    private Text chapterTitle;
    [SerializeField]
    private Text stageNum;
    [SerializeField]
    private Text stageTitle;
    [SerializeField]
    private Text stageDescription;
    [SerializeField]
    private Text stageNeededPower;
    [SerializeField]
    private TextMeshProUGUI onyxResourceGui;
    [SerializeField]
    private StageSceneTransitionGui stageSceneTransitionGui;
    [SerializeField]
    private FadeGui fadeGui;
    [SerializeField]
    private ThreeStateToggleGroup<ChapterInformation> chapterGroup;
    [SerializeField]
    private ThreeStateToggleGroup<StageInformation> stageGroup;
    [SerializeField]
    private ChapterInformation firstChapter;
    [SerializeField]
    private AudioClip bgm;

    private StageInformation stageInfo;
    private ChapterInformation chapterInfo;

    private void Awake()
    {
        chapterGroup.SubscribeManager.Subscribe(new ChapterGroupSubscriber(OnChangeChapterSelection));
        stageGroup.SubscribeManager.Subscribe(new StageGroupSubscriber(OnChangeStageSelection));
    }

    // Login -> ReadStageCleared    -Success->  FadeIn
    //                              -Failed-->  Debug.Log(It should be replaced with retry alert or exit alert.).
    private void Start()
    {
        if (!OnyxGameInstance.instance.IsSignInSuccess)
            OnyxGameInstance.instance.SignIn("RandomCommander", AfterSignIn);
        else
            AfterSignIn();

        void AfterSignIn()
        {
            onyxResourceGui.text = OnyxGameInstance.instance.OnyxValue.ToString();
            chapterGroup.Initialize(firstChapter);

            if (OnyxGameInstance.instance.isFromBatteScene)
            {
                OnyxGameInstance.instance.BgmAudioSource.clip = bgm;
                OnyxGameInstance.instance.BgmAudioSource.Play();
                fadeGui.ForceFadeIn();
                stageSceneTransitionGui.StartDissolveIn(false, null);
            }
            else
                fadeGui.StartFadeIn(AfterFadeIn);
        }

        void AfterFadeIn(FadeGui.FinishStatus finishStatus)
        {
            return;
        }

    }

    public void BackToLobby()
    {
        fadeGui.StartFadeOut(AfterFadeOut);
        static void AfterFadeOut(FadeGui.FinishStatus finishStatus)
        {
            OnyxGameInstance.instance.isFromBatteMapScene = true;
            SceneManager.LoadScene((int)ScenesToBeBuild.Lobby);
        }
    }

    public void EnterSelectedStage()
    {
        stageSceneTransitionGui.StartDissolveOut(true, AfterSceneTransition);

        void AfterSceneTransition()
        {
            OnyxGameInstance.instance.SetStageInfoForStageScene(stageInfo);
            OnyxGameInstance.instance.SetChapterInfoForStageScene(chapterInfo);
            SceneManager.LoadScene((int)stageInfo.StageScene);
        }
    }

    private void OnChangeStageSelection(StageInformation stageInfo)
    {
        this.stageInfo = stageInfo;
        stageNum.text = String.Format("{0}-{1}", stageInfo.ChapterNumber, stageInfo.StageNumber);
        stageTitle.text = stageInfo.Title;
        stageDescription.text = stageInfo.Description;
        stageNeededPower.text = stageInfo.NeededPower.ToString();
    }

    private void OnChangeChapterSelection(ChapterInformation chapterInfo)
    {
        this.chapterInfo = chapterInfo;
        chapterNum.text = String.Format("{0}{1, 2:D2}.", "Chapter", chapterInfo.ChapterNum);
        chapterTitle.text = chapterInfo.ChapterName;
        stageGroup.Initialize(chapterInfo.FirstStage);
    }

    private class StageGroupSubscriber : ThreeStateToggleGroup<StageInformation>.ISubscriber
    {
        readonly Action<StageInformation> OnChangeSelection;
        public StageGroupSubscriber(Action<StageInformation> OnChangeSelection)
        {
            this.OnChangeSelection = OnChangeSelection;
        }

        void ThreeStateToggleGroup<StageInformation>.ISubscriber.OnChangeSelection(StageInformation stageInfo)
        {
            OnChangeSelection(stageInfo);
        }
    }
    private class ChapterGroupSubscriber : ThreeStateToggleGroup<ChapterInformation>.ISubscriber
    {
        readonly Action<ChapterInformation> OnChangeSelection;
        public ChapterGroupSubscriber(Action<ChapterInformation> OnChangeSelection)
        {
            this.OnChangeSelection = OnChangeSelection;
        }
        void ThreeStateToggleGroup<ChapterInformation>.ISubscriber.OnChangeSelection(ChapterInformation chapterInfo)
        {
            OnChangeSelection(chapterInfo);
        }
    }
}
