using Onyx.Ai;
using Onyx.Input;
using Onyx.GameElement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunSlingerAi : AiBase
{
    [Header("GunSlingerAi")]
    [SerializeField] private float attackRange;
    [SerializeField] private float patrolMoveProbability;
    [SerializeField] private Vector2 patrolTimeMinMax;
    [SerializeField] private Vector2 hesitateTimeMinMax;
    [SerializeField] GroundChecker groundCheckerToStop;

    private float patrolTimer;
    private float contactHesitateTimer;
    private float actionHestitateTimer;
    private bool isPatrolMoveRight;
    private PatrolStatus patrolStatus = PatrolStatus.Think;

    protected override void OnUpdate()
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

        if (actionHestitateTimer > 0)
        {
            ReduceActionHesitateTimer();
        }
        else if (Vector2.Distance(modelTransform.position, target.transform.position) < attackRange)
        {
            if (contactHesitateTimer > 0)
            {
                ReduceContactHesitateTimer();
            }
            else
            {
                bool shouldShootLeft = target.transform.position.x < modelTransform.position.x;
                bool isHeadingRight = modelTransform.rotation.y == 0;
                bool isHeadingLeft = Mathf.Abs(modelTransform.rotation.y) == 1;

                if (shouldShootLeft && isHeadingRight)
                {
                    virtualInputSourceAsController.SetLeftStick(Vector2.left);
                    SetActionHesitateTimer();
                }
                else if (!shouldShootLeft && isHeadingLeft)
                {
                    virtualInputSourceAsController.SetLeftStick(Vector2.right);
                    SetActionHesitateTimer();
                }
                else
                {
                    virtualInputSourceAsController.SetButtonDown(InputSource.Button.R2);
                    SetActionHesitateTimer();
                }
            }
        }
        else
        {
            bool shouldMoveLeft = target.transform.position.x < modelTransform.position.x;

            bool isHeadingRight = modelTransform.rotation.y == 0;
            bool isHeadingLeft = Mathf.Abs(modelTransform.rotation.y) == 1;
            bool isRightEmpty = isHeadingRight && !groundCheckerToStop.IsGrounded;
            bool isLeftEmpty = isHeadingLeft && !groundCheckerToStop.IsGrounded;

            if (shouldMoveLeft && !isLeftEmpty)
                virtualInputSourceAsController.SetLeftStick(Vector2.left);
            else if (!shouldMoveLeft && !isRightEmpty)
                virtualInputSourceAsController.SetLeftStick(Vector2.right);
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
            bool isHeadingRight = modelTransform.rotation.y == 0;
            bool isHeadingLeft = Mathf.Abs(modelTransform.rotation.y) == 1;
            bool isRightEmpty = isHeadingRight && !groundCheckerToStop.IsGrounded;
            bool isLeftEmpty = isHeadingLeft && !groundCheckerToStop.IsGrounded;

            if (isPatrolMoveRight && !isRightEmpty)
                virtualInputSourceAsController.SetLeftStick(Vector2.right);
            else if(!isPatrolMoveRight && !isLeftEmpty)
                virtualInputSourceAsController.SetLeftStick(Vector2.left);
        }

        ReducePatrolTimer();

        if (patrolTimer == 0)
            patrolStatus = PatrolStatus.Think;
    }

    private Decision Think()
    {
        if (enemy == null)
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
