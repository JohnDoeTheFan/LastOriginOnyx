using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StageSceneTransitionGui : MonoBehaviourBase
{
    [SerializeField]
    [Range(0.1f, 5f)]
    private float dissolveTime = 1.5f;
    [SerializeField]
    [Range(0.1f, 5f)]
    private float fadeMessageTime = 1f;
    [SerializeField]
    private AudioClip onlineAudio;
    [SerializeField]
    private AudioClip offlineAudio;


    private Image image;
    private Animator animator;
    private AudioSource audioSource;
    private float dissolve;

    private bool isDissolving = false;

    private void Awake()
    {
        image = GetComponent<Image>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
    }

    private void OnDestroy()
    {
        image.material.SetFloat("Dissolve", 0);
    }

    public void StartDissolveOut(bool isOnline, Action afterDissolve)
    {
        if (!isDissolving)
        {
            dissolve = 0;
            image.material.SetFloat("Dissolve", dissolve);

            if (isOnline)
                audioSource.clip = onlineAudio;
            else
                audioSource.clip = offlineAudio;
            audioSource.Play();

            StartCoroutine(Job(() => Dissolve(1 / dissolveTime * TargetFrameSeconds), DisplayMessage));

            void DisplayMessage()
            {
                animator.SetBool("DisplayMessage", true);
                animator.SetBool("IsOnline", isOnline);
                animator.SetTrigger("Start");
                StartCoroutine(Job(() => WaitForSecondsRealtimeRoutine(fadeMessageTime), () => afterDissolve?.Invoke()));
            }
        }
        else
        {
            afterDissolve?.Invoke();
        }

    }

    public void StartDissolveIn(bool isOnline, Action afterDissolve)
    {
        if (!isDissolving)
        {
            dissolve = 1;
            image.material.SetFloat("Dissolve", dissolve);

            animator.SetBool("DisplayMessage", false);
            animator.SetBool("IsOnline", isOnline);
            animator.SetTrigger("Start");
            StartCoroutine(Job(() => WaitForSecondsRealtimeRoutine(fadeMessageTime), DissolveOut));

            void DissolveOut()
            {
                StartCoroutine(Job(() => Dissolve(-1 * 1 / dissolveTime * TargetFrameSeconds), () => afterDissolve?.Invoke()));
            }
        }
        else
        {
            afterDissolve?.Invoke();
        }
    }

    IEnumerator Dissolve(float value)
    {
        if (isDissolving)
            yield break;
        
        isDissolving = true;

        do
        {
            yield return new WaitForSecondsRealtime(TargetFrameSeconds);
            dissolve = Mathf.Clamp01(dissolve + value);
            image.material.SetFloat("Dissolve", dissolve);
        } while (dissolve != 0 && dissolve != 1);

        isDissolving = false;

    }
}
