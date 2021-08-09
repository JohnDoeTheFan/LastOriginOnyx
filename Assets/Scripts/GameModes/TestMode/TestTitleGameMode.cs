using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TestTitleGameMode : MonoBehaviour
{
    public bool IsOnToggle0 { get; set; }
    public bool IsOnToggle1 { get; set; }
    public bool IsOnToggle2 { get; set; }

    public AudioSource uiAudioSource;
    public AudioClip buttonSound;
    public AudioClip toggleSound;

    public void OnClickedStartButton()
    {
        uiAudioSource.clip = buttonSound;
        uiAudioSource.Play();

        if (IsOnToggle0)
            MyGameInstance.instance.difficulty = MyGameInstance.Difficulty.Easy;
        if (IsOnToggle1)
            MyGameInstance.instance.difficulty = MyGameInstance.Difficulty.Normal;
        if (IsOnToggle2)
            MyGameInstance.instance.difficulty = MyGameInstance.Difficulty.Hard;

        StartCoroutine(LoadSceneCoroutine(uiAudioSource.clip.length));
    }
    
    public void OnValueChangedToggle(Toggle toggle)
    {
        if(toggle.isOn)
        {
            uiAudioSource.clip = toggleSound;
            uiAudioSource.Play();
        }
    }

    IEnumerator LoadSceneCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(1);
    }
}
