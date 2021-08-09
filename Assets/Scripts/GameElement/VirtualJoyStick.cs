using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class VirtualJoyStick : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
{
    [SerializeField] private RectTransform background;
    [SerializeField] private Image stick;
    [SerializeField] private Vector2 value;

    public Vector2 Value => value;

    void IDragHandler.OnDrag(PointerEventData eventData)
    {
        Vector2 position;
        if(RectTransformUtility.ScreenPointToLocalPointInRectangle(background, eventData.position, eventData.pressEventCamera, out position))
        {
            position.x = (position.x / background.sizeDelta.x);
            position.y = (position.y / background.sizeDelta.y);

            value = position * 2;
            if (value.magnitude > 1f)
                value = value.normalized;

            UpdateStickImage();
        }
    }

    void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
    {
        ((IDragHandler)this).OnDrag(eventData);
    }

    void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
    {
        value = Vector2.zero;
        UpdateStickImage();
    }

    void UpdateStickImage()
    {
        stick.rectTransform.anchoredPosition = value * background.sizeDelta / 2;
    }

}
