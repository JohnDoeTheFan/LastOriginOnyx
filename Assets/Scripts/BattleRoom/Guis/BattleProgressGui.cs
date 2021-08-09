using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Onyx.BattleRoom;

public class BattleProgressGui : MonoBehaviour, BattleRoom.ISubscriber
{
    [SerializeField]
    private BattleObjectiveRecordListGui battleObjectiveList;
    [SerializeField]
    private AudioSource sfxAudioSource;

    private IDisposable unsubscriber;
    private Animator animator;

    private BattleObjectiveRecordGui killRecord;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        sfxAudioSource = GetComponent<AudioSource>();
    }

    public void UnsetBattleRoom()
    {
        unsubscriber?.Dispose();
        animator.SetBool("Expand", false);
        killRecord = null;
        battleObjectiveList.ClearRecords();
    }

    public void SetBattleRoom(BattleRoom battleRoom)
    {
        gameObject.SetActive(true);
        UnsetBattleRoom();

        unsubscriber = battleRoom.SubscribeManager.Subscribe(this);
        if(battleRoom.WholeEnemyCount > 0)
        {
            if(battleRoom.RemainEnemyCount == 0)
                animator.SetBool("Expand", true);

            killRecord = battleObjectiveList.AddRecord("모든 철충을 제거");
            killRecord.UpdateProgress(battleRoom.WholeEnemyCount - battleRoom.RemainEnemyCount, battleRoom.WholeEnemyCount);
        }

        if (battleObjectiveList.IsEmpty)
            gameObject.SetActive(false);
    }

    void BattleRoom.ISubscriber.OnUpdateBattleMission(BattleRoom battleRoom)
    {
        if(killRecord != null)
            killRecord.UpdateProgress(battleRoom.WholeEnemyCount - battleRoom.RemainEnemyCount, battleRoom.WholeEnemyCount);
    }

    void BattleRoom.ISubscriber.OnEnter(BattleRoom battleRoom, bool shouldPlayBriefing)
    {
    }

    void BattleRoom.ISubscriber.OnExit(BattleRoomPortal exitingPortal, GameObject user)
    {
        UnsetBattleRoom();
    }

    void BattleRoom.ISubscriber.BeforeDestroy(BattleRoomPortal battleRoomPortal)
    {
    }

    void BattleRoom.ISubscriber.OnClearBattleRoom(BattleRoom battleRoom)
    {
        animator.SetBool("Expand", true);
        animator.SetTrigger("JustCleared");
        sfxAudioSource.Play();
    }
}
