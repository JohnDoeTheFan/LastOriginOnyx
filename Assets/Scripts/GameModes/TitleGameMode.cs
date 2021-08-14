using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Onyx.Core;

public class TitleGameMode : MonoBehaviour
{
    [SerializeField] private FadeGui fadeGui;
    [SerializeField] private Canvas loadingCanvas;
    [SerializeField] private RectTransform creditPanel;
    [SerializeField] private RectTransform freeResourcesPanel;
    [SerializeField] private RectTransform resetSaveDataConfirmPanel;

    public void LoadLobbyScene()
    {
        loadingCanvas.gameObject.SetActive(true);
        OnyxGameInstance.instance.SignIn("RandomCommander", AfterSignIn);

        void AfterSignIn()
        {
            fadeGui.StartFadeOut(AfterFadeOut);
        }

        void AfterFadeOut(FadeGui.FinishStatus status)
        {
            OnyxGameInstance.instance.isFromTitleScene = true;
            SceneManager.LoadScene((int)ScenesToBeBuild.Lobby);
        }
    }

    public void DisplayCreditPanel(bool display)
    {
        creditPanel.gameObject.SetActive(display);
    }

    public void DisplayFreeResourcesPanel(bool display)
    {
        freeResourcesPanel.gameObject.SetActive(display);
    }

    public void DisplayResetSaveDataConfirmPanel(bool display)
    {
        resetSaveDataConfirmPanel.gameObject.SetActive(display);
    }

    public void ResetSaveData()
    {
        OnyxGameInstance.instance.ResetSaveData();
        DisplayResetSaveDataConfirmPanel(false);
    }
}
