using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Onyx.BattleRoom;

public class StopEnemiesBattleRoom : BattleRoom
{
    protected override void OnStart()
    {
        foreach (var target in destroyTargets)
        {
            target.PreventControl(true);
            target.gameObject.SetActive(false);
        }
    }

    protected override void OnEnterViaPortal()
    {
        foreach (var target in destroyTargets)
        {
            target.PreventControl(false);
            target.gameObject.SetActive(true);
        }
    }
}
