using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldSpaceCanvasCameraSetter : MonoBehaviour
{
    static public Func<Camera> GetCamera;

    private void Start()
    {
        GetComponent<Canvas>().worldCamera = GetCamera();
    }
}
