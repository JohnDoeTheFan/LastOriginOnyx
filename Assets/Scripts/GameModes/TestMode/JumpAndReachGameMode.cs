using Onyx.GameElement;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Onyx;
using Onyx.Input;

public class JumpAndReachGameMode : MonoBehaviourBase
{
    public Transform gameStartPosition;
    public bool playerControlEnable = true;

    private void Awake()
    {
        InitInputHandler();
        InitUnit();
        InitTriggerVolume();
    }

    protected virtual void InitInputHandler()
    {
        InputHandler.GetPermission = type =>
        {
            return type switch
            {
                InputHandler.Type.Player => playerControlEnable,
                InputHandler.Type.Ai => playerControlEnable,
                _ => false
            };
        };
    }

    protected virtual void InitUnit()
    {
        void OnDeath(MyUnit myUnit)
        {
            if (myUnit.CompareTag("Player"))
                StartCoroutine(GameOverAndRespawnCoroutine(myUnit));
        }

        MyUnit.OnEndOfStart = newUnit => newUnit.SubscribeManager.Subscribe(new MyUnitGameRule(OnDeath));
    }

    protected virtual void InitTriggerVolume()
    {
        TriggerVolume.OnEnterTriggerVolume = (type, gameObject) =>
        {
            MyUnit myUnit = gameObject.GetComponent<MyUnit>();
            if (myUnit != null)
            {
                switch (type)
                {
                    case TriggerVolume.Type.Death:
                        myUnit.Die();
                        break;
                    case TriggerVolume.Type.Win:
                        if (myUnit.CompareTag("Player"))
                        {
                            myUnit.Ceremony();
                            StartCoroutine(GameWinCoroutine());
                        }
                        break;
                    default:
                        break;
                }
            }
        };
    }

    protected IEnumerator GameOverAndRespawnCoroutine(MyUnit myUnit)
    {
        playerControlEnable = false;
        Debug.Log("GameOver");
        yield return new WaitForSeconds(1.5f);
        myUnit.TeleportAt(gameStartPosition.transform.position);
        playerControlEnable = true;
        myUnit.Revive();
    }

    IEnumerator GameWinCoroutine()
    {
        playerControlEnable = false;
        Debug.Log("GameWin");
        yield return new WaitForSeconds(3);
        MyGameInstance.instance.hasCleared = true;
        SceneManager.LoadScene(0);
    }

    class MyUnitGameRule : MyUnit.ISubscriber
    {
        private readonly Action<MyUnit> onDeath;

        public MyUnitGameRule(Action<MyUnit> onDeath)
        {
            this.onDeath = onDeath;
        }

        void MyUnit.ISubscriber.OnDeath(MyUnit myUnit)
        {
            onDeath(myUnit);
        }

        void MyUnit.ISubscriber.OnDamage(MyUnit myUnit, float damage) { }
        void MyUnit.ISubscriber.OnHeal(MyUnit myUnit, float heal) { }
        void MyUnit.ISubscriber.OnHealthPointChange(MyUnit myUnit) { }
    }

}
