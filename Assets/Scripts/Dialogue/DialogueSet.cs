using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Utility/Dialogue/Set")]
public class DialogueSet : ScriptableObject
{
    [System.Serializable]
    public class Dialogue
    {
        public string name;
        public string text;
        public float updateDuration;
    }

    public Dialogue[] dialogues;
}
