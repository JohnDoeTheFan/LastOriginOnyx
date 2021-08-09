using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Onyx.GameElement
{
    public class Sight : MonoBehaviour
    {
        public interface ISubscriber
        {
            void OnEnter(GameObject enteringObject);
            void OnExit(GameObject exitingObject);
        }
        public SubscribeManagerTemplate<ISubscriber> SubscribeManager = new SubscribeManagerTemplate<ISubscriber>();

        private void OnTriggerEnter(Collider other)
        {
            SubscribeManager.ForEach(item => item.OnEnter(other.gameObject));
        }

        private void OnTriggerExit(Collider other)
        {
            SubscribeManager.ForEach(item => item.OnExit(other.gameObject));
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            SubscribeManager.ForEach(item => item.OnEnter(collision.gameObject));
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            SubscribeManager.ForEach(item => item.OnExit(collision.gameObject));
        }
    }
}