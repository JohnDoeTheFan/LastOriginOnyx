using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Onyx.Core
{
    public enum ScenesToBeBuild
    {
        Title = 0,
        Lobby = 1,
        StageMapScene = 2,
    }

    public enum StageScenesToBeBuild
    {
        StageScene = 3
    }

    public enum LayerSetting
    {
        // Builtin
        Default,
        TransparentFX,
        IgnoreRaycast,
        Undefined0,
        Water,
        UI,
        // Character
        Player,
        Npc,
        Enemy,
        DeadBody,
        Undefined1,
        Undefined2,
        Undefined3,
        Undefined4,
        // Objects
        Interactable,
        Structure,
        BulletOfPlayer,
        BulletOfEnemy,
        ShieldOfPlayer,
        ShieldOfEnemy,
        Undefined5,
        Undefined6,
        // Utilities
        Sight,
        GroundChecker,
        Background,
        Undefined7,
        Undefined8,
        Undefined9,
        Undefined10,
        Undefined11,
        Undefined12,
        Undefined13,
    }

}


public class BaseObject : MonoBehaviour
{
}

public interface IHitReactor
{
    enum HitType
    {
        Bullet,
        Trap,
        MeleeAttackStrike
    }

    struct HitInfo
    {
        public readonly HitType type;
        public readonly float damage;
        public readonly Vector3 direction;
        public readonly bool isPenetration;
        public readonly Vector3 knockBackVelocity;
        public readonly float stiffenTime;

        public HitInfo(HitType type, float damage, Vector3 direction, bool isPenetration)
        {
            this.type = type;
            this.damage = damage;
            this.direction = direction;
            this.isPenetration = isPenetration;
            knockBackVelocity = Vector3.zero;
            stiffenTime = 0f;
        }

        public HitInfo(HitType type, float damage, Vector3 direction, bool isPenetration, Vector3 knockBackVelocity)
        {
            this.type = type;
            this.damage = damage;
            this.direction = direction;
            this.isPenetration = isPenetration;
            this.knockBackVelocity = knockBackVelocity;
            stiffenTime = 0f;
        }

        public HitInfo(HitType type, float damage, Vector3 direction, bool isPenetration, Vector3 knockBackVelocity, float stiffenTime)
        {
            this.type = type;
            this.damage = damage;
            this.direction = direction;
            this.isPenetration = isPenetration;
            this.knockBackVelocity = knockBackVelocity;
            this.stiffenTime = stiffenTime;
        }
    }

    struct HitReaction
    {
        public readonly bool isBlocked;

        public HitReaction(bool isBlocked)
        {
            this.isBlocked = isBlocked;
        }
    }

    struct HitResult
    {
        public HitResult(float acceptedDamage, bool isKilledByHit)
        {
            this.acceptedDamage = acceptedDamage;
            this.isKilledByHit = isKilledByHit;
        }

        public float acceptedDamage;
        public bool isKilledByHit;
    }

    HitResult Hit(HitInfo hitInfo);

    Vector3 GetWorldPosition { get; }

    GameObject GameObject { get; }
}

public class MonoBehaviourBase : MonoBehaviour
{
    public float TargetFrameSeconds => 1f / 30f;

    protected IEnumerator Job(Func<IEnumerator> toDo, Action nextJob)
    {
        yield return StartCoroutine(toDo());
        nextJob?.Invoke();
    }
    protected IEnumerator WaitForEndOfFrameRoutine()
    {
        yield return new WaitForEndOfFrame();
    }

    protected IEnumerator WaitForSecondsRoutine(float seconds)
    {
        yield return new WaitForSeconds(seconds);
    }
    
    protected IEnumerator WaitForSecondsRealtimeRoutine(float seconds)
    {
        yield return new WaitForSecondsRealtime(seconds);
    }

    protected IEnumerator WaitUntilRoutine(Func<bool> predicate)
    {
        yield return new WaitUntil(predicate);
    }

    protected IEnumerator WaitUntilOrForSecondsRoutine(Func<bool> predicate, float seconds)
    {
        yield return new WaitUntilOrForSeconds(predicate, seconds);
    }
}

public class TangibleComponent : MonoBehaviourBase
{
    [Header("TangibleComponent")]
    [SerializeField]
    private Sprite _image;
    public Sprite Image => _image;
}

public abstract class InteractableComponent : TangibleComponent
{
    [SerializeField]
    private Canvas interactIconCanvas;
    [SerializeField]
    private Button interactIconButton;
    
    public abstract void Interact(GameObject user);

    public void ActiveInteractIcon(bool active, GameObject user)
    {
        interactIconCanvas.gameObject.SetActive(active);
        if (active)
            interactIconButton.onClick.AddListener(() => Interact(user));
        else
            interactIconButton.onClick.RemoveAllListeners();
    }
}