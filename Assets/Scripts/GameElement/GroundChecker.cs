using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Onyx.GameElement
{
    public class GroundChecker : MonoBehaviour
    {
        public bool IsGrounded => colliderCount > 0;
        public SubscribeManagerTemplate<ISubscriber> SubscribeManager { private set; get; } = new SubscribeManagerTemplate<ISubscriber>();
        private int colliderCount = 0;

        private void OnTriggerEnter2D(Collider2D collision)
        {
            colliderCount++;
            SubscribeManager.ForEach((item)=>item.OnGrounded());
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            colliderCount--;
            if(colliderCount == 0)
                SubscribeManager.ForEach((item) => item.OnAir());
        }

        public interface ISubscriber
        {
            void OnGrounded();

            void OnAir();
        }
    }

}