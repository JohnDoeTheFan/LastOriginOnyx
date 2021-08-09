using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SelectableVolume : MonoBehaviour
{
    public enum Type
    {
        Background,
        Barrack,
    }

    public Type type;

    public interface ISubscriber
    {
        void OnMouseDown(SelectableVolume selectableVolume);
        void BeforeDestroy(SelectableVolume selectableVolume);
    }

    public SubscribeManagerTemplate<ISubscriber> SubscribeManager { private set; get; } = new SubscribeManagerTemplate<ISubscriber>();

    private void OnMouseDown()
    {
        if (!EventSystem.current.IsPointerOverGameObject())
            SubscribeManager.ForEach(item => item.OnMouseDown(this));
    }

    private void OnDestroy()
    {
        SubscribeManager.ForEach(item => item.BeforeDestroy(this));
    }
}
