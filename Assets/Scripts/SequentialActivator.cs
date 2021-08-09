using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SequentialActivator : MonoBehaviourBase
{
    [SerializeField]
    private List<Sequence> sequences;
    [SerializeField]
    private List<Sequence> addtionalSequences;

    readonly private List<bool> isSequenceFinished = new List<bool>();

    public void PlaySequences(Action OnFinished)
    {
        isSequenceFinished.Clear();
        for (int i = 0; i < sequences.Count; i++)
        {
            isSequenceFinished.Add(false);
            int capturedIndex = i;
            sequences[i].Play(() => OnFinishedSequece(capturedIndex), StartCoroutine);
        }
        foreach(Sequence sequence in addtionalSequences)
        {
            sequence.Play(null, StartCoroutine);
        }

        void OnFinishedSequece(int i)
        {
            isSequenceFinished[i] = true;
            if (isSequenceFinished.TrueForAll((item => item)))
                OnFinished?.Invoke();
        }
    }

    [System.Serializable]
    struct TargetAndPlayTime
    {
        public GameObject target;
        public float playTime;

        public TargetAndPlayTime(GameObject target, float playTime)
        {
            this.target = target;
            this.playTime = playTime;
        }
    }

    [Serializable]
    struct Sequence
    {
        public List<TargetAndPlayTime> targetAndPlayTimes;
        public void Play(Action onFinished, Func<IEnumerator, Coroutine> StartCoroutine)
        {
            if (targetAndPlayTimes.Count > 0)
            {
                if (targetAndPlayTimes[0].target != null)
                    targetAndPlayTimes[0].target.SetActive(true);
                StartCoroutine(FinishCurrentAfterSeconds(targetAndPlayTimes[0].playTime, 0, onFinished, StartCoroutine));
            }
        }
        private IEnumerator FinishCurrentAfterSeconds(float seconds, int current, Action onFinished, Func<IEnumerator, Coroutine> StartCoroutine)
        {
            yield return new WaitForSeconds(seconds);

            if (targetAndPlayTimes[current].target != null)
                targetAndPlayTimes[current].target.SetActive(false);

            int next = current + 1;
            if (targetAndPlayTimes.Count > next)
                PlayNext(next, onFinished, StartCoroutine);
            else
                onFinished?.Invoke();
        }

        private void PlayNext(int next, Action onFinished, Func<IEnumerator, Coroutine> StartCoroutine)
        {
            if (targetAndPlayTimes[next].target != null)
                targetAndPlayTimes[next].target.SetActive(true);
            StartCoroutine(FinishCurrentAfterSeconds(targetAndPlayTimes[0].playTime, next, onFinished, StartCoroutine));
        }
    }
}
