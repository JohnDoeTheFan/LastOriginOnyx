using Onyx.Ai;
using Onyx.Input;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatersAi : AiBase
{
    [SerializeField] List<Transform> patrolTransforms;
    [SerializeField, Range(0.1f, 1)] float accuracy = 0.5f;
    [SerializeField] private Vector2 hesitateTimeMinMax;

    private int currentIndex;
    private List<Vector2> patrolPositions = new List<Vector2>();
    private PatrolStatus patrolStatus = PatrolStatus.Wait;
    private float waitHesitateTimer;


    protected override void Start()
    {
        base.Start();

        foreach(Transform patrolTransform in patrolTransforms)
            patrolPositions.Add(patrolTransform.position);

        patrolPositions.Add(transform.position);
    }

    protected override void OnUpdate()
    {
        Decision decision = Think();

        if (decision == Decision.Patrol)
        {
            DoPatrol();
        }
        else if (decision == Decision.Attack)
        {
            DoAttack();
        }
    }

    private void DoAttack()
    {
        virtualInputSourceAsController.SetButtonDown(InputSource.Button.R2);
    }

    private void DoPatrol()
    {
        if(patrolPositions.Count != 0)
        {
            if (patrolStatus == PatrolStatus.Wait)
            {
                if (waitHesitateTimer > 0)
                    ReduceWaitHesitateTimer();
                else
                {
                    patrolStatus = PatrolStatus.Move;
                    currentIndex++;

                    if (patrolPositions.Count <= currentIndex)
                        currentIndex = 0;
                }
            }
            else
            {
                Vector2 position2d = transform.position;
                Vector2 distanceDiff = patrolPositions[currentIndex] - position2d;

                if(distanceDiff.magnitude < accuracy)
                {
                    patrolStatus = PatrolStatus.Wait;
                    SetWaitHesitateTimer();
                }
                else
                    virtualInputSourceAsController.SetLeftStick(distanceDiff.normalized);
            }
        }
    }

    private void SetWaitHesitateTimer()
    {
        waitHesitateTimer = UnityEngine.Random.Range(hesitateTimeMinMax.x, hesitateTimeMinMax.y);
    }

    private void ReduceWaitHesitateTimer()
    {
        waitHesitateTimer = Mathf.Max(0, waitHesitateTimer - Time.deltaTime);
    }

    private Decision Think()
    {
        if (enemy == null)
            return Decision.Patrol;
        else
            return Decision.Attack;
    }

    enum Decision
    {
        Patrol,
        Attack,
    }

    enum PatrolStatus
    {
        Wait,
        Move
    }

}
