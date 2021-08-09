using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Onyx.BattleRoom
{
    public class BattleRoomPortal : InteractableComponent
    {
        [SerializeField]
        private BattleRoomPortal destinationPortal;
        [SerializeField]
        private AudioSource interactAudio;

        private Action<BattleRoomPortal, GameObject> onExit;
        private Action<BattleRoomPortal> onEnter;

        public BattleRoomPortal DestinationPortal => destinationPortal;

        public override void Interact(GameObject user)
        {
            if (interactAudio != null)
                interactAudio.Play();
            onExit(this, user);
        }

        public void Enter()
        {
            onEnter(this);
        }

        public void SetOnExit(Action<BattleRoomPortal, GameObject> onExit)
        {
            this.onExit = onExit;
        }

        public void SetOnEnter(Action<BattleRoomPortal> onEnter)
        {
            this.onEnter = onEnter;
        }

        public void SetDestinationPortal(BattleRoomPortal destinationPortal)
        {
            this.destinationPortal = destinationPortal;
        }
    }
}