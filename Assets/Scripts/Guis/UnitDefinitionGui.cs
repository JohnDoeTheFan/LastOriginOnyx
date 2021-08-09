using System.Collections;
using System;
using UnityEngine;
using UnityEngine.UI;

public class UnitDefinitionGui : MonoBehaviour
{
    public interface ISubscriber
    {
        void OnClick(UnitDefinitionGui unitDefinitionGui);
    }

    public Func<float> GetRemainCoolTime;

    public SubscribeManagerTemplate<ISubscriber> SubscribeManager { private set; get; } = new SubscribeManagerTemplate<ISubscriber>();

    public struct UnitDefinition
    {
        public string name;
        public Sprite image;
        public int cost;
        public float coolTime;
    }

    public UnitDefinition unitDefinition;
    public Text unitName;
    public Image unitImage;
    public Text unitCost;
    public Image costPanel;
    public Image coolTimeMask;

    bool isCoolTimeDisplayUpdaterWorking = false;

    public void SetUnitDefinition(UnitDefinition unitDefinition)
    {
        this.unitDefinition = unitDefinition;
        unitImage.sprite = unitDefinition.image;
        unitImage.preserveAspect = true; ;
        unitName.text = unitDefinition.name;
        unitCost.text = unitDefinition.cost.ToString();
        StartCoolTimeDisplayUpdate();
    }

    public void UpdateAvailableDisplay(int currentCost)
    {
        if(unitDefinition.cost > currentCost)
            costPanel.color = new Color(1, 0, 0, 100 / 255);
        else
            costPanel.color = new Color(1, 1, 1, 100 / 255);
    }
    
    public void SetRemainCoolTime(float remainCoolTime)
    {
        Vector3 scale = coolTimeMask.rectTransform.localScale;
        scale.x = remainCoolTime / unitDefinition.coolTime;
        coolTimeMask.rectTransform.localScale = scale;
    }

    public void StartCoolTimeDisplayUpdate()
    {
        if( ! isCoolTimeDisplayUpdaterWorking)
            StartCoroutine(UpdateCoolTimeDisplay());
    }

    IEnumerator UpdateCoolTimeDisplay()
    {
        isCoolTimeDisplayUpdaterWorking = true;
        SetRemainCoolTime((GetRemainCoolTime?.Invoke() ?? 0f));
        while ((GetRemainCoolTime?.Invoke() ?? 0f) != 0)
        {
            yield return new WaitForSeconds(0.03f);
            SetRemainCoolTime((GetRemainCoolTime?.Invoke() ?? 0f));
        }
        isCoolTimeDisplayUpdaterWorking = false;
    }

    public void OnClick()
    {
        SubscribeManager.ForEach(item => item.OnClick(this));
    }

}
