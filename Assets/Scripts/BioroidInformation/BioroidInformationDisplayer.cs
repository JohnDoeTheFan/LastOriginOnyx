using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class BioroidInformationDisplayer : MonoBehaviour
{
    [SerializeField] private Image portrait;
    [SerializeField] private Text bioroidNameText;
    [SerializeField] private Text bioroidDescriptionText;

    [SerializeField] private List<AbilityDisplay> abilityDisplays;

    [SerializeField] private BioroidInformation bioroidInformation;

    private void Start()
    {
        ParseBioroidInformation(bioroidInformation);
    }

    private void ParseBioroidInformation(BioroidInformation bioroid)
    {
        portrait.sprite = bioroid.Portrait;
        bioroidNameText.text = bioroid.BioroidName;
        bioroidDescriptionText.text = bioroid.Description;

        ReadOnlyCollection<BioroidInformation.AbilityDescription> abilities = bioroid.Abilities;

        for(int i = 0; i < abilityDisplays.Count; i++)
        {
            if(abilities.Count > i)
            {
                abilityDisplays[i].abilityName.text = abilities[i].abilityName;
                abilityDisplays[i].abilityDescription.text = abilities[i].description;

                for(int j = 0; j < abilityDisplays[i].skillDisplays.Count; j++)
                {
                    if(abilities[i].skills.Count > j)
                    {
                        abilityDisplays[i].skillDisplays[j].icon.sprite = abilities[i].skills[j].image;
                        abilityDisplays[i].skillDisplays[j].skillName.text = abilities[i].skills[j].skillName;
                        abilityDisplays[i].skillDisplays[j].skillDescription.text = abilities[i].skills[j].description;
                    }
                }
            }
        }
    }


    [System.Serializable]
    struct AbilityDisplay
    {
        public Text abilityName;
        public Text abilityDescription;

        public List<SkillDisplay> skillDisplays;

        [System.Serializable]
        public struct SkillDisplay
        {
            public Image icon;
            public Text skillName;
            public Text skillDescription;
        }
    }
}
