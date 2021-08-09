using Onyx;
using Onyx.GameElement;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RunAndGunGameMode : JumpAndReachGameMode, DebugGui.IDebugGuiTarget
{
    [Header("Instances", order = 1)]
    public DialogueDeck startingDialogue;
    public Camera mainCamera;

    [Header("Guis")]
    public DebugGui debugGui;
    public FadeGui fadeGui;
    public RectTransform pauseMenuGui;
    public Text scoreGui;
    public ValueBarGui playerDurabilityGui;
    public GunGui gunGui;
    public OwningBulletsGui owningAvailableBulletsGui;
    public OwningBulletsGui owningNotAvailableBulletsGui;
    public Canvas screenCanvas;

    [Header("GuiPrefabs")]
    public HealthPointChangeGui damageGuiPrefab;
    public HealthPointChangeGui healGuiPrefab;
    public RectTransform GameOverGuiPrefab;
    public YouWinGui youWinGuiPrefab;

    private int score;
    protected bool isGameOvered = false;
    private float timeScaleBackupOnPause;

    protected int Score { set { score = value; if (scoreGui != null) scoreGui.text = Score.ToString(); } get { return score; } }

    private void Awake()
    {
        InitInputHandler();
        InitUnit();
        InitTriggerVolume();
        InitGunSlinger();
    }

    protected virtual void Start()
    {
        if (debugGui != null)
            debugGui.SetDebugGuiTarget(this);

        playerControlEnable = false;

        fadeGui.StartFadeIn(AfterFadeOut);

        void AfterFadeOut(FadeGui.FinishStatus action)
        {
            if (startingDialogue != null)
                GetComponent<DialogueBehaviour>().StartDialogueDeck(startingDialogue, AfterFirstDialog);
            else
                playerControlEnable = true;
        }

        void AfterFirstDialog()
        {
            playerControlEnable = true;
        }
    }

    private void Update()
    {
        if(isGameOvered)
        {
            if(Input.anyKeyDown)
            {
                SceneManager.LoadScene(0);
            }
        }

        if (Input.GetButtonDown("Cancel"))
        {
            if (Time.timeScale == 0f)
                ResumeGame();
            else
                PauseGame();
        }
    }

    protected override void InitUnit()
    {
        MyUnit.OnEndOfStart = myUnit =>
        {
            if (myUnit.CompareTag("Player"))
            {
                myUnit.SubscribeManager.Subscribe(new PlayerUnitSubscriber(OnDeathPlayerUnit, OnUnitDamageOrHeal, playerDurabilityGui));
                playerDurabilityGui.SetValue(myUnit.HealthPoint);
            }
            else
                myUnit.SubscribeManager.Subscribe(new UnitSubscriber(OnDeathEnemyUnit, OnUnitDamageOrHeal));
        };
    }

    protected override void InitTriggerVolume()
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
                            playerControlEnable = false;
                            myUnit.Ceremony();
                            StartCoroutine(Job(()=>WaitForSecondsRoutine(1f), AfterCeremony));
                            void AfterCeremony()
                            {
                                YouWinGui newYouWinGui = Instantiate<YouWinGui>(youWinGuiPrefab, screenCanvas.transform);
                                newYouWinGui.SetScore(score);
                                newYouWinGui.SetAfterDisplayScoreAction(()=>isGameOvered = true);
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
        };
    }

    protected virtual void InitGunSlinger()
    {
        GunSlinger.OnStart = gunSlinger =>
        {
            if (gunSlinger.CompareTag("Player"))
            {
                PlayerGunSlingerSubscriber gunSlingerSubscriber = new PlayerGunSlingerSubscriber(gunSlinger, gunGui);

                if(owningAvailableBulletsGui != null)
                    owningAvailableBulletsGui.SetGunSlinger(gunSlinger);
                if(owningNotAvailableBulletsGui != null)
                    owningNotAvailableBulletsGui.SetGunSlinger(gunSlinger);
            }
        };
    }

    public void ResumeGame()
    {
        pauseMenuGui.gameObject.SetActive(false);
        Time.timeScale = timeScaleBackupOnPause;
    }

    public void PauseGame()
    {
        if( ! isGameOvered)
        { 
            pauseMenuGui.gameObject.SetActive(true);
            timeScaleBackupOnPause = Time.timeScale;
            Time.timeScale = 0f;
        }
    }

    protected virtual void OnDeathPlayerUnit(MyUnit myUnit)
    {
        Instantiate<RectTransform>(GameOverGuiPrefab, screenCanvas.transform);
        playerControlEnable = false;
        isGameOvered = true;
    }

    protected virtual void OnDeathEnemyUnit(MyUnit myUnit)
    {
        Score += 100;
        StartCoroutine(Job(() => WaitForSecondsRoutine(1.5f), () => Destroy(myUnit.gameObject)));
    }

    protected virtual void OnUnitDamageOrHeal(MyUnit unit, float value)
    {
        Bounds bounds = new Bounds();
        BoxCollider2D collider = unit.GetComponent<BoxCollider2D>();
        if(collider != null)
            bounds = collider.bounds;
        else
        {
            SpriteRenderer spriteRenderer = unit.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
                bounds = spriteRenderer.bounds;
        }

        Vector3 unitPosition = bounds.center + new Vector3(0, bounds.extents.y, 0);
        Vector3 DisplayPosition = unitPosition + UnityEngine.Random.insideUnitSphere.normalized * bounds.extents.x;

        DisplayDamageOrHealGui(value, DisplayPosition);
    }

    void DisplayDamageOrHealGui(float value, Vector3 position)
    {
        HealthPointChangeGui guiToInstantiate = damageGuiPrefab;
        if (value >= 0)
            guiToInstantiate = healGuiPrefab;

        HealthPointChangeGui DamageOrHealGui = Instantiate<HealthPointChangeGui>(guiToInstantiate, screenCanvas.transform);
        DamageOrHealGui.Initialize(mainCamera, position, Mathf.RoundToInt(value * 100));
    }

    bool DebugGui.IDebugGuiTarget.GetPlayerInputPermission()
    {
        return playerControlEnable;
    }

    bool DebugGui.IDebugGuiTarget.GetAiInputPermission()
    {
        return playerControlEnable;
    }

    protected class UnitSubscriber : MyUnit.ISubscriber
    {
        readonly Action<MyUnit> onDeath;
        readonly Action<MyUnit, float> onDamageOrHeal;
        public UnitSubscriber(Action<MyUnit> onDeath, Action<MyUnit, float> onDamageOrHeal)
        {
            this.onDeath = onDeath;
            this.onDamageOrHeal = onDamageOrHeal;
        }

        public void OnDeath(MyUnit myUnit)
        {
            onDeath(myUnit);
        }

        public void OnDamage(MyUnit myUnit, float damage)
        {
            onDamageOrHeal(myUnit, damage * -1);
        }
        public void OnHeal(MyUnit myUnit, float heal)
        {
            onDamageOrHeal(myUnit, heal);
        }

        public virtual void OnHealthPointChange(MyUnit myUnit) { }
    }

    protected class PlayerUnitSubscriber : UnitSubscriber
    {
        readonly ValueBarGui playerDurabilityGui;

        public PlayerUnitSubscriber(Action<MyUnit> onDeath, Action<MyUnit, float> onDamageOrHeal, ValueBarGui playerDurabilityGui) : base(onDeath, onDamageOrHeal)
        {
            this.playerDurabilityGui = playerDurabilityGui;
        }

        public override void OnHealthPointChange(MyUnit myUnit)
        {
            playerDurabilityGui.SetValue(myUnit.HealthPoint);
        }
    }

    protected class PlayerGunSlingerSubscriber : GunSlinger.ISubscriber
    {
        readonly GunGui gunGui;
        Gun2D lastClosestGun;
        public PlayerGunSlingerSubscriber(GunSlinger gunSlinger, GunGui gunGui)
        {
            gunSlinger.SubscribeManager.Subscribe(this);
            this.gunGui = gunGui;
        }

        void GunSlinger.ISubscriber.AfterUnequip(GunSlinger gunSlinger, Gun2D gun)
        {
            if (gunGui != null)
                gunGui.EmptyGun();
        }

        void GunSlinger.ISubscriber.AfterEquip(GunSlinger gunSlinger, Gun2D gun)
        {
            if(gunGui != null)
                gunGui.SetGun(gun);
        }

        void GunSlinger.ISubscriber.OnUpdateBullets(GunSlinger gunSlinger) { }
        void GunSlinger.ISubscriber.BeforeDestroy(GunSlinger gunSlinger) { }
        void GunSlinger.ISubscriber.OnUpdateReloadTime(GunSlinger gunSlinger) { }
        void GunSlinger.ISubscriber.OnChangedClosestGun(GunSlinger gunSlinger, Gun2D closestGun)
        {
            if(lastClosestGun != null)
                lastClosestGun.SetFocus(false);

            lastClosestGun = closestGun;
            if (lastClosestGun != null)
                lastClosestGun.SetFocus(true);
        }
    }
}