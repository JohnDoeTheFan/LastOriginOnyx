using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BattleObjective : MonoBehaviour
{
    public SubscribeManagerTemplate<ISubscriber> subscribeManager { private set; get; } = new SubscribeManagerTemplate<ISubscriber>();

    public interface ISubscriber
    {
        void OnChangedStatus();
    }

}
