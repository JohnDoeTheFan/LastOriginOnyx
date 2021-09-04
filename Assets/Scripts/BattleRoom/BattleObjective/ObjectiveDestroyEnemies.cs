using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Onyx;

public class ObjectiveDestroyEnemies : BattleObjective
{
    [SerializeField] private List<EnemySpawnPoint> enemySpawnPoints;

    private int remainEnemyCount = 0;
    private UnsubscriberPack unsubscriberPack = new UnsubscriberPack();
    private int wholeEnemyCount = 0;

    public void Start()
    {
        wholeEnemyCount = enemySpawnPoints.Count;
        remainEnemyCount = wholeEnemyCount;
        foreach (EnemySpawnPoint point in enemySpawnPoints)
        {
            Onyx.MyUnit newUnit = point.Spawn();
            unsubscriberPack.Add(new EnemySubscriber(newUnit, OnDeath));

            void OnDeath()
            {
                remainEnemyCount--;

                if (remainEnemyCount == 0)
                {
                    subscribeManager.ForEach((item) => item.OnChangedStatus());
                }
            }
        }
    }

    public void OnDestroy()
    {
        unsubscriberPack.UnsubscribeAll();
    }

    private class EnemySubscriber : UniUnsubscriber, MyUnit.ISubscriber
    {
        Action onDeath;
        IDisposable unsubscriber;
        protected override IDisposable Unsubscriber => throw new NotImplementedException();

        public EnemySubscriber(MyUnit myUnit, Action onDeath)
        {
            unsubscriber = myUnit.SubscribeManager.Subscribe(this);
            this.onDeath = onDeath;
        }

        void MyUnit.ISubscriber.OnDamage(MyUnit myUnit, float damage)
        {
        }

        void MyUnit.ISubscriber.OnDeath(MyUnit myUnit)
        {
            onDeath();
            unsubscriber.Dispose();
        }

        void MyUnit.ISubscriber.OnHeal(MyUnit myUnit, float heal)
        {
        }

        void MyUnit.ISubscriber.OnHealthPointChange(MyUnit myUnit)
        {
        }
    }
}
