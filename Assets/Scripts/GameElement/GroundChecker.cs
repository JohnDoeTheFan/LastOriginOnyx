using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Onyx.GameElement
{
    public class GroundChecker : MonoBehaviour
    {
        public bool IsGrounded { get; private set; }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            IsGrounded = true;
        }
        private void OnTriggerStay2D(Collider2D collision)
        {
            IsGrounded = true;
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            IsGrounded = false;
        }
    }

}