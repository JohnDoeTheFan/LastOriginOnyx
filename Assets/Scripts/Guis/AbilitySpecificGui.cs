using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Onyx.Ability;

public abstract class AbilitySpecificGui<T> : MonoBehaviourBase where T :IAbility
{
    public abstract void SetAbility(T ability);
}
