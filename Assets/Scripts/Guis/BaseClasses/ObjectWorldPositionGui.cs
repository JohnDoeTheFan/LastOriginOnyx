using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectWorldPositionGui : WorldPositionGui
{
    [SerializeField]
    private Transform targetTransform;
    protected override Vector3 Position => targetTransform.position;

    protected void Initialize(Camera targetCamera, RectTransform mainCanvasTransform, Transform targetTransform)
    {
        this.targetTransform = targetTransform;

        Initialize(targetCamera, mainCanvasTransform);
    }
}
