using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MouseEffector : MonoBehaviourBase
{
    [SerializeField]
    private Canvas canvas;
    [SerializeField]
    private RectTransform clickEffectGui;
    [SerializeField]
    [Range(0.2f, 1.5f)]
    private float destroyClickEffectTime = 0.5f;

    private RectTransform canvasRectTransform;

    private void Awake()
    {
        canvasRectTransform = canvas.GetComponent<RectTransform>();
    }

    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            RectTransform rectTransform = Instantiate<RectTransform>(clickEffectGui, canvas.transform);

            
            Vector2 mousePosition = Input.mousePosition / canvas.renderingDisplaySize * canvasRectTransform.sizeDelta;

            rectTransform.anchoredPosition = mousePosition;

            StartCoroutine(Job(() => WaitForSecondsRoutine(destroyClickEffectTime), () => Destroy(rectTransform.gameObject)));
        }
    }
}