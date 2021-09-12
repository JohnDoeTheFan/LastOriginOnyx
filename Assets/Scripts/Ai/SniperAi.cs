using Onyx.Ai;
using Onyx.Input;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SniperAi : AiBase
{
    protected override void OnUpdate()
    {
        virtualInputSourceAsController.SetButtonDown(InputSource.Button.R2);
    }
}
