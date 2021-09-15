using Onyx.GameElement;
using Onyx.Input;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace Onyx.Ai
{
    public abstract class AiBase : MonoBehaviour, Sight.ISubscriber
    {
        [Header("AiBase")]
        [SerializeField] protected VirtualInputSource virtualInputSourceAsController;
        [SerializeField] private Sight sight;
        [SerializeField] protected Transform modelTransform;

        private IDisposable unsubscriber;

        protected Transform enemy;
        protected ReadOnlyCollection<Transform> objectsInSight;

        protected Transform ObjectiveOfTarget;

        protected virtual void Start()
        {
            unsubscriber = sight.SubscribeManager.Subscribe(this);
        }

        private void OnDestroy()
        {
            if(unsubscriber != null)
                unsubscriber.Dispose();
        }

        void Update()
        {
            OnUpdate();
        }

        protected abstract void OnUpdate();

        void Sight.ISubscriber.OnEnter(GameObject enteringObject)
        {
            if (enteringObject.CompareTag("Player"))
                enemy = enteringObject.transform;
        }
        void Sight.ISubscriber.OnExit(GameObject exitingObject)
        {
            if (exitingObject.transform == enemy)
                enemy = null;
        }
    }
}