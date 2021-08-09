using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixedWorldPositionGui : WorldPositionGui
{
    [SerializeField]
    private Vector3 position;
    protected override Vector3 Position => position;

    protected void Initialize(Camera targetCamera, RectTransform mainCanvasTransform, Vector3 position)
    {
        this.position = position;

        base.Initialize(targetCamera, rectTransform.parent.GetComponent<RectTransform>());
    }
}
