using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Onyx.BattleRoom
{
    public class BattleRoom : MonoBehaviour
    {
        static public Action<BattleRoom> OnStart;

        static public Action<BattleRoom, bool> OnEnter;
        static public Action<Action> OnExit;

        [SerializeField]
        private Grid grid;
        [SerializeField]
        private Tilemap tileMapComponent;
        [SerializeField]
        private GameObject background;
        [SerializeField]
        private List<MyUnit> destroyTargets;
        [SerializeField]
        private BattleRoomPortal enterPortal;
        [SerializeField]
        private BattleRoomPortal exitPortal;
        [SerializeField]
        private bool isFirstEntering = true;

        readonly private UnsubscriberPack destroyTargetSubscribers = new UnsubscriberPack();
        private int remainEnemyCount;
        private int wholeEnemyCount;

        public int RemainEnemyCount => remainEnemyCount;
        public int WholeEnemyCount => wholeEnemyCount;
        public GameObject Background => background;
        public SubscribeManagerTemplate<ISubscriber> SubscribeManager { private set; get; } = new SubscribeManagerTemplate<ISubscriber>();

        private void OnDestroy()
        {
            destroyTargetSubscribers.UnsubscribeAll();
        }

        private void Start()
        {
            OnStart(this);

            remainEnemyCount = destroyTargets.Count;
            wholeEnemyCount = remainEnemyCount;
            foreach (var target in destroyTargets)
            {
                destroyTargetSubscribers.Add(new DestoryTargetSubscriber(target, ReduceRemainEnemyCount));
                target.PreventControl(true);
            }

            if (enterPortal != null)
            {
                enterPortal.SetOnExit(OnTryToExitViaPortal);
                enterPortal.SetOnEnter(EnterViaPortal);
            }
            if (exitPortal != null)
            {
                if (remainEnemyCount != 0)
                    exitPortal.gameObject.SetActive(false);
                exitPortal.SetOnExit(OnTryToExitViaPortal);
                exitPortal.SetOnEnter(EnterViaPortal);
            }

            foreach (var target in destroyTargets)
                target.gameObject.SetActive(false);
        }

        public Vector2 CalcTileMapSize()
        {
            return new Vector2(grid.cellSize.x * tileMapComponent.size.x, grid.cellSize.x * tileMapComponent.size.y);
        }

        public MinMax2 CalcTileMapBound()
        {
            Vector3 gridPosition = tileMapComponent.transform.position;
            BoundsInt gridBounds = tileMapComponent.cellBounds;
            Vector3 cellSize = tileMapComponent.cellSize;

            Vector2 gridMin = new Vector2(gridPosition.x + gridBounds.xMin * cellSize.x, gridPosition.y + gridBounds.yMin * cellSize.y);
            Vector2 gridMax = new Vector2(gridPosition.x + gridBounds.xMax * cellSize.x, gridPosition.y + gridBounds.yMax * cellSize.y);

            return new MinMax2(gridMin, gridMax);
        }

        private void EnterViaPortal(BattleRoomPortal portal)
        {
            bool shouldPlayBriefing = isFirstEntering;
            isFirstEntering = false;

            SubscribeManager.ForEach(item => item.OnEnter(this, shouldPlayBriefing));

            foreach (var target in destroyTargets)
                target.PreventControl(false);

            enterPortal.gameObject.SetActive(false);

            foreach (var target in destroyTargets)
                target.gameObject.SetActive(true);
        }

        private void OnTryToExitViaPortal(BattleRoomPortal exitingPortal, GameObject user)
        {
            SubscribeManager.ForEach(item => item.OnExit(exitingPortal, user));
        }

        public void AttachRoom(BattleRoom nextRoom)
        {
            exitPortal.SetDestinationPortal(nextRoom.enterPortal);
            nextRoom.enterPortal.SetDestinationPortal(exitPortal);
        }

        private void ReduceRemainEnemyCount()
        {
            remainEnemyCount--;
            SubscribeManager.ForEach(item => item.OnUpdateBattleMission(this));
            if (remainEnemyCount == 0)
            {
                Clear();
            }
        }

        private void Clear()
        {
            SubscribeManager.ForEach(item => item.OnClearBattleRoom(this));
            exitPortal.gameObject.SetActive(true);
        }

        public interface ISubscriber
        {
            void OnEnter(BattleRoom battleRoom, bool shouldPlayBriefing);
            void OnExit(BattleRoomPortal exitingPortal, GameObject user);
            void OnUpdateBattleMission(BattleRoom battleRoom);
            void OnClearBattleRoom(BattleRoom battleRoom);
            void BeforeDestroy(BattleRoomPortal battleRoomPortal);
        }

        private class DestoryTargetSubscriber : UniUnsubscriber, MyUnit.ISubscriber
        {
            readonly Action onDeath;

            public DestoryTargetSubscriber(MyUnit target, Action onDeath)
            {
                InitUniSubscriber(target.SubscribeManager.Subscribe(this));
                this.onDeath = onDeath;
            }

            void MyUnit.ISubscriber.OnDeath(MyUnit myUnit)
            {
                Unsubscribe();
                onDeath();
            }

            void MyUnit.ISubscriber.OnDamage(MyUnit myUnit, float damage)
            {
            }

            void MyUnit.ISubscriber.OnHeal(MyUnit myUnit, float heal)
            {
            }

            void MyUnit.ISubscriber.OnHealthPointChange(MyUnit myUnit)
            {
            }
        }
    }

}
