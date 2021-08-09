using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldPositionGui : MonoBehaviourBase
{
    [SerializeField]
    private Camera targetCamera;
    [SerializeField]
    private RectTransform mainCanvasRectTransform;

    protected RectTransform rectTransform;

    protected virtual Vector3 Position { get; }

    protected virtual void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void Update()
    {
        UpdatePosition();
    }

    protected void Initialize(Camera targetCamera, RectTransform mainCanvasTransform)
    {
        this.targetCamera = targetCamera;
        if(mainCanvasRectTransform == null)
            mainCanvasRectTransform = mainCanvasTransform;

        UpdatePosition();
    }

    public void ChangeCamera(Camera newCamera)
    {
        StartCoroutine(Job(() => WaitForEndOfFrameRoutine(), () => targetCamera = newCamera));
    }

    protected void UpdatePosition()
    {
        Vector2 viewportPoint = targetCamera.WorldToViewportPoint(Position);
        Vector2 canvasSize = mainCanvasRectTransform.sizeDelta;

        Vector2 targetPoint = canvasSize * viewportPoint;
        Vector2 newPosition = new Vector2(targetPoint.x, targetPoint.y) + ViewportPositionAdjust() * canvasSize + ScreenPositionAdjust();

        if(rectTransform != null)
            rectTransform.anchoredPosition = newPosition;
    }

    protected virtual Vector2 ViewportPositionAdjust()
    {
        return new Vector2();
    }

    protected virtual Vector2 ScreenPositionAdjust()
    {
        return new Vector2();
    }

}
