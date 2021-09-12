using Onyx.Ai;
using Onyx.GameElement;
using Onyx.Input;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Onyx;

public class SideScrollStrategyGameMode : RunAndGunGameMode
{
    public static SideScrollStrategyGameMode gameMode;

    protected override void Start()
    {
        StartCoroutine(SpawnEnemyPerSeconds(3));
        StartCoroutine(AddCostPerSecond());

        InputHandler.GetPermission = type =>
        {
            return type switch
            {
                InputHandler.Type.Player => playerControlEnable,
                InputHandler.Type.Ai => true,
                _ => false,
            };
        };
    }

    private void OnEnable()
    {
        UpdateGuisAboutCost();

        shouldSpawnEnemy = true;
        shouldAddCost = true;
    }

    private void OnDisable()
    {
        shouldSpawnEnemy = false;
    }


    [Header("Guis")]
    public UnitInfoGui unitInfoGui;
    public UnitCreationGui unitCreationGui;
    public UnityEngine.UI.Text costText;

    [Header("Units")]
    public CreatableUnitDesign[] creatableUnits;
    public CreatableEnemyDesign[] creatableEnemies;

    [Header("Instances")]
    public FreeMovement freeMover;
    public Transform enemyBarrack;
    public Transform playerBarrack;
    public Transform enemyGameStart;

    [Header("Variables")]
    public int currentCost;
    public int costPerSeconds;

    private int unitSeqNum;
    private int enemySeqNum;

    private bool shouldSpawnEnemy = false;
    private bool shouldAddCost = false;

    public List<UnitDefinitionGui.UnitDefinition> GetCreatableUnitDefinitions()
    {
        if (creatableUnits != null)
        {
            List<UnitDefinitionGui.UnitDefinition> retVal = new List<UnitDefinitionGui.UnitDefinition>();
            for (int i = 0; i < creatableUnits.Length; i++)
            {
                retVal.Add(
                    new UnitDefinitionGui.UnitDefinition
                    {
                        name = creatableUnits[i].prefab.name,
                        image = (creatableUnits[i].image != null) ? creatableUnits[i].image : creatableUnits[i].prefab.GetComponent<SpriteRenderer>().sprite,
                        cost = creatableUnits[i].cost,
                        coolTime = creatableUnits[i].coolTime
                    }
                    );
            }

            return retVal;
        }
        else
            return new List<UnitDefinitionGui.UnitDefinition>();
    }

    public void OnClickUnitDefinitionGui(UnitDefinitionGui unitDefinitionGui, int relatedCreatableUnitsIndex)
    {
        if(creatableUnits[relatedCreatableUnitsIndex].remainCoolTime != 0f)
        {
            Debug.Log("Still cooling.");
        }
        else if( currentCost < creatableUnits[relatedCreatableUnitsIndex].cost )
        {
            Debug.Log("Not enough cost.");
        }
        else
        {
            MyUnit newUnit = Instantiate<MyUnit>(creatableUnits[relatedCreatableUnitsIndex].prefab, gameStartPosition.transform.position, Quaternion.identity);
            newUnit.gameObject.name = "Unit" + unitSeqNum++;
            // AiBase myBotAi = newUnit.GetComponent<AiBase>();
            // myBotAi.AiScript.SetFinalObjective( enemyBarrack );

            currentCost -= creatableUnits[relatedCreatableUnitsIndex].cost;
            creatableUnits[relatedCreatableUnitsIndex].remainCoolTime = creatableUnits[relatedCreatableUnitsIndex].coolTime;

            UpdateGuisAboutCost();

            StartCoroutine(CoolTimeReductionCoroutine(relatedCreatableUnitsIndex));
            unitDefinitionGui.StartCoolTimeDisplayUpdate();
        }
    }

    IEnumerator CoolTimeReductionCoroutine(int index)
    {
        while(creatableUnits[index].remainCoolTime != 0f)
        {
            yield return new WaitForSeconds(0.03f);
            creatableUnits[index].remainCoolTime = Mathf.Max(0f, creatableUnits[index].remainCoolTime - 0.03f);
        }
    }

    IEnumerator SpawnEnemyPerSeconds(float delay)
    {
        while (true)
        {
            yield return new WaitForSeconds(delay);
            if (shouldSpawnEnemy)
                CreateEnemy();
        }
    }

    IEnumerator AddCostPerSecond()
    {
        while (true)
        {
            yield return new WaitForSeconds(1);
            if(shouldAddCost)
            { 
                currentCost += costPerSeconds;
                UpdateGuisAboutCost();
            }
        }
    }

    private void UpdateGuisAboutCost()
    {
        costText.text = currentCost.ToString();

        unitCreationGui.ForEachUnitDefinitionGui((item) => item.UpdateAvailableDisplay(currentCost));
    }

    public void CreateEnemy()
    {
        int index = Mathf.FloorToInt( UnityEngine.Random.Range(0, creatableEnemies.Length) );
        index = Mathf.Max(index, creatableEnemies.Length - 1);

        if(creatableEnemies[index].currentCount < creatableEnemies[index].maximumCreatableNumber)
        {
            MyUnit newUnit = Instantiate<MyUnit>(creatableEnemies[index].prefab, enemyGameStart.transform.position, Quaternion.identity);
            creatableEnemies[index].currentCount++;
            newUnit.gameObject.name = "Enemy" + enemySeqNum++;

            // AiBase myBotAi = newUnit.GetComponent<AiBase>();
            // myBotAi.AiScript.SetFinalObjective( playerBarrack);

            newUnit.SubscribeManager.Subscribe(new EnemyUnitGameRule(index));
        }

    }

    protected void OnDeathMyUnit(MyUnit myUnit)
    {
        StartCoroutine(DestroyUnitAfterSeconds(myUnit, 3));
        return;
    }

    protected void OnDeathEnemyUnit(MyUnit myUnit, int creatableEnemyIndex)
    {
        creatableEnemies[creatableEnemyIndex].currentCount--;
        StartCoroutine(DestroyUnitAfterSeconds(myUnit, 1));
        return;
    }
    IEnumerator DestroyUnitAfterSeconds(MyUnit myUnit, float seconds)
    {
        yield return new WaitForSeconds(seconds);
        Destroy(myUnit.gameObject);
    }

    [System.Serializable]
    public struct CreatableUnitDesign
    {
        public MyUnit prefab;
        public Sprite image;
        public int cost;
        public float coolTime;
        public float remainCoolTime;
    }

    [System.Serializable]
    public struct CreatableEnemyDesign
    {
        public MyUnit prefab;
        public bool canCreateInfinite;
        public int maximumCreatableNumber;
        public int currentCount;
    }

    class EnemyUnitGameRule : MyUnit.ISubscriber
    {
        private readonly int creatableEnemyIndex = 0;

        public EnemyUnitGameRule(int creatableEnemyIndex)
        {
            this.creatableEnemyIndex = creatableEnemyIndex;
        }

        void MyUnit.ISubscriber.OnDeath(MyUnit myUnit)
        {
            gameMode.OnDeathEnemyUnit(myUnit, creatableEnemyIndex);
        }

        void MyUnit.ISubscriber.OnHealthPointChange(MyUnit myUnit)
        {
        }

        void MyUnit.ISubscriber.OnDamage(MyUnit myUnit, float damage)
        {
        }

        void MyUnit.ISubscriber.OnHeal(MyUnit myUnit, float heal)
        {
        }
    }

    class UnitInfoGuiGameRule : UnitInfoGui.ISubscriber
    {
        private readonly SelectedSelectableVolumeSubscriber ssvgr;

        public UnitInfoGuiGameRule(SelectedSelectableVolumeSubscriber ssvgr)
        {
            this.ssvgr = ssvgr;
        }

        void UnitInfoGui.ISubscriber.BeforeDestroy(UnitInfoGui unitInfoGui)
        {
            ssvgr.shouldHideOnDestroy = false;
        }
    }

    class SelectableVolumeGameRule : SelectableVolume.ISubscriber
    {
        void SelectableVolume.ISubscriber.OnMouseDown(SelectableVolume selectableVolume)
        {
            gameMode.OnMouseDownSelectableVolume(selectableVolume);
        }
        void SelectableVolume.ISubscriber.BeforeDestroy(SelectableVolume selectableVolume)
        {
            HandleDestroy(selectableVolume);
        }

        protected virtual void HandleDestroy(SelectableVolume selectableVolume)
        {
            return;
        }

    }

    public virtual void OnMouseDownSelectableVolume(SelectableVolume selectableVolume)
    {
        switch (selectableVolume.type)
        {
            case SelectableVolume.Type.Barrack:
                HandleBarrackSelection(selectableVolume);
                break;
            case SelectableVolume.Type.Background:
                HandleBackgroundSelection();
                break;
            default:
                break;
        }
    }

    private void HandleBarrackSelection(SelectableVolume selectableVolume)
    {
        
        SelectedSelectableVolumeSubscriber ssvgr = new SelectedSelectableVolumeSubscriber();
        selectableVolume.SubscribeManager.Subscribe(ssvgr);
        unitInfoGui.SubscribeManager.Subscribe(new UnitInfoGuiGameRule(ssvgr));

        unitInfoGui.Display(new UnitInfoGui.UnitInfo(selectableVolume.gameObject.name, selectableVolume.gameObject.GetComponent<SpriteRenderer>()));

        List<UnitDefinitionGui.UnitDefinition> CreatableUnits = GetCreatableUnitDefinitions();
        unitCreationGui.Initialize(CreatableUnits.Count);

        int index = 0;
        void SetUnitDefinition(UnitDefinitionGui gui)
        {
            gui.SetUnitDefinition(CreatableUnits[index]);
            gui.SubscribeManager.Subscribe(new UnitDefinitionGuiSubscriber(index));
            gui.GetRemainCoolTime = ()=> creatableUnits[index].remainCoolTime;
            index++;
        }

        unitCreationGui.ForEachUnitDefinitionGui(SetUnitDefinition);

        freeMover.transform.position = new Vector3(selectableVolume.transform.position.x, selectableVolume.transform.position.y, freeMover.transform.position.z);
        
    }

    private void HandleBackgroundSelection()
    {
        unitInfoGui.Hide();
        unitCreationGui.Hide();
    }

    class SelectedSelectableVolumeSubscriber : SelectableVolumeGameRule
    {
        public bool shouldHideOnDestroy = true;
        protected override void HandleDestroy(SelectableVolume selectableVolume)
        {
            if(shouldHideOnDestroy)
                gameMode.unitInfoGui.Hide();
        }
    }

    class UnitDefinitionGuiSubscriber : UnitDefinitionGui.ISubscriber
    {
        private readonly int creatableUnitIndex;
        public UnitDefinitionGuiSubscriber(int creatableUnitIndex)
        {
            this.creatableUnitIndex = creatableUnitIndex;
        }

        void UnitDefinitionGui.ISubscriber.OnClick(UnitDefinitionGui unitDefinitionGui)
        {
            gameMode.OnClickUnitDefinitionGui(unitDefinitionGui, creatableUnitIndex);
        }

    }
}
