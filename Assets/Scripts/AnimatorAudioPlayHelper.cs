using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorAudioPlayHelper : MonoBehaviour
{
    [SerializeField] List<AudioSource> audioList;
    
    public void PlayAudio(int index)
    {
        if(index < audioList.Count)
            audioList[index].Play();
    }
}
