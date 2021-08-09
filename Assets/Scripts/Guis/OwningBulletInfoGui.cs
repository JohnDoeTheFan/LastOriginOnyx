using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OwningBulletInfoGui : MonoBehaviour
{
    [SerializeField]
    private Text nameText;
    [SerializeField]
    private Image image;
    [SerializeField]
    private Text remainNum;

    private Image imageComponent;

    public struct BulletInfo
    {
        public string name;
        public Sprite image;
        public int remainNum;
    }

    private void Awake()
    {
        imageComponent = GetComponent<Image>();
    }

    public void SetBulletInfo(BulletInfo bulletInfo)
    {
        nameText.text = bulletInfo.name;
        image.sprite = bulletInfo.image;
        image.preserveAspect = true;
        remainNum.text = bulletInfo.remainNum.ToString();
    }
    public void SetBulletInfo(Loader<Bullet> loader)
    {
        nameText.text = loader.Item.name;
        image.sprite = loader.Item.Image;
        image.preserveAspect = true;
        remainNum.text = loader.Quantity.ToString();
    }

    public void SetFocus(bool focus)
    {
        if (focus)
            imageComponent.color = new Color(1, 1, 0, 0.4f);
        else
            imageComponent.color = new Color(1, 1, 1, 0.4f);
    }
}
