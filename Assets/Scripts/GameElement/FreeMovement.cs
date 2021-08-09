using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Onyx.Input;

namespace Onyx.GameElement
{
    public class FreeMovement : MonoBehaviour, InputHandler.IInputReceiver
    {
        public float speed = 10;
        public float lowerLimit = 0;

        InputHandler inputHandler;

        // Start is called before the first frame update
        void Start()
        {
            inputHandler = GetComponent<InputHandler>();
            inputHandler.AddInputReceiverRegisterAwaiter(this);
        }

        void InputHandler.IInputReceiver.OnLeftStick(Vector2 leftStick)
        {
            float newX = transform.position.x + speed * leftStick.x * Time.deltaTime;

            float newY = transform.position.y + speed * leftStick.y * Time.deltaTime;
            newY = Mathf.Max(newY, lowerLimit);

            transform.position = new Vector3(newX, newY, transform.position.z);
        }
        void InputHandler.IInputReceiver.OnRightStick(Vector2 rightStick) { }

        void InputHandler.IInputReceiver.OnInteractButtonDown() { }
        void InputHandler.IInputReceiver.OnPreservedButtonDown() { }

        void InputHandler.IInputReceiver.OnAbility2Skill1ButtonDown() { }
        void InputHandler.IInputReceiver.OnAbility2Skill0ButtonDown() { }

        void InputHandler.IInputReceiver.OnAbility1Skill1ButtonDown() { }
        void InputHandler.IInputReceiver.OnAbility1Skill1ButtonUp() { }
        void InputHandler.IInputReceiver.OnAbility1Skill0ButtonDown() { }
        void InputHandler.IInputReceiver.OnAbility1Skill0ButtonUp() { }

        void InputHandler.IInputReceiver.OnAbility0Skill1ButtonDown() { }
        void InputHandler.IInputReceiver.OnAbility0Skill1ButtonUp() { }
        void InputHandler.IInputReceiver.OnAbility0Skill0ButtonDown() { }
        void InputHandler.IInputReceiver.OnAbility0Skill0ButtonUp() { }

    }
}