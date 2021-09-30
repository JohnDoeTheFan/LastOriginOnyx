using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class ResearchGuiController : MonoBehaviour
{
    [Header("Upgrade")]
    [SerializeField] private RectTransform upgradePanel;
    [SerializeField] private Text upgradeCurrentLevel;
    [SerializeField] private Text upgradeNextLevel;
    [SerializeField] private Text upgradeCurrentHealth;
    [SerializeField] private Text upgradeNextHealth;
    [SerializeField] private Text upgradeCurrentAttack;
    [SerializeField] private Text upgradeNextAttack;
    [SerializeField] private Button upgradeLevelUpButton;
    [SerializeField] private Text upgradeLevelUpButtonText;
    [SerializeField] private Text upgradeNeededOnyx;
    [SerializeField] private Text upgradeCurrentOnyx;
    [SerializeField] private Text upgradeRemainingOnyx;

    [Header("Embargo")]
    [SerializeField] private RectTransform embargoPanel;
    [SerializeField] private ToggleGroup combatantEmbargoToggleGroup;
    [SerializeField] private CombatantEmbargoToggle combatantEmbargoTogglePrefab;
    [SerializeField] private Graphic embargoListEmptyGraphic;
    [SerializeField] private Image embargoPortrait;
    [SerializeField] private Text embargoName;
    [SerializeField] private Text embargoDescription;
    [SerializeField] private Text embargoCurrentOnyxValue;
    [SerializeField] private Text embargoUnlockCost;
    [SerializeField] private Text embargoRemainOnyxValue;
    [SerializeField] private Button embargoUnlockButton;
    [SerializeField] private Text embargoUnlockButtonText;
    [SerializeField] private ToggleGroup embargoSkillToggleGroup;
    [SerializeField] private List<Toggle> embargoSkillToggles;
    [SerializeField] private List<Image> embargoSkillImages;
    [SerializeField] private Text embargoSkillName;
    [SerializeField] private Text embargoSkillDescription;

    private BioroidInformation embargoSelectedCombatant;

    private const int numberOfSkillsPerAbility = 2;

    bool isFirstInitOfEmbargoSkillList = true;

    private void HideAllPanel()
    {
        upgradePanel.gameObject.SetActive(false);
        embargoPanel.gameObject.SetActive(false);
    }

    public void OnUpgradeToggleChanged(Toggle toggle)
    {
        if (toggle.isOn)
        {
            HideAllPanel();
            upgradePanel.gameObject.SetActive(true);
        }
    }
    public void OnEmbargoToggleChanged(Toggle toggle)
    {
        if (toggle.isOn)
        {
            HideAllPanel();
            embargoPanel.gameObject.SetActive(true);
        }
    }

    [System.Serializable]
    public struct Test
    {
        Toggle toggle;
        string name;
    }

    public void InitializeUpgradePanel(int playerLevel, int healthDefaultValue, int attackDefaultValue, float currentLevelMultiplier, float nextLevelMultiplier, int levelUpCost, int onyxValue)
    {
        upgradeCurrentLevel.text = playerLevel.ToString();
        upgradeNextLevel.text = (playerLevel + 1).ToString();

        upgradeCurrentHealth.text = Mathf.Floor(healthDefaultValue * currentLevelMultiplier).ToString();
        upgradeNextHealth.text = Mathf.Floor(healthDefaultValue * nextLevelMultiplier).ToString();

        upgradeCurrentAttack.text = Mathf.Floor(attackDefaultValue * currentLevelMultiplier).ToString();
        upgradeNextAttack.text = Mathf.Floor(attackDefaultValue * nextLevelMultiplier).ToString();

        upgradeNeededOnyx.text = levelUpCost.ToString();
        upgradeCurrentOnyx.text = onyxValue.ToString();
        int remainingOnyx = onyxValue - levelUpCost;
        upgradeRemainingOnyx.text = remainingOnyx.ToString();

        if (onyxValue < levelUpCost)
        {
            upgradeRemainingOnyx.color = new Color(1, 0, 0);
            upgradeLevelUpButton.interactable = false;
            upgradeLevelUpButtonText.text = "Not Enough Cost";
        }
    }

    public void InitializeUpgradePanel(int playerLevel, int healthDefaultValue, int attackDefaultValue, float currentLevelMultiplier)
    {
        upgradeCurrentLevel.text = playerLevel.ToString();
        upgradeNextLevel.text = "-";

        upgradeCurrentHealth.text = Mathf.Floor(healthDefaultValue * currentLevelMultiplier).ToString();
        upgradeNextHealth.text = "-";

        upgradeCurrentAttack.text = Mathf.Floor(attackDefaultValue * currentLevelMultiplier).ToString();
        upgradeNextAttack.text = "-";

        upgradeNeededOnyx.text = "-";
        upgradeCurrentOnyx.text = "-";
        upgradeRemainingOnyx.text = "-";
        upgradeLevelUpButton.interactable = false;
        upgradeLevelUpButtonText.text = "Max Level";
    }

    public void InitializeEmbargoList(List<BioroidInformation> bioroidList, int playerLevel, int onyxValue)
    {
        InitEmbargoSkillList();

        for (int i = combatantEmbargoToggleGroup.transform.childCount - 1; i >= 0; i--)
            Destroy(combatantEmbargoToggleGroup.transform.GetChild(i).gameObject);

        bool isFirstInteractableToggle = true;
        combatantEmbargoToggleGroup.allowSwitchOff = true;
        foreach (BioroidInformation bioroid in bioroidList)
        {
            CombatantEmbargoToggle newToggle = Instantiate<CombatantEmbargoToggle>(combatantEmbargoTogglePrefab, combatantEmbargoToggleGroup.transform);
            newToggle.SetBioroidInformation(bioroid, playerLevel, onyxValue);
            newToggle.Toggle.group = combatantEmbargoToggleGroup;

            if (newToggle.Toggle.interactable && isFirstInteractableToggle)
            {
                newToggle.Toggle.isOn = true;
                combatantEmbargoToggleGroup.allowSwitchOff = false;
                isFirstInteractableToggle = false;
            }
        }
    }

    public void InitializeEmbargoList()
    {
        InitEmbargoSkillList();

        for (int i = combatantEmbargoToggleGroup.transform.childCount - 1; i >= 0; i--)
            Destroy(combatantEmbargoToggleGroup.transform.GetChild(i).gameObject);

        embargoListEmptyGraphic.gameObject.SetActive(true);
    }

    public void InitEmbargoSkillList()
    {
        if(isFirstInitOfEmbargoSkillList)
        {
            for (int i = 0; i < embargoSkillToggles.Count; i++)
            {
                int abilityIndex = i / numberOfSkillsPerAbility;
                int skillIndex = i % numberOfSkillsPerAbility;

                Action<bool> OnValueChanged = (isOn) =>
                {
                    if (isOn && embargoSelectedCombatant != null)
                    {
                        ReadOnlyCollection<BioroidInformation.AbilityDescription> abilities = embargoSelectedCombatant.Abilities;
                        if (abilityIndex < abilities.Count && skillIndex < abilities[abilityIndex].skills.Count)
                        {
                            BioroidInformation.AbilityDescription.SkillDescription skill = abilities[abilityIndex].skills[skillIndex];
                            embargoSkillName.text = skill.skillName;
                            embargoSkillDescription.text = skill.description;
                        }
                    }
                };

                embargoSkillToggles[i].onValueChanged.AddListener(new UnityEngine.Events.UnityAction<bool>(OnValueChanged));
            }

            isFirstInitOfEmbargoSkillList = false;
        }
    }

    public void InitializeEmbargoInformation(BioroidInformation bioroidInformation, int onyxValue)
    {
        embargoSelectedCombatant = bioroidInformation;

        embargoPortrait.sprite = embargoSelectedCombatant.Portrait;
        embargoName.text = embargoSelectedCombatant.BioroidName;
        embargoDescription.text = embargoSelectedCombatant.Description;
        embargoCurrentOnyxValue.text = onyxValue.ToString();
        embargoUnlockCost.text = embargoSelectedCombatant.UnlockCost.ToString();
        embargoRemainOnyxValue.text = (onyxValue - embargoSelectedCombatant.UnlockCost).ToString();

        if (onyxValue < embargoSelectedCombatant.UnlockCost)
        {
            embargoRemainOnyxValue.color = Color.red;
            embargoUnlockButton.interactable = false;
            embargoUnlockButtonText.text = "Not Enough Cost";
        }
        else
        {
            embargoRemainOnyxValue.color = Color.white;
            embargoUnlockButton.interactable = true;
            embargoUnlockButtonText.text = "Unlock";
        }

        foreach (Toggle skillToggle in embargoSkillToggles)
            skillToggle.interactable = false;
        foreach (Image skillImage in embargoSkillImages)
            skillImage.gameObject.SetActive(false);

        var abilities = embargoSelectedCombatant.Abilities;

        for (int i = 0; i < abilities.Count; i++)
        {
            for (int j = 0; j < abilities[i].skills.Count; j++)
            {
                int index = i * numberOfSkillsPerAbility + j;
                if (index < embargoSkillToggles.Count)
                {
                    embargoSkillToggles[index].interactable = true;
                    embargoSkillImages[index].gameObject.SetActive(true);
                    embargoSkillImages[index].sprite = abilities[i].skills[j].image;
                }
            }
        }

        if(embargoSkillToggles[0].isOn)
        {
            embargoSkillToggleGroup.allowSwitchOff = true;
            embargoSkillToggles[0].isOn = false;
        }
        embargoSkillToggleGroup.allowSwitchOff = false;
        embargoSkillToggles[0].isOn = true;
    }

    public void InitializeEmbargoInformation(int onyxValue)
    {
        embargoPortrait.sprite = null;
        embargoName.text = "-";
        embargoDescription.text = "-";
        embargoCurrentOnyxValue.text = onyxValue.ToString();
        embargoUnlockCost.text = "-";
        embargoRemainOnyxValue.text = "-";
        embargoUnlockButton.interactable = false;
        embargoUnlockButtonText.text = "Unlock";

        embargoRemainOnyxValue.color = Color.white;

        embargoSkillToggleGroup.allowSwitchOff = true;
        foreach (Toggle skillToggle in embargoSkillToggles)
        {
            skillToggle.interactable = false;
            skillToggle.isOn = false;
        }
        foreach (Image skillImage in embargoSkillImages)
            skillImage.gameObject.SetActive(false);

        embargoSkillName.text = "-";
        embargoSkillDescription.text = "-";
    }
}
