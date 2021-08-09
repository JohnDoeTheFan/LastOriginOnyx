using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugGui : MonoBehaviour
{
    public interface IDebugGuiTarget
    {
        bool GetPlayerInputPermission();
        bool GetAiInputPermission();
    }
    private IDebugGuiTarget debugGuiTarget;
    public void SetDebugGuiTarget(IDebugGuiTarget debugGuiTarget)
    {
        this.debugGuiTarget = debugGuiTarget;
    }


    public Text playerInputPermission;
    public Text aiInputPermission;

    private void Update()
    {
        if(debugGuiTarget != null)
        {
            playerInputPermission.text = debugGuiTarget.GetPlayerInputPermission() ? "true" : "false";
            aiInputPermission.text = debugGuiTarget.GetAiInputPermission() ? "true" : "false";
        }
    }
}
