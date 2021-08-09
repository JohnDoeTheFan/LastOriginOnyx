using Onyx.Input;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace Onyx.Ai
{
    public abstract class AiScriptFactoryBase : ScriptableObject
    {
        public abstract AiScriptBase ProductAiScript();

        public abstract class AiScriptBase
        {
            protected Transform transform;
            protected VirtualInputSource controller;
            protected Transform enemy;
            protected Transform finalObjective;
            protected ReadOnlyCollection<Transform> objectsInSight;

            protected Transform ObjectiveOfTarget;

            public Transform Enemy => enemy;
            public Transform FinalObjective => finalObjective;

            public void SetTransform(Transform transform)
            {
                this.transform = transform;
            }
            public void SetContoller(VirtualInputSource controller)
            {
                this.controller = controller;
            }
            public void SetFinalObjective(Transform finalObjective)
            {
                this.finalObjective = finalObjective;
            }

            public abstract void OnUpdate();

            public virtual void OnEnterSight(GameObject enteringObject)
            {
                if (finalObjective != null)
                {
                    AiPlayer enteringAiPlayer = enteringObject.GetComponent<AiPlayer>();

                    AiScriptBase enteringAiScript = null;
                    if (enteringAiPlayer != null)
                        enteringAiScript = enteringAiPlayer.AiScript;

                    if (enteringAiScript != null)
                    {

                        Transform objectiveOfTarget = enteringAiScript.FinalObjective;
                        if (objectiveOfTarget != null && objectiveOfTarget != finalObjective)
                        {
                            if (enemy == null)
                            {
                                enemy = enteringObject.transform;
                            }
                            else
                            {
                                float currentTargetDistance = Vector2.Distance(transform.position, enemy.transform.position);
                                float enteringTargetDistance = Vector2.Distance(transform.position, enteringObject.transform.position);
                                if (enteringTargetDistance < currentTargetDistance)
                                {
                                    enemy = enteringObject.transform;
                                }
                            }
                        }
                    }

                }
                else if (enteringObject.CompareTag("Player"))
                    enemy = enteringObject.transform;
            }
            public virtual void OnExitSight(GameObject exitingObject)
            {
                if (exitingObject.transform == enemy)
                    enemy = null;
            }
        }

    }
}