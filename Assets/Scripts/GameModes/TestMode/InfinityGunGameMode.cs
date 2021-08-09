using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Onyx;

public class InfinityGunGameMode : RunAndGunGameMode
{
    [Header("EnemySpawn", order = 0)]
    [SerializeField]
    protected Transform enemySpawnPosition;

    [SerializeField]
    protected MyUnit enemyPrefab;

    protected override void Start()
    {
        base.Start();
        StartCoroutine(DoActionAfterSeconds(SpawnEnemy, 3));
    }

    void SpawnEnemy()
    {
        Instantiate<MyUnit>(enemyPrefab, enemySpawnPosition.transform.position, Quaternion.identity);
    }

    protected override void OnDeathEnemyUnit(MyUnit enemyUnit)
    {
        Score += 100;
        StartCoroutine(Job(() => WaitForSecondsRoutine(1.5f), () => Destroy(enemyUnit.gameObject)));
        StartCoroutine(Job(() => WaitForSecondsRoutine(3f), SpawnEnemy));
    }

    IEnumerator DoActionAfterSeconds(Action action, float seconds)
    {
        yield return new WaitForSeconds(seconds);
        action();
    }
}
