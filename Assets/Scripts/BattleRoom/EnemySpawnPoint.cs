using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Onyx;

public class EnemySpawnPoint : MonoBehaviour
{
    [SerializeField] MyUnit enemyToSpawn;
    [SerializeField] bool shouldFaceRight;

    public MyUnit Spawn()
    {
        MyUnit spawnedEnemy = Instantiate<MyUnit>(enemyToSpawn, transform.position, Quaternion.identity);

        if (shouldFaceRight)
            spawnedEnemy.RotateUnit(Vector2.left);

        return spawnedEnemy;
    }
}
