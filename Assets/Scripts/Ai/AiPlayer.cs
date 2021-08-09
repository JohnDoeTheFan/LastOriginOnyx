using Onyx.GameElement;
using Onyx.Input;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Onyx.Ai
{
    public class AiPlayer : MonoBehaviour, Sight.ISubscriber
    {
        [SerializeField]
        private AiScriptFactoryBase aiScriptFactory;
        [SerializeField]
        private VirtualInputSource virtualInputSourceAsController;
        [SerializeField]
        private Sight sight;

        private AiScriptFactoryBase.AiScriptBase aiScript;
        private IDisposable unsubscriber;

        public AiScriptFactoryBase.AiScriptBase AiScript => aiScript;

        private void Start()
        {
            unsubscriber = sight.SubscribeManager.Subscribe(this);

            aiScript = aiScriptFactory.ProductAiScript();
            aiScript.SetTransform(transform);
            aiScript.SetContoller(virtualInputSourceAsController);
        }

        private void OnDestroy()
        {
            if(unsubscriber != null)
                unsubscriber.Dispose();
        }

        void Update()
        {
            aiScript.OnUpdate();
        }
        void Sight.ISubscriber.OnEnter(GameObject enteringObject)
        {
            aiScript.OnEnterSight(enteringObject);
        }
        void Sight.ISubscriber.OnExit(GameObject exitingObject)
        {
            aiScript.OnExitSight(exitingObject);
        }
    }
}