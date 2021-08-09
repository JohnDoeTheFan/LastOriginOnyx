using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OwningBulletsGui : MonoBehaviour
{
    [SerializeField]
    private OwningBulletInfoGui owningBulletInfoGui;
    private RectTransform rectTransform;

    [SerializeField]
    float size = 200;
    [SerializeField]
    float focusExpansion = 0.1f;

    public enum Type
    {
        All,
        Available,
        NotAvailable
    }
    public Type type;

    readonly List<Child> childs = new List<Child>();

    IUnsubscriber gunSlingerSubscriber;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void Start()
    {
        rectTransform.sizeDelta = new Vector2(0, size);
    }

    private void OnDestroy()
    {
        gunSlingerSubscriber?.Unsubscribe();
    }

    public void Initialize(int numOfData, int focusIndex)
    {
        InitializeImpl(numOfData);

        SetFocus(focusIndex);
    }

    public void Initialize(int numOfData)
    {
        InitializeImpl(numOfData);

        NoFocus();
    }

    private void InitializeImpl(int numOfData)
    {
        if (!gameObject.activeInHierarchy)
            gameObject.SetActive(true);

        foreach (Child child in childs)
            Destroy(child.owningBulletInfoGui.gameObject);
        childs.Clear();

        rectTransform.sizeDelta = new Vector2(size * numOfData, size);

        for (int i = 0; i < numOfData; i++)
        {
            OwningBulletInfoGui newOwningBulletInfoGui = Instantiate<OwningBulletInfoGui>(owningBulletInfoGui);
            RectTransform newRectTransform = newOwningBulletInfoGui.GetComponent<RectTransform>();

            newRectTransform.SetParent(rectTransform, false);
            newRectTransform.anchoredPosition = new Vector2(size * i, 0);
            newRectTransform.sizeDelta = new Vector2(size, 0);

            childs.Add(new Child(newOwningBulletInfoGui, newRectTransform));
        }
    }

    public void NoFocus()
    {
        rectTransform.sizeDelta = new Vector2(size * childs.Count, size);

        for(int i = 0; i < childs.Count; i++)
        {
            childs[i].rectTransform.anchoredPosition = new Vector2(size * i, 0);
            childs[i].rectTransform.sizeDelta = new Vector2(size, 0);
        }
    }

    public void SetFocus(int index)
    {
        if (index < 0 && index > childs.Count - 1)
            NoFocus();

        rectTransform.sizeDelta = new Vector2(size * childs.Count + size * focusExpansion, size + size * focusExpansion);

        float xToMove = size * focusExpansion;

        for (int i = 0; i < childs.Count; i++)
        {
            if(i < index)
            {
                childs[i].rectTransform.anchoredPosition = new Vector2(size * i, 0);
                childs[i].rectTransform.sizeDelta = new Vector2(size, -size * focusExpansion);
                childs[i].owningBulletInfoGui.SetFocus(false);
            }
            else if(i > index)
            {
                childs[i].rectTransform.anchoredPosition = new Vector2(size * i + xToMove, 0);
                childs[i].rectTransform.sizeDelta = new Vector2(size, -size * focusExpansion);
                childs[i].owningBulletInfoGui.SetFocus(false);
            }
            else
            {
                childs[i].rectTransform.anchoredPosition = new Vector2(size * i, 0);
                childs[i].rectTransform.sizeDelta = new Vector2(size + size * focusExpansion, 0);
                childs[i].owningBulletInfoGui.SetFocus(true);
            }
        }
    }

    public void SetGunSlinger(GunSlinger gunSlinger)
    {
        gunSlingerSubscriber?.Unsubscribe();
        gunSlingerSubscriber = new GunSlingerSubscriber(gunSlinger, this);
        gunSlingerSubscriber.SetOnUnsubscribe(() => gunSlingerSubscriber = null);
    }

    void OnUpdateBullets(GunSlinger gunSlinger)
    {
        switch (type)
        {
            case Type.All:
                {
                    Initialize(gunSlinger.LoaderSet.Loaders.Count);
                }
                break;
            case Type.Available:
                {
                    Initialize(gunSlinger.AvailableLoaders.Count);

                    for (int i = 0; i < childs.Count; i++)
                        childs[i].owningBulletInfoGui.SetBulletInfo(gunSlinger.AvailableLoaders[i]);

                    if (gunSlinger.AvailableLoaders.IsInRange(gunSlinger.SelectedLoaderIndex))
                        SetFocus(gunSlinger.SelectedLoaderIndex);
                }
                break;
            case Type.NotAvailable:
                {
                    Initialize(gunSlinger.NotAvailableLoaders.Count);

                    for(int i = 0; i < childs.Count; i++)
                        childs[i].owningBulletInfoGui.SetBulletInfo(gunSlinger.NotAvailableLoaders[i]);
                }
                break;
        }
    }
    struct Child
    {
        public Child(OwningBulletInfoGui owningBulletInfoGui, RectTransform rectTransform)
        {
            this.owningBulletInfoGui = owningBulletInfoGui;
            this.rectTransform = rectTransform;
        }

        public OwningBulletInfoGui owningBulletInfoGui;
        public RectTransform rectTransform;
    }

    private class GunSlingerSubscriber : GunSlinger.SubscriberImpl, IUnsubscriber
    {
        private readonly IDisposable unsubscriber;
        private Action onUnsubscribe;

        readonly OwningBulletsGui gui = null;
        public GunSlingerSubscriber(GunSlinger gunSlinger, OwningBulletsGui gui)
        {
            unsubscriber = gunSlinger.SubscribeManager.Subscribe(this);
            this.gui = gui;
        }

        public override void OnUpdateBullets(GunSlinger gunSlinger)
        {
            gui.OnUpdateBullets(gunSlinger);
        }

        public override void BeforeDestroy(GunSlinger gunSlinger)
        {
            Unsubscribe();
        }

        public void Unsubscribe()
        {
            unsubscriber.Dispose();
            onUnsubscribe?.Invoke();
        }

        public void SetOnUnsubscribe(Action onUnsubscribe)
        {
            this.onUnsubscribe = onUnsubscribe;
        }
    }
}
