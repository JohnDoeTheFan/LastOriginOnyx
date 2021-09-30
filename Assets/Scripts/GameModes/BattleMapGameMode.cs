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
    [SerializeField] private StageSceneTransitionGui stageSceneTransitionGui;
    [SerializeField] private FadeGui fadeGui;
    [SerializeField] private AudioClip bgm;
    [SerializeField] private TextMeshProUGUI onyxResourceGui;
    [Header("Chapter Information Gui")]
    [SerializeField] private Text chapterNum;
    [SerializeField] private Text chapterTitle;
    [Header("Stage Information Gui")]
    [SerializeField] private Text stageNum;
    [SerializeField] private Text stageTitle;
    [SerializeField] private Text stageDescription;
    [SerializeField] private Text stageNeededPower;
    [Header("Chapter And Stage Parsing")]
    [SerializeField] private ThreeStateToggleGroup<ChapterInformation> chapterGroup;
    [SerializeField] private ThreeStateToggleGroup<StageInformation> stageGroup;
    [SerializeField] private ChapterInformation firstChapter;
    [Header("Combatant Selection")]
    [SerializeField] private Canvas combatantSelectionCanvas;
    [SerializeField] private ToggleGroup combatantToggleGroup;
    [SerializeField] private CombatantToggle combatantTogglePrefab;
    [SerializeField] private List<BioroidInformation> combatantList;
    [SerializeField] private Image combatantPortrait;
    [SerializeField] private Text combatantName;
    [SerializeField] private Text combatantDescription;
    [SerializeField] private ToggleGroup skillToggleGroup;
    [SerializeField] private List<Toggle> skillToggles;
    [SerializeField] private List<Image> skillImages;
    [SerializeField] private Text skillName;
    [SerializeField] private Text skillDescription;

    private StageInformation stageInfo;
    private ChapterInformation chapterInfo;
    private BioroidInformation bioroidInfo;

    private const int numberOfSkillsPerAbility = 2;

    private void Awake()
    {
        chapterGroup.SubscribeManager.Subscribe(new ChapterGroupSubscriber(OnChangeChapterSelection));
        stageGroup.SubscribeManager.Subscribe(new StageGroupSubscriber(OnChangeStageSelection));

        CombatantToggle.OnSelected = (bioroid) =>
        {
            bioroidInfo = bioroid;

            combatantPortrait.sprite = bioroidInfo.Portrait;
            combatantName.text = bioroidInfo.BioroidName;
            combatantDescription.text = bioroidInfo.Description;

            foreach (Toggle skillToggle in skillToggles)
                skillToggle.interactable = false;
            foreach (Image skillImage in skillImages)
                skillImage.gameObject.SetActive(false);

            var abilities = bioroidInfo.Abilities;

            for(int i = 0; i < abilities.Count; i++)
            {
                for(int j = 0; j < abilities[i].skills.Count; j++)
                {
                    int index = i * numberOfSkillsPerAbility + j;
                    if (index < skillToggles.Count)
                    {
                        skillToggles[index].interactable = true;
                        skillImages[index].gameObject.SetActive(true);
                        skillImages[index].sprite = abilities[i].skills[j].image;
                    }
                }
            }
            skillToggleGroup.allowSwitchOff = false;
            skillToggles[0].isOn = true;
        };
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
                stageSceneTransitionGui.StartDissolveIn(false, ()=> InitCombatantList());
            }
            else
                fadeGui.StartFadeIn(AfterFadeIn);
        }

        void AfterFadeIn(FadeGui.FinishStatus finishStatus)
        {
            InitSkillList();
            InitCombatantList();
        }

    }

    private void Update()
    {
        if(Input.GetButtonDown("Cancel"))
        {
            BackToLobby();
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

    public void DisplayCombatantSelection(bool display)
    {
        combatantSelectionCanvas.gameObject.SetActive(display);
    }

    public void InitCombatantList()
    {
        CombatantToggle firstToggle = null;
        foreach (BioroidInformation combatant in combatantList)
        {
            if (OnyxGameInstance.instance.OwningBioroidsIds.Contains(combatant.Id))
            {
                CombatantToggle newToggle = Instantiate<CombatantToggle>(combatantTogglePrefab, combatantToggleGroup.transform);
                newToggle.SetBioroidInformation(combatant);
                newToggle.Toggle.group = combatantToggleGroup;

                if (firstToggle == null)
                    firstToggle = newToggle;
            }
        }
        firstToggle.Toggle.isOn = true;
    }

    public void InitSkillList()
    {
        for (int i = 0; i < skillToggles.Count; i++)
        {
            int abilityIndex = i / numberOfSkillsPerAbility;
            int skillIndex = i % numberOfSkillsPerAbility;

            Action<bool> OnValueChanged = (isOn) =>
            {
                if (isOn && bioroidInfo != null)
                {
                    ReadOnlyCollection<BioroidInformation.AbilityDescription> abilities = bioroidInfo.Abilities;
                    if (abilityIndex < abilities.Count && skillIndex < abilities[abilityIndex].skills.Count)
                    {
                        BioroidInformation.AbilityDescription.SkillDescription skill = abilities[abilityIndex].skills[skillIndex];
                        skillName.text = skill.skillName;
                        skillDescription.text = skill.description;
                    }
                }
            };

            skillToggles[i].onValueChanged.AddListener(new UnityEngine.Events.UnityAction<bool>(OnValueChanged));
        }
    }

    public void EnterSelectedStage()
    {
        stageSceneTransitionGui.StartDissolveOut(true, AfterSceneTransition);

        void AfterSceneTransition()
        {
            OnyxGameInstance.instance.SetStageInfoForStageScene(stageInfo);
            OnyxGameInstance.instance.SetChapterInfoForStageScene(chapterInfo);
            OnyxGameInstance.instance.SetBioroidInfoForStageScene(bioroidInfo);
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
