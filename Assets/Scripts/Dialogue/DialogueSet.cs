using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Utility/Dialogue/Set")]
public class DialogueSet : ScriptableObject
{
    public enum PortraitPosition
    {
        Left,
        Center,
        Right
    }

    [System.Serializable]
    public class Dialogue
    {
        public Sprite Portrait;
        public PortraitPosition PortraitPosition;
        public string name;
        [TextArea]public string text;
        public float updateDuration;
    }

    public Dialogue[] dialogues;
}
