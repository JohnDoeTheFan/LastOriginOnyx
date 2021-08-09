using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Onyx.Ai
{
    [CreateAssetMenu(menuName = "Utility/Ai/Shielder")]
    public class ShielderAiScriptFactory : AiScriptFactoryBase
    {
        [SerializeField]
        private Vector2 hesitateTimeMinMax = new Vector2(0.5f, 0.8f);

        public override AiScriptBase ProductAiScript()
        {
            return new ShielderAiScript(hesitateTimeMinMax);
        }

        public class ShielderAiScript : AiScriptBase
        {
            private readonly Vector2 hesitateTimeMinMax;

            private float actionHestitateTimer;
            private Vector2 preservedDirection;

            public ShielderAiScript(Vector2 hesitateTimeMinMax)
            {
                this.hesitateTimeMinMax = hesitateTimeMinMax;
            }

            public override void OnUpdate()
            {
                if (actionHestitateTimer > 0)
                {
                    ReduceActionHesitateTimer();
                    if(actionHestitateTimer == 0)
                        controller.SetLeftStick(preservedDirection);
                }
                else if (finalObjective != null || enemy != null)
                {
                    Transform target = enemy;
                    if (target == null)
                        target = finalObjective;

                    bool shouldHeadLeft = target.transform.position.x < transform.position.x;
                    bool isHeadingRight = transform.rotation.y == 0;
                    bool isHeadingLeft = Mathf.Abs(transform.rotation.y) == 1;

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
    }
}