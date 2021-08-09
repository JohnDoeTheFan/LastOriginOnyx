using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Onyx.Ability;

public class AbilityGui : MonoBehaviourBase
{
    [SerializeField]
    private Text abilityName;
    [SerializeField]
    private skillGuiSlot skillGuiSlotForSingle;
    [SerializeField]
    private List<skillGuiSlot> skillGuiSlotsForDouble;
    [SerializeField]
    private AbilitySkillGui skillGuiPrefab;

    readonly private List<AbilitySkillGui> skillGuis = new List<AbilitySkillGui>();

    public void SetAbility(IAbility ability)
    {
        UnsetAbility();

        abilityName.text = ability.AbilityName;

        if( ability.Skills.Count == 1 )
        {
            skillGuiSlotForSingle.SetActive(true);
            foreach (skillGuiSlot slot in skillGuiSlotsForDouble)
                slot.SetActive(false);

            skillGuis.Add(Instantiate<AbilitySkillGui>(skillGuiPrefab, skillGuiSlotForSingle.holder));
            skillGuis[0].SetAbilitySkill(ability.Skills[0]);
        }
        else if( ability.Skills.Count >= 2 )
        {
            skillGuiSlotForSingle.SetActive(false);
            foreach (skillGuiSlot slot in skillGuiSlotsForDouble)
                slot.SetActive(true);

            skillGuis.Add(Instantiate<AbilitySkillGui>(skillGuiPrefab, skillGuiSlotsForDouble[0].holder));
            skillGuis.Add(Instantiate<AbilitySkillGui>(skillGuiPrefab, skillGuiSlotsForDouble[1].holder));
            skillGuis[0].SetAbilitySkill(ability.Skills[0]);
            skillGuis[1].SetAbilitySkill(ability.Skills[1]);
        }
    }

    public void UnsetAbility()
    {
        abilityName.text = "";

        skillGuis.ForEach(item =>
        {
            item.UnsetAbilitySkill();
            Destroy(item.gameObject);
        });

        skillGuis.Clear();

        skillGuiSlotForSingle.SetActive(false);
        foreach (skillGuiSlot slot in skillGuiSlotsForDouble)
            slot.SetActive(false);
    }

    [System.Serializable]
    struct skillGuiSlot
    {
        public RectTransform holder;
        public RectTransform keyLabel;

        public void SetActive(bool active)
        {
            holder.gameObject.SetActive(active);
            keyLabel.gameObject.SetActive(active);
        }
    }

}
