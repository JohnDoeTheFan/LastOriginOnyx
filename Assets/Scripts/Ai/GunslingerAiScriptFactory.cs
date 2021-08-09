using Onyx.Input;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Onyx.Ai
{
    [CreateAssetMenu(menuName = "Utility/Ai/GunSlinger")]
    public class GunslingerAiScriptFactory : AiScriptFactoryBase
    {
        [SerializeField]
        private float attackRange;
        [SerializeField]
        private float patrolMoveProbability = 0.3f;
        [SerializeField]
        private Vector2 patrolTimeMinMax = new Vector2(0.8f, 1f);
        [SerializeField]
        private Vector2 hesitateTimeMinMax = new Vector2(0.5f, 0.8f);

        public override AiScriptBase ProductAiScript()
        {
            return new GunSlingerAiScript(attackRange, patrolMoveProbability, patrolTimeMinMax, hesitateTimeMinMax);
        }

        public class GunSlingerAiScript : AiScriptBase
        {
            private readonly float attackRange;
            private readonly float patrolMoveProbability;
            private readonly Vector2 patrolTimeMinMax;
            private readonly Vector2 hesitateTimeMinMax;

            private float patrolTimer;
            private float contactHesitateTimer;
            private float actionHestitateTimer;
            private bool isPatrolMoveRight;
            private PatrolStatus patrolStatus = PatrolStatus.Think;

            public GunSlingerAiScript(float attackRange, float patrolMoveProbability, Vector2 patrolTimeMinMax, Vector2 hesitateTimeMinMax)
            {
                this.attackRange = attackRange;
                this.patrolMoveProbability = patrolMoveProbability;
                this.patrolTimeMinMax = patrolTimeMinMax;
                this.hesitateTimeMinMax = hesitateTimeMinMax;
            }

            public override void OnUpdate()
            {
                Decision decision = Think();

                if (decision == Decision.Patrol)
                {
                    DoPatrol();
                    SetContactHesitateTimer();
                }
                else if (decision == Decision.Chase)
                {
                    DoChase();
                }
            }

            public void DoChase()
            {
                Transform target = enemy;
                if (target == null)
                    target = finalObjective;

                if(actionHestitateTimer > 0)
                {
                    ReduceActionHesitateTimer();
                }
                else if (Vector2.Distance(transform.position, target.transform.position) < attackRange)
                {
                    if (contactHesitateTimer > 0)
                    {
                        ReduceContactHesitateTimer();
                    }
                    else
                    {
                        bool shouldShootLeft = target.transform.position.x < transform.position.x;
                        bool isHeadingRight = transform.rotation.y == 0;
                        bool isHeadingLeft = Mathf.Abs(transform.rotation.y) == 1;

                        if (shouldShootLeft && isHeadingRight)
                        {
                            controller.SetLeftStick(Vector2.left);
                            SetActionHesitateTimer();
                        }
                        else if (!shouldShootLeft && isHeadingLeft)
                        {
                            controller.SetLeftStick(Vector2.right);
                            SetActionHesitateTimer();
                        }
                        else
                        {
                            controller.SetButtonDown(InputSource.Button.R2);
                            SetActionHesitateTimer();
                        }
                    }
                }
                else
                {
                    bool shouldMoveLeft = target.transform.position.x < transform.position.x;

                    if (shouldMoveLeft)
                        controller.SetLeftStick(Vector2.left);
                    else
                        controller.SetLeftStick(Vector2.right);
                }
            }

            public void DoPatrol()
            {
                if (patrolStatus == PatrolStatus.Think)
                {
                    if (UnityEngine.Random.value < patrolMoveProbability)
                    {
                        patrolStatus = PatrolStatus.Move;
                        isPatrolMoveRight = RandomBool(); 
                        SetPatrolTimer();
                    }
                    else
                    {
                        patrolStatus = PatrolStatus.Wait;
                        SetPatrolTimer();
                    }
                }
                else if (patrolStatus == PatrolStatus.Move)
                {
                    if (isPatrolMoveRight)
                        controller.SetLeftStick(Vector2.right);
                    else
                        controller.SetLeftStick(Vector2.left);
                }

                ReducePatrolTimer();

                if (patrolTimer == 0)
                    patrolStatus = PatrolStatus.Think;
            }

            private Decision Think()
            {
                if (finalObjective == null && enemy == null)
                    return Decision.Patrol;
                else
                    return Decision.Chase;
            }

            private bool RandomBool()
            {
                return UnityEngine.Random.value < 0.5f;
            }

            private void SetContactHesitateTimer()
            {
                contactHesitateTimer = UnityEngine.Random.Range(hesitateTimeMinMax.x, hesitateTimeMinMax.y);
            }

            private void ReduceContactHesitateTimer()
            {
                contactHesitateTimer = Mathf.Max(0, contactHesitateTimer - Time.deltaTime);
            }

            private void SetActionHesitateTimer()
            {
                actionHestitateTimer = UnityEngine.Random.Range(hesitateTimeMinMax.x, hesitateTimeMinMax.y);
            }

            private void ReduceActionHesitateTimer()
            {
                actionHestitateTimer = Mathf.Max(0, actionHestitateTimer - Time.deltaTime);
            }

            private void SetPatrolTimer()
            {
                patrolTimer = UnityEngine.Random.Range(patrolTimeMinMax.x, patrolTimeMinMax.y);
            }

            private void ReducePatrolTimer()
            {
                patrolTimer = Mathf.Max(0, patrolTimer - Time.deltaTime);
            }

            enum Decision
            {
                Patrol,
                Chase,
            }

            enum PatrolStatus
            {
                Think,
                Wait,
                Move
            }
        }


    }

}