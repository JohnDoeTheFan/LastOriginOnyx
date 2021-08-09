using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.UI;
using Onyx.Ability;

public class CommandPanel : MonoBehaviourBase
{
    [SerializeField]
    private Image unitImage;
    [SerializeField]
    private List<AbilityGui> abilityGuis;

    private RectTransform rectTransform;
    private Animator animator;

    private Func<ReadOnlyCollection<IAbility>> GetAbilities;


    public Vector2 GetSize()
    {
        return rectTransform.sizeDelta;
    }

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        animator = GetComponent<Animator>();
    }

    public void Unlock(List<IAbility> abilities)
    {
        GetAbilities = () => abilities.AsReadOnly();
        animator.SetBool("Unlocked", true);
    }

    public void Unlock(ReadOnlyCollection<IAbility> abilities)
    {
        GetAbilities = () => abilities;
        animator.SetBool("Unlocked", true);
    }

    public void Lock()
    {
        UnsetAbilityGuis();
        animator.SetBool("Unlocked", false);
    }

    public void InitAbilityGuis()
    {
        if( GetAbilities != null )
        {
            ReadOnlyCollection<IAbility> abilities = GetAbilities();

            if(abilities != null)
            {
                for (int i = 0; i < abilityGuis.Count; i++)
                {
                    if (i < abilities.Count)
                        abilityGuis[i].SetAbility(abilities[i]);
                }
            }

            GetAbilities = null;
        }
    }
    public void UnsetAbilityGuis()
    {
        for (int i = 0; i < abilityGuis.Count; i++)
        {
                abilityGuis[i].UnsetAbility();
        }
    }
}
