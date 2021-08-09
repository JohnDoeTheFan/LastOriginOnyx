using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class BattleObjectiveRecordListGui : MonoBehaviour
{
    [SerializeField]
    private BattleObjectiveRecordGui recordPrefab;

    private RectTransform rectTransform;
    readonly private List<BattleObjectiveRecordGui> records = new List<BattleObjectiveRecordGui>();

    public bool IsEmpty => records.Count == 0;
    public ReadOnlyCollection<BattleObjectiveRecordGui> Records => records.AsReadOnly();

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public BattleObjectiveRecordGui AddRecord(string description)
    {
        BattleObjectiveRecordGui newRecord = Instantiate<BattleObjectiveRecordGui>(recordPrefab, rectTransform);
        newRecord.Initialize(description);

        Vector2 textPosition = newRecord.RectTransform.anchoredPosition;
        textPosition.y = -1 * newRecord.RectTransform.sizeDelta.y * records.Count;
        newRecord.RectTransform.anchoredPosition = textPosition;

        records.Add(newRecord);

        return newRecord;
    }

    public void ClearRecords()
    {
        records.ForEach(item => Destroy(item.gameObject));
        records.Clear();
    }

}
