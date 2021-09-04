using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

public abstract class ThreeStateToggleGroup<TCoreData> : MonoBehaviour
{
    [SerializeField]
    protected ButtonAsThreeStateToggle<TCoreData> threeStateTogglePrefab;
    [SerializeField]
    private List<ButtonAsThreeStateToggle<TCoreData>> threeStateToggles = new List<ButtonAsThreeStateToggle<TCoreData>>();

    private ButtonAsThreeStateToggle<TCoreData> currentSelection;
    private readonly UnsubscriberPack unsubscriberPack = new UnsubscriberPack();
    public SubscribeManagerTemplate<ISubscriber> SubscribeManager { private set; get; } = new SubscribeManagerTemplate<ISubscriber>();

    public ReadOnlyCollection<ButtonAsThreeStateToggle<TCoreData>> ThreeStateToggles => threeStateToggles.AsReadOnly();

    protected virtual void Awake()
    {
        threeStateToggles.ForEach(item =>
        {
            unsubscriberPack.Add(new ThreeStateToggleSubscriber(item, OnChageInteractable, OnChangeIsOn));
        });
    }

    public abstract void Initialize(TCoreData coreData);

    protected void DestroyToggles()
    {
        unsubscriberPack.UnsubscribeAll();
        threeStateToggles.ForEach(item => Destroy(item.gameObject));
        threeStateToggles.Clear();
    }

    protected void RegisterToggle(ButtonAsThreeStateToggle<TCoreData> newToggle)
    {
        unsubscriberPack.Add(new ThreeStateToggleSubscriber(newToggle, OnChageInteractable, OnChangeIsOn));
        threeStateToggles.Add(newToggle);
    }

    void OnChageInteractable(ButtonAsThreeStateToggle<TCoreData> threeStateToggle, bool interactable)
    {
        if (!interactable && threeStateToggle == currentSelection)
        {
            currentSelection = null;
            for (int i = threeStateToggles.Count - 1; i >= 0; i--)
            {
                if (threeStateToggles[i].Interactable)
                {
                    currentSelection = threeStateToggles[i];
                    currentSelection.IsOn = true;
                }
            }
        }
    }

    void OnChangeIsOn(ButtonAsThreeStateToggle<TCoreData> threeStateToggle, bool isOn)
    {
        if (isOn && threeStateToggle != currentSelection)
        {
            foreach (ButtonAsThreeStateToggle<TCoreData> elseIcon in threeStateToggles)
            {
                if (elseIcon != threeStateToggle)
                    elseIcon.IsOn = false;
            }
            currentSelection = threeStateToggle;
            SubscribeManager.ForEach(item => item.OnChangeSelection(threeStateToggle.CoreData));
        }
    }

    public interface ISubscriber
    {
        void OnChangeSelection(TCoreData stageInfo);
    }

    public class ThreeStateToggleSubscriber : UniUnsubscriber, ButtonAsThreeStateToggle<TCoreData>.ISubscriber
    {
        readonly Action<ButtonAsThreeStateToggle<TCoreData>, bool> OnChangeInteractable;
        readonly Action<ButtonAsThreeStateToggle<TCoreData>, bool> OnChangeIsOn;

        IDisposable unsubscriber;

        protected override IDisposable Unsubscriber => unsubscriber;

        public ThreeStateToggleSubscriber(ButtonAsThreeStateToggle<TCoreData> threeStateToggle, Action<ButtonAsThreeStateToggle<TCoreData>, bool> OnChangeInteractable, Action<ButtonAsThreeStateToggle<TCoreData>, bool> OnChangeIsOn)
        {
            unsubscriber = threeStateToggle.SubscribeManager.Subscribe(this);
            this.OnChangeInteractable = OnChangeInteractable;
            this.OnChangeIsOn = OnChangeIsOn;
        }


        void ButtonAsThreeStateToggle<TCoreData>.ISubscriber.OnChageInteractable(ButtonAsThreeStateToggle<TCoreData> threeStateToggle, bool interactable)
        {
            OnChangeInteractable(threeStateToggle, interactable);
        }

        void ButtonAsThreeStateToggle<TCoreData>.ISubscriber.OnChangeIsOn(ButtonAsThreeStateToggle<TCoreData> threeStateToggle, bool isOn)
        {
            OnChangeIsOn(threeStateToggle, isOn);
        }
    }
}

public abstract class ButtonAsThreeStateToggle<TCoreData> : MonoBehaviour
{
    [SerializeField]
    private bool interactable = true;
    [SerializeField]
    private bool isOn;
    [SerializeField]
    protected TCoreData coreData;

    private Animator animator;
    private RectTransform rectTransform;

    public RectTransform RectTransform => rectTransform;
    public TCoreData CoreData => coreData;
    public SubscribeManagerTemplate<ISubscriber> SubscribeManager { get; private set; } = new SubscribeManagerTemplate<ISubscriber>();

    public bool IsOn
    {
        get => isOn;
        set
        {
            isOn = value;

            SubscribeManager.ForEach(item => item.OnChangeIsOn(this, isOn));

            if (animator != null)
                animator.SetBool("IsOn", isOn);
        }
    }

    public bool Interactable
    {
        get => interactable;
        set
        {
            interactable = value;

            SubscribeManager.ForEach(item => item.OnChageInteractable(this, interactable));

            if (animator != null)
                animator.SetBool("Interactable", interactable);
        }
    }

    private void Awake()
    {
        animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetBool("IsOn", isOn);
            animator.SetBool("Interactable", interactable);
        }
        rectTransform = GetComponent<RectTransform>();
    }

    public abstract void Initialize(TCoreData coreData);

    public void Interact()
    {
        if (Interactable)
            IsOn = true;
    }

    public interface ISubscriber
    {
        void OnChangeIsOn(ButtonAsThreeStateToggle<TCoreData> stageIconGui, bool isOn);
        void OnChageInteractable(ButtonAsThreeStateToggle<TCoreData> stageIconGui, bool interactable);
    }
}