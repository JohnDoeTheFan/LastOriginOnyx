using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

[CreateAssetMenu(menuName = "Utility/BioroidInformation")]
public class BioroidInformation : ScriptableObject
{
    [SerializeField] private Onyx.MyUnit unit;
    [SerializeField] private Sprite portrait;
    [SerializeField] private string bioroidName;
    [SerializeField] private string description;

    [SerializeField] private List<AbilityDescription> abilities;

    public Sprite Portrait => portrait;
    public string BioroidName => bioroidName;
    public string Description => description;
    public ReadOnlyCollection<AbilityDescription> Abilities => abilities.AsReadOnly();

    [System.Serializable]
    public struct AbilityDescription
    {
        public Sprite image;
        public string abilityName;
        public string description;

        public List<SkillDescription> skills;

        [System.Serializable]
        public struct SkillDescription
        {
            public Sprite image;
            public string skillName;
            public string description;
        }
    }
}
