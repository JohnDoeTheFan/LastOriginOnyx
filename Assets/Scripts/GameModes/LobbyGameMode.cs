using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Onyx.Core;

public class LobbyGameMode : MonoBehaviourBase
{
    [SerializeField] private Camera mainCamera;

    [Header("Guis")]
    [SerializeField] private Canvas touchPreventingCanvas;
    [SerializeField] private FadeGui fadeGui;
    [SerializeField] private CanvasGroup selectWorldCanvas;
    [SerializeField] private CanvasGroup researchCanvas;
    [SerializeField] private Canvas quitCanvas;
    [SerializeField] private Canvas firstTouchCanvas;
    [SerializeField] private List<Canvas> canvasesToHideBeforeFirstTouch;
    [SerializeField] private Canvas noticeCanvas;
    [SerializeField] private Canvas greetingDecorationGui;
    [SerializeField] private TextMeshProUGUI onyxResourceGui;
    [SerializeField] private TextMeshProUGUI playerLevelGuiOnMain;

    [Header("ResearchGui")]
    [SerializeField] private Text playerCurrentLevelGuiOnResearch;
    [SerializeField] private Text playerNextLevelGuiOnResearch;
    [SerializeField] private Text playerCurrentHealthGuiOnResearch;
    [SerializeField] private Text playerNextHealthGuiOnResearch;
    [SerializeField] private Text playerCurrentAttackGuiOnResearch;
    [SerializeField] private Text playerNextAttackGuiOnResearch;
    [SerializeField] private Button levelUpButtonOnResearch;
    [SerializeField] private Text levelUpButtonTextOnResearch;
    [SerializeField] private Text currentOnyxGuiOnResearch;
    [SerializeField] private Text remainingOnyxGuiOnResearch;

    [Header("Audios")]
    [SerializeField] private AudioSource voiceAudioSource;
    [SerializeField] private AudioClip bgm;

    [Header("GreetingSettings")]
    [SerializeField] private Vector2 cameraIdlePosition;
    [SerializeField] private float cameraIdleSize;
    [SerializeField] private float greetingPositionUnit = 0.1f;
    [SerializeField] private float greetingSizeUnit = 0.1f;
    [SerializeField] private float idleRecoverTime = 1f;

    [Header("GameSettings")]
    [SerializeField] private List<int> levelUpCosts;
    [SerializeField] private MultiplierPerLevel playerStatusMultiplier;
    [SerializeField] private int playerHealthDefaultValue = 100;
    [SerializeField] private int playerAttackDefaultValue = 10;

    private void Start()
    {
        if(! OnyxGameInstance.instance.IsSignInSuccess)
            OnyxGameInstance.instance.SignIn("RandomCommander", AfterSignIn);
        else
            AfterSignIn();

        void AfterSignIn()
        {
            onyxResourceGui.text = OnyxGameInstance.instance.OnyxValue.ToString();
            playerLevelGuiOnMain.text = OnyxGameInstance.instance.PlayerLevel.ToString();
            UpdateResearchGui();

            fadeGui.StartFadeIn(null);
            selectWorldCanvas.alpha = 0;

            if (OnyxGameInstance.instance.isFromTitleScene)
            {
                OnyxGameInstance.instance.isFromTitleScene = false;
                SetupGreeting();
            }
            else if(OnyxGameInstance.instance.isFromBatteMapScene)
                OnyxGameInstance.instance.isFromBatteMapScene = false;
            else
                SetupGreeting();
        }
    }

    private void SetupGreeting()
    {
        foreach (Canvas canvas in canvasesToHideBeforeFirstTouch)
            canvas.gameObject.SetActive(false);
        firstTouchCanvas.gameObject.SetActive(true);

        SetupGreetingAudios();
        SetupGreetingCamera();
    }

    private void SetupGreetingAudios()
    {
        OnyxGameInstance.instance.BgmAudioSource.clip = bgm;
        OnyxGameInstance.instance.BgmAudioSource.Play();
        voiceAudioSource.Play();
    }

    private void SetupGreetingCamera()
    {
        Vector3 oldPosition = mainCamera.transform.position;

        mainCamera.orthographicSize = OnyxGameInstance.instance.GreetingCameraSize;

        Vector2 greetingPosition = OnyxGameInstance.instance.GreetingCameraPosition;
        mainCamera.transform.position = ClampCameraPosition(mainCamera.orthographicSize, new Vector3(greetingPosition.x, greetingPosition.y, oldPosition.z));
    }

    public void OnFirstTouch()
    {
        firstTouchCanvas.gameObject.SetActive(false);

        StartCoroutine(MoveCameraToIdlePosition(AfterMove));

        void AfterMove()
        {
            foreach (Canvas canvas in canvasesToHideBeforeFirstTouch)
                canvas.gameObject.SetActive(true);
            noticeCanvas.gameObject.SetActive(true);
        }
    }

    private IEnumerator MoveCameraToIdlePosition(Action afterMove)
    {
        Vector3 GetCameraPosition() => mainCamera.transform.position;

        Vector2 startPosition = GetCameraPosition();
        float startSize = mainCamera.orthographicSize;

        float distance = Vector2.Distance(startPosition, cameraIdlePosition);
        float sizeDiff = Mathf.Abs(startSize - cameraIdleSize);

        float duration = 1f / 30f;

        while (!IsReached())
        {
            yield return new WaitForSeconds(duration);
            Vector2 currentCameraPosition = GetCameraPosition();
            Vector2 newCameraPosition = Vector2.MoveTowards(currentCameraPosition, cameraIdlePosition, distance / idleRecoverTime * duration);

            mainCamera.transform.position = new Vector3(newCameraPosition.x, newCameraPosition.y, mainCamera.transform.position.z);
            mainCamera.orthographicSize = Mathf.MoveTowards(mainCamera.orthographicSize, cameraIdleSize, sizeDiff / idleRecoverTime * duration);
        }

        yield return new WaitForSeconds(0.5f);

        afterMove();

        bool IsReached()
        {
            return GetCameraPosition().x == cameraIdlePosition.x
                && GetCameraPosition().y == cameraIdlePosition.y
                && mainCamera.orthographicSize == cameraIdleSize;
        }
    }

    public void OnTouchGreetingDecorationButton()
    {
        foreach (Canvas canvas in canvasesToHideBeforeFirstTouch)
            canvas.gameObject.SetActive(false);

        greetingDecorationGui.gameObject.SetActive(true);
        Vector3 oldPosition = mainCamera.transform.position;
        Vector2 greetingPosition = OnyxGameInstance.instance.GreetingCameraPosition;
        mainCamera.transform.position = new Vector3(greetingPosition.x, greetingPosition.y, oldPosition.z);
        mainCamera.orthographicSize = OnyxGameInstance.instance.GreetingCameraSize;
    }

    public void OnGreetingCameraUpDownButton(bool isDown)
    {
        Vector3 oldPosition = mainCamera.transform.position;
        Vector3 newPosition = oldPosition + new Vector3(0, isDown ? -greetingPositionUnit : greetingPositionUnit, 0);

        mainCamera.transform.position = ClampCameraPosition(mainCamera.orthographicSize, newPosition);
    }
    public void OnGreetingCameraLeftRightButton(bool isRight)
    {
        Vector3 oldPosition = mainCamera.transform.position;
        Vector3 newPosition = oldPosition + new Vector3(isRight ? greetingPositionUnit : -greetingPositionUnit, 0, 0);

        mainCamera.transform.position = ClampCameraPosition(mainCamera.orthographicSize, newPosition);
    }

    public void OnGreetingCameraSizeButton(bool isCloseUp)
    {
        float oldSize = mainCamera.orthographicSize;
        float newSize = oldSize + (isCloseUp? -greetingSizeUnit : greetingSizeUnit);

        Vector2 greetingCameraSizeMinMax = OnyxGameInstance.instance.GreetingCameraSizeMinMax;
        newSize = Mathf.Clamp(newSize, greetingCameraSizeMinMax.x, greetingCameraSizeMinMax.y);

        mainCamera.orthographicSize = newSize;
        mainCamera.transform.position = ClampCameraPosition(newSize, mainCamera.transform.position);
    }

    private Vector3 ClampCameraPosition(float cameraSize, Vector3 inputPosition)
    {
        Vector3 outputPosition = inputPosition;

        float maxCameraSize = OnyxGameInstance.instance.GreetingCameraSizeMinMax.y;
        float diffOfMaxAndCurrentSize = maxCameraSize - cameraSize;

        outputPosition.x = Mathf.Clamp(outputPosition.x, -diffOfMaxAndCurrentSize, diffOfMaxAndCurrentSize);
        outputPosition.y = Mathf.Clamp(outputPosition.y, -diffOfMaxAndCurrentSize, diffOfMaxAndCurrentSize);

        return outputPosition;
    }

    public void OnGreetingCameraEditSaveButton()
    {
        touchPreventingCanvas.gameObject.SetActive(true);
        OnyxGameInstance.instance.SaveGreetingCameraInfo(mainCamera.transform.position, mainCamera.orthographicSize, afterSave);

        void afterSave()
        {
            touchPreventingCanvas.gameObject.SetActive(false);
            OnGreetingCameraEditCloseButton();
        }
    }

    public void OnGreetingCameraEditCloseButton()
    {
        Vector3 oldPosition = mainCamera.transform.position;
        mainCamera.transform.position = new Vector3(cameraIdlePosition.x, cameraIdlePosition.y, oldPosition.z);
        mainCamera.orthographicSize = cameraIdleSize;

        greetingDecorationGui.gameObject.SetActive(false);

        foreach (Canvas canvas in canvasesToHideBeforeFirstTouch)
            canvas.gameObject.SetActive(true);
    }

    public void DisplaySelectWorldGui()
    {
        selectWorldCanvas.gameObject.SetActive(true);

        IEnumerator IncreaseAlpha()
        {
            while (selectWorldCanvas.alpha != 1)
            {
                yield return new WaitForSeconds(TargetFrameSeconds);
                selectWorldCanvas.alpha = Mathf.Min(1, selectWorldCanvas.alpha + TargetFrameSeconds / 0.3f);
            }
        }

        StartCoroutine(IncreaseAlpha());
    }

    public void HideSelectWorldGui()
    {
        IEnumerator DecreaseAlpha()
        {
            while (selectWorldCanvas.alpha != 0)
            {
                yield return new WaitForSeconds(TargetFrameSeconds);
                selectWorldCanvas.alpha = Mathf.Max(0, selectWorldCanvas.alpha - TargetFrameSeconds / 0.3f);
            }
            selectWorldCanvas.gameObject.SetActive(false);
        }

        StartCoroutine(DecreaseAlpha());
    }

    public void DisplayResearchGui()
    {
        researchCanvas.gameObject.SetActive(true);

        IEnumerator IncreaseAlpha()
        {
            while (researchCanvas.alpha != 1)
            {
                yield return new WaitForSeconds(TargetFrameSeconds);
                researchCanvas.alpha = Mathf.Min(1, researchCanvas.alpha + TargetFrameSeconds / 0.3f);
            }
        }

        StartCoroutine(IncreaseAlpha());
    }

    public void HideResearchGui()
    {
        IEnumerator DecreaseAlpha()
        {
            while (researchCanvas.alpha != 0)
            {
                yield return new WaitForSeconds(TargetFrameSeconds);
                researchCanvas.alpha = Mathf.Max(0, researchCanvas.alpha - TargetFrameSeconds / 0.3f);
            }
            researchCanvas.gameObject.SetActive(false);
        }

        StartCoroutine(DecreaseAlpha());
    }

    public void OnLevelButton()
    {
        int index = OnyxGameInstance.instance.PlayerLevel - 1;
        if(levelUpCosts.Count > index && OnyxGameInstance.instance.OnyxValue >= levelUpCosts[index])
        {
            touchPreventingCanvas.gameObject.SetActive(true);
            OnyxGameInstance.instance.LevelUpPlayer(AfterLevelUp);
        }

        void AfterLevelUp()
        {
            touchPreventingCanvas.gameObject.SetActive(false);
            playerLevelGuiOnMain.text = OnyxGameInstance.instance.PlayerLevel.ToString();
            onyxResourceGui.text = OnyxGameInstance.instance.OnyxValue.ToString();

            UpdateResearchGui();
        }
    }

    public void UpdateResearchGui()
    {
        float multiplier = playerStatusMultiplier.GetMultiplier(OnyxGameInstance.instance.PlayerLevel);
        int index = OnyxGameInstance.instance.PlayerLevel - 1;
        if (levelUpCosts.Count > index)
        {
            playerCurrentLevelGuiOnResearch.text = OnyxGameInstance.instance.PlayerLevel.ToString();
            playerNextLevelGuiOnResearch.text = (OnyxGameInstance.instance.PlayerLevel + 1).ToString();

            float nextLevelMultiplier = playerStatusMultiplier.GetMultiplier(OnyxGameInstance.instance.PlayerLevel + 1);

            playerCurrentHealthGuiOnResearch.text = Mathf.Floor(playerHealthDefaultValue * multiplier).ToString();
            playerNextHealthGuiOnResearch.text = Mathf.Floor(playerHealthDefaultValue * nextLevelMultiplier).ToString();

            playerCurrentAttackGuiOnResearch.text = Mathf.Floor(playerAttackDefaultValue * multiplier).ToString();
            playerNextAttackGuiOnResearch.text = Mathf.Floor(playerAttackDefaultValue * nextLevelMultiplier).ToString();

            currentOnyxGuiOnResearch.text = OnyxGameInstance.instance.OnyxValue.ToString();
            int remainingOnyx = OnyxGameInstance.instance.OnyxValue - levelUpCosts[index];
            remainingOnyxGuiOnResearch.text = remainingOnyx.ToString();

            if (OnyxGameInstance.instance.OnyxValue < levelUpCosts[index])
            {
                remainingOnyxGuiOnResearch.color = new Color(1, 0, 0);
                levelUpButtonOnResearch.interactable = false;
                levelUpButtonTextOnResearch.text = "Not Enough Cost";
            }
        }
        else
        {
            playerCurrentLevelGuiOnResearch.text = OnyxGameInstance.instance.PlayerLevel.ToString();
            playerNextLevelGuiOnResearch.text = "-";

            playerCurrentHealthGuiOnResearch.text = Mathf.Floor(playerHealthDefaultValue * multiplier).ToString();
            playerNextHealthGuiOnResearch.text = "-";

            playerCurrentAttackGuiOnResearch.text = Mathf.Floor(playerAttackDefaultValue * multiplier).ToString();
            playerNextAttackGuiOnResearch.text = "-";

            currentOnyxGuiOnResearch.text = "-";
            remainingOnyxGuiOnResearch.text = "-";
            levelUpButtonOnResearch.interactable = false;
            levelUpButtonTextOnResearch.text = "Max Level";
        }
    }

    public void LoadBattleMap()
    {
        fadeGui.StartFadeOut(afterFadeOut);

        static void afterFadeOut(FadeGui.FinishStatus finishStatus)
        {
            OnyxGameInstance.instance.isFromBatteScene = false;
            SceneManager.LoadScene((int)ScenesToBeBuild.StageMapScene);
        }
    }

    private void Update()
    {
        if(Input.GetButtonDown("Cancel"))
        {
            if (selectWorldCanvas.gameObject.activeInHierarchy)
                HideSelectWorldGui();
            else
                quitCanvas.gameObject.SetActive(true);
        }
    }

    public void CloseQuitAlert()
    {
        quitCanvas.gameObject.SetActive(false);
    }

    public void QuitGame()
    {
        Debug.Log("QuitGame!");
        Application.Quit();
    }

    public enum Direction4
    {
        Left,
        Right,
        Up,
        Down,
    }
}
