using System;
using UnityEngine;
using UnityEngine.Events;

namespace Onyx.GameElement
{
    public class TriggerVolume : MonoBehaviour
    {
        public Type type;
        public bool shouldDisableColliderWhenEnter;

        [SerializeField]
        private UnityEvent OnEnter;
        [SerializeField]
        private UnityEvent OnExit;

        static public Action<Type, GameObject> OnEnterTriggerVolume;
        static public Action<Type, GameObject> onExitTriggerVolume;

        private BoxCollider2D boxCollider;

        private void Start()
        {
            boxCollider = GetComponent<BoxCollider2D>();
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            OnEnterTriggerVolume?.Invoke(type, collision.gameObject);
            OnEnter.Invoke();

            if (shouldDisableColliderWhenEnter)
                boxCollider.enabled = false;
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            onExitTriggerVolume?.Invoke(type, collision.gameObject);
            OnExit.Invoke();
        }

        public enum Type
        {
            Death,
            Win,
        }
    }
}