using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitInfoGui : MonoBehaviour
{ 
    public interface ISubscriber
    {
        void BeforeDestroy(UnitInfoGui unitInfoGui);
    }

    public SubscribeManagerTemplate<ISubscriber> SubscribeManager { private set; get; } = new SubscribeManagerTemplate<ISubscriber>();

    private void Start()
    {
        Hide();
    }
    private void Update()
    {
        UpdateUnitInfo();
    }

    private void OnDestroy()
    {
        SubscribeManager.ForEach(item => item.BeforeDestroy(this));
    }

    public struct UnitInfo
    {
        public string name;
        public SpriteRenderer renderer;
        public UnitInfo(string name, SpriteRenderer renderer)
        {
            this.name = name;
            this.renderer = renderer;
        }
    }

    public Text unitNameText;
    public Image unitImage;
    UnitInfo currentUnitInfo;

    public void Display(UnitInfo unitInfo)
    {
        currentUnitInfo = unitInfo;
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        currentUnitInfo = new UnitInfo();
    }

    public void UpdateUnitInfo()
    {
        if (gameObject.activeInHierarchy)
        {
            unitNameText.text = currentUnitInfo.name;
            unitImage.sprite = currentUnitInfo.renderer.sprite;
            unitImage.preserveAspect = true;
        }
    }
}
