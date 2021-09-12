using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Onyx.GameElement
{
    public class GroundChecker : MonoBehaviour
    {
        public bool IsGrounded => otherColliders.Count > 0;
        public SubscribeManagerTemplate<ISubscriber> SubscribeManager { private set; get; } = new SubscribeManagerTemplate<ISubscriber>();
        private List<Collider2D> otherColliders = new List<Collider2D>();

        public Vector2 GetGroundVelocity()
        {
            if (otherColliders.Count == 0)
                return Vector2.zero;
            else
            {
                Vector2 velocitySum = new Vector2();
                foreach (Collider2D collider in otherColliders)
                {
                    velocitySum += collider.attachedRigidbody.velocity;
                }
                return velocitySum / otherColliders.Count;
            }

        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            otherColliders.Add(collision);
            SubscribeManager.ForEach((item)=>item.OnGrounded());
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            otherColliders.Remove(collision);
            if (otherColliders.Count == 0)
                SubscribeManager.ForEach((item) => item.OnAir());
        }

        public interface ISubscriber
        {
            void OnGrounded();

            void OnAir();
        }
    }

}