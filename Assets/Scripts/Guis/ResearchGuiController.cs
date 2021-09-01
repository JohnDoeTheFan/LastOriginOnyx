using System.Collections;
using System.Collections.Generic;
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
    [SerializeField] private Text embargoCurrentOnyxValue;
    [SerializeField] private Text embargoUnlockCost;
    [SerializeField] private Text embargoRemainOnyxValue;
    [SerializeField] private Button embargoUnlockButton;
    [SerializeField] private Text embargoUnlockButtonText;

    private BioroidInformation bioroidInfo;

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
        for (int i = combatantEmbargoToggleGroup.transform.childCount - 1; i >= 0; i--)
            Destroy(combatantEmbargoToggleGroup.transform.GetChild(i).gameObject);

        embargoListEmptyGraphic.gameObject.SetActive(true);
    }

    public void InitializeEmbargoInformation(BioroidInformation bioroidInformation, int onyxValue)
    {
        embargoPortrait.sprite = bioroidInformation.Portrait;
        embargoName.text = bioroidInformation.BioroidName;
        embargoCurrentOnyxValue.text = onyxValue.ToString();
        embargoUnlockCost.text = bioroidInformation.UnlockCost.ToString();
        embargoRemainOnyxValue.text = (onyxValue - bioroidInformation.UnlockCost).ToString();

        if (onyxValue < bioroidInformation.UnlockCost)
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
    }

    public void InitializeEmbargoInformation(int onyxValue)
    {
        embargoPortrait.sprite = null;
        embargoName.text = "-";
        embargoCurrentOnyxValue.text = onyxValue.ToString();
        embargoUnlockCost.text = "-";
        embargoRemainOnyxValue.text = "-";
        embargoUnlockButton.interactable = false;
        embargoUnlockButtonText.text = "Unlock";

        embargoRemainOnyxValue.color = Color.white;
    }
}
