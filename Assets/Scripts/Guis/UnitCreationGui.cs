using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

public class UnitCreationGui : MonoBehaviour
{
    public UnitDefinitionGui unitDefinitionGuiPrefab;
    public RectTransform scrollViewContent;

    private readonly List<UnitDefinitionGui> unitDefinitionGuis = new List<UnitDefinitionGui>();

    // Start is called before the first frame update
    void Start()
    {
        Hide();
    }

    public void Initialize(int numOfItem)
    {
        foreach (var unitDefGui in unitDefinitionGuis)
        {
            Destroy(unitDefGui.gameObject);
        }
        unitDefinitionGuis.Clear();

        for (int i = 0; i < numOfItem; i++)
        {
            UnitDefinitionGui unitDefGui = Instantiate<UnitDefinitionGui>(unitDefinitionGuiPrefab, scrollViewContent);
            RectTransform rectTransform = unitDefGui.GetComponent<RectTransform>();

            //Positioning
            rectTransform.anchoredPosition = new Vector2(rectTransform.sizeDelta.x * i, 0);

            unitDefinitionGuis.Add(unitDefGui);
        }

        gameObject.SetActive(true);
    }

    public void ForEachUnitDefinitionGui(Action<UnitDefinitionGui> action)
    {
        unitDefinitionGuis.ForEach(action);
    }

    public ReadOnlyCollection<UnitDefinitionGui> GetUnitDefinitionGuis()
    {
        return unitDefinitionGuis.AsReadOnly();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
    
}
