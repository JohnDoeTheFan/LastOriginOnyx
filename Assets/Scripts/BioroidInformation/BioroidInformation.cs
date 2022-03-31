using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

[CreateAssetMenu(menuName = "Utility/BioroidInformation")]
public class BioroidInformation : ScriptableObject
{
    [SerializeField] private int id;
    [SerializeField] private Onyx.MyUnit unit;
    [SerializeField] private Sprite image;
    [SerializeField] private Sprite portrait;
    [SerializeField] private string bioroidName;
    [SerializeField] [TextArea] private string description;
    [SerializeField] private int unlockCost;
    [SerializeField] private int unlockLevel;
    [SerializeField] private AudioClip greetingAudioClip;

    [SerializeField] private List<AbilityDescription> abilities;

    public int Id => id;
    public Onyx.MyUnit Unit => unit;
    public Sprite Image => image;
    public Sprite Portrait => portrait;
    public string BioroidName => bioroidName;
    public string Description => description;
    public int UnlockCost => unlockCost;
    public int UnlockLevel => unlockLevel;
    public AudioClip GreetingAudioClip => greetingAudioClip;
    public ReadOnlyCollection<AbilityDescription> Abilities => abilities.AsReadOnly();

    [System.Serializable]
    public struct AbilityDescription
    {
        public Sprite image;
        public string abilityName;
        [TextArea] public string description;

        public List<SkillDescription> skills;

        [System.Serializable]
        public struct SkillDescription
        {
            public Sprite image;
            public string skillName;
            [TextArea] public string description;
        }
    }
}
