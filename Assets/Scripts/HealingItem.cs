using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Onyx.GameElement
{
    public class HealingItem : MonoBehaviour, Sight.ISubscriber
    {
        [SerializeField]
        private float health;
        [SerializeField]
        private Sight sight;
        [SerializeField]
        private GameObject gettingEffectPrefab;

        private IDisposable unsubscriber;
        private void Start()
        {
            unsubscriber = sight.SubscribeManager.Subscribe(this);
        }

        private void OnDestroy()
        {
            if(unsubscriber != null)
                unsubscriber.Dispose();
        }
        public interface IHealingItemReactor
        {
            bool Heal(float health);
            bool HealPercent(float percent);
        }

        void Sight.ISubscriber.OnEnter(GameObject enteringObject)
        {
            IHealingItemReactor itemReactor = enteringObject.GetComponent<IHealingItemReactor>();
            if (itemReactor != null && itemReactor.HealPercent(health))
            {
                if (gettingEffectPrefab != null)
                    Instantiate<GameObject>(gettingEffectPrefab, transform.position, Quaternion.identity);
                Destroy(gameObject);
            }
        }

        void Sight.ISubscriber.OnExit(GameObject exitingObject)
        {

        }
    }
}