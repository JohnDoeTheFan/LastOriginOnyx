using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CombatantToggle : MonoBehaviour
{
    static public Action<BioroidInformation> OnSelected;

    [SerializeField] private Image portrait;
    [SerializeField] private Text bioroidName;
    [SerializeField] private Toggle toggle;

    private BioroidInformation bioroidInformation;

    public Toggle Toggle => toggle;

    public void SetBioroidInformation(BioroidInformation bioroidInformation)
    {
        this.bioroidInformation = bioroidInformation;
        portrait.sprite = bioroidInformation.Portrait;
        bioroidName.text = bioroidInformation.BioroidName;
    }

    public void OnValueChanged()
    {
        if (toggle.isOn)
            OnSelected(bioroidInformation);
    }
}
