using Onyx.Ai;
using Onyx.Input;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShielderAi : AiBase
{
    [SerializeField] private Vector2 hesitateTimeMinMax;

    private float actionHestitateTimer;
    private Vector2 preservedDirection;

    protected override void OnUpdate()
    {
        if (actionHestitateTimer > 0)
        {
            ReduceActionHesitateTimer();
            if (actionHestitateTimer == 0)
                virtualInputSourceAsController.SetLeftStick(preservedDirection);
        }
        else if (enemy != null)
        {
            Transform target = enemy;

            bool shouldHeadLeft = target.transform.position.x < modelTransform.position.x;
            bool isHeadingRight = modelTransform.rotation.y == 0;
            bool isHeadingLeft = Mathf.Abs(modelTransform.rotation.y) == 1;

            if (shouldHeadLeft && isHeadingRight)
            {
                preservedDirection = Vector2.left;
                SetActionHesitateTimer();
            }
            else if (!shouldHeadLeft && isHeadingLeft)
            {
                preservedDirection = Vector2.right;
                SetActionHesitateTimer();
            }
        }
    }

    private void SetActionHesitateTimer()
    {
        actionHestitateTimer = UnityEngine.Random.Range(hesitateTimeMinMax.x, hesitateTimeMinMax.y);
    }

    private void ReduceActionHesitateTimer()
    {
        actionHestitateTimer = Mathf.Max(0, actionHestitateTimer - Time.deltaTime);
    }
}
