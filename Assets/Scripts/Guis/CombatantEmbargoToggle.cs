using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CombatantEmbargoToggle : MonoBehaviour
{
    public static Action<BioroidInformation> OnSelected;

    [SerializeField] Toggle toggle;

    public Toggle Toggle => toggle;

    public void SetBioroidInformation(BioroidInformation bioroidInformation, int playerLevel, int onyxValue)
    {

    }
}
