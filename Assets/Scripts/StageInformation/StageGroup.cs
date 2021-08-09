using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StageGroup : ThreeStateToggleGroup<StageInformation>
{
    [SerializeField]
    private Vector2 distance;
    [SerializeField]
    private RectTransform linesParent;
    [SerializeField]
    private Image previousLinePrefab;
    [SerializeField]
    private Vector2 previousLineOffset;
    [SerializeField]
    private Image lowerLinePrefab;
    [SerializeField]
    private Vector2 lowerLineOffset;
    [SerializeField]
    private Color disabledLineColor;

    public override void Initialize(StageInformation coreData)
    {
        DestroyToggles();
        for (int i = 0; i < linesParent.childCount; i++)
            Destroy(linesParent.GetChild(i).gameObject);

        StageIcons icons = CreateStageIconRecursively(coreData, new Vector2(0, 0), NewStagePositionType.New);

        ButtonAsThreeStateToggle<StageInformation> newestAvailableNormalToggle = null;
        foreach(ButtonAsThreeStateToggle<StageInformation> toggle in icons.normals)
        {
            StageInformation stageInformation = toggle.CoreData;
            if (stageInformation.PreviousStage == null)
                newestAvailableNormalToggle = toggle;
            else if (stageInformation.IsCleared)
                newestAvailableNormalToggle = toggle;
            else if (stageInformation.PreviousStage.IsCleared)
                newestAvailableNormalToggle = toggle;
        }

        if (newestAvailableNormalToggle != null)
            newestAvailableNormalToggle.IsOn = true;
    }

    private StageIcons CreateStageIconRecursively(StageInformation stageInfo, Vector2 lastPosition, NewStagePositionType newStagePositionType)
    {
        StageIcons result = StageIcons.Empty;

        if (stageInfo == null)
            return result;

        Vector2 newPosition = CalcNewPosition(lastPosition, newStagePositionType);

        bool shouldMakePreviousLine = newStagePositionType == NewStagePositionType.Next;
        bool shouldMakeLowerLine = newStagePositionType == NewStagePositionType.Upper;

        ButtonAsThreeStateToggle<StageInformation> newStageGui = Instantiate<ButtonAsThreeStateToggle<StageInformation>>(threeStateTogglePrefab, transform);
        newStageGui.RectTransform.anchoredPosition = newPosition;
        newStageGui.Initialize(stageInfo);

        if (shouldMakePreviousLine)
            MakePreviousLine(lastPosition, newPosition, newStageGui.Interactable);

        if (shouldMakeLowerLine)
            MakeLowerLine(lastPosition, newPosition, newStageGui.Interactable);

        RegisterToggle(newStageGui);

        switch (stageInfo.StageType)
        {
            case StageInformation.StageTypeEnum.Normal:
                result.normals.Add(newStageGui);
                break;
            case StageInformation.StageTypeEnum.Side:
                result.sides.Add(newStageGui);
                break;
            case StageInformation.StageTypeEnum.Ex:
                result.normals.Add(newStageGui);
                break;
        }
        result.Add(CreateNextStageIcons(stageInfo, newPosition));

        return result;
    }

    private StageIcons CreateNextStageIcons(StageInformation stageInfo, Vector2 lastPosition)
    {
        StageIcons stageIcons = StageIcons.Empty;

        if (stageInfo.StageType == StageInformation.StageTypeEnum.Normal)
        {
            stageIcons.Add(CreateStageIconRecursively(stageInfo.NextStages.nextSideStage, lastPosition, NewStagePositionType.Upper));
            stageIcons.Add(CreateStageIconRecursively(stageInfo.NextStages.nextNormalStage, lastPosition, NewStagePositionType.Next));
            stageIcons.Add(CreateStageIconRecursively(stageInfo.NextStages.nextExStage, lastPosition, NewStagePositionType.FirstLower));
        }
        else if(stageInfo.StageType == StageInformation.StageTypeEnum.Side)
        {
            stageIcons.Add(CreateStageIconRecursively(stageInfo.NextStages.nextSideStage, lastPosition, NewStagePositionType.Next));
        }
        else if (stageInfo.StageType == StageInformation.StageTypeEnum.Ex)
        {
            stageIcons.Add(CreateStageIconRecursively(stageInfo.NextStages.nextExStage, lastPosition, NewStagePositionType.Next));
        }

        return stageIcons;
    }

    private void MakePreviousLine(Vector2 lastPosition, Vector2 newPosition, bool shouldBeEnabled)
    {
        Vector2 previousLinePosition = (lastPosition + newPosition) / 2;
        Image newImage = Instantiate<Image>(previousLinePrefab, linesParent);
        newImage.rectTransform.anchoredPosition = previousLinePosition + previousLineOffset;
        if (!shouldBeEnabled)
            newImage.color = disabledLineColor;
    }

    private void MakeLowerLine(Vector2 lastPosition, Vector2 newPosition, bool shouldBeEnabled)
    {
        Vector2 lowerLinePosition = (lastPosition + newPosition) / 2;
        Image newImage = Instantiate<Image>(lowerLinePrefab, linesParent);
        newImage.rectTransform.anchoredPosition = lowerLinePosition + lowerLineOffset;
        if (shouldBeEnabled)
            newImage.color = disabledLineColor;
    }

    private Vector2 CalcNewPosition(Vector2 lastPosition, NewStagePositionType newStagePositionType)
    {
        return newStagePositionType switch
        {
            NewStagePositionType.New => lastPosition,
            NewStagePositionType.Next => lastPosition + new Vector2(distance.x, 0),
            NewStagePositionType.Upper => lastPosition + new Vector2(distance.x * 0.4f, distance.y),
            NewStagePositionType.FirstLower => new Vector2(0, -distance.y),
            _ => throw new NotImplementedException(),
        };
    }

    enum NewStagePositionType
    {
        New,
        Next,
        Upper,
        FirstLower
    }

    struct StageIcons
    {
        static public StageIcons Empty => new StageIcons(
            new List<ButtonAsThreeStateToggle<StageInformation>>(),
            new List<ButtonAsThreeStateToggle<StageInformation>>(),
            new List<ButtonAsThreeStateToggle<StageInformation>>()
            );

        public List<ButtonAsThreeStateToggle<StageInformation>> normals;
        public List<ButtonAsThreeStateToggle<StageInformation>> sides;
        public List<ButtonAsThreeStateToggle<StageInformation>> exs;

        public StageIcons(List<ButtonAsThreeStateToggle<StageInformation>> normals,
            List<ButtonAsThreeStateToggle<StageInformation>> sides,
            List<ButtonAsThreeStateToggle<StageInformation>> exs)
        {
            this.normals = normals;
            this.sides = sides;
            this.exs = exs;
        }

        public void Add(StageIcons other)
        {
            normals.AddRange(other.normals);
            sides.AddRange(other.sides);
            exs.AddRange(other.exs);
        }
    }

}