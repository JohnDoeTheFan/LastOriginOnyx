using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CombatantEmbargoToggle : MonoBehaviour
{
    public static Action<BioroidInformation> OnSelected;

    [SerializeField] private Toggle toggle;
    [SerializeField] private Image portrait;
    [SerializeField] private Text bioroidName;
    [SerializeField] private Text unlockCost;
    [SerializeField] private Graphic lowLevelIndicator;

    private BioroidInformation bioroidInformation;

    public Toggle Toggle => toggle;

    public void SetBioroidInformation(BioroidInformation bioroidInformation, int playerLevel, int onyxValue)
    {
        this.bioroidInformation = bioroidInformation;
        portrait.sprite = bioroidInformation.Portrait;
        bioroidName.text = bioroidInformation.BioroidName;
        unlockCost.text = bioroidInformation.UnlockCost.ToString();
        if (onyxValue < bioroidInformation.UnlockCost)
            unlockCost.color = Color.red;
        if (playerLevel < bioroidInformation.UnlockLevel)
        {
            lowLevelIndicator.gameObject.SetActive(true);
            toggle.interactable = false;
        }
    }

    public void OnValueChanged()
    {
        if (toggle.interactable && toggle.isOn)
            OnSelected(bioroidInformation);
    }
}
