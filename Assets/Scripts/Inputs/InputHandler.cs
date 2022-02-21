using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Onyx.Input
{
    public class InputHandler : MonoBehaviour
    {
        static public Func<Type, bool> GetPermission;

        [SerializeField]
        private InputSource inputSource;
        [SerializeField]
        private Type type;

        private HashSet<IInputReceiver> inputReceivers = new HashSet<IInputReceiver>();
        private HashSet<IInputReceiver> inputReceiverRegisterAwaiters = new HashSet<IInputReceiver>();
        private HashSet<IInputReceiver> inputReceiverUnregisterAwaiters = new HashSet<IInputReceiver>();

        private bool lastPermission = false;

        // Update is called once per frame
        void Update()
        {
            inputReceivers.UnionWith(inputReceiverRegisterAwaiters);
            inputReceiverRegisterAwaiters.Clear();

            foreach (IInputReceiver inputReceiver in inputReceiverUnregisterAwaiters)
            {
                inputReceiver.OnAbility0Skill0ButtonUp();
            }
            inputReceivers.ExceptWith(inputReceiverUnregisterAwaiters);
            inputReceiverUnregisterAwaiters.Clear();

            bool currentPermission = GetPermission?.Invoke(type) ?? false;
            if (currentPermission)
            {
                foreach (var receiver in inputReceivers)
                {
                    receiver.OnLeftStick(inputSource.GetLeftStick());
                    receiver.OnRightStick(inputSource.GetRightStick());

                    if (inputSource.GetButtonDown(InputSource.Button.Square))
                        receiver.OnInteractButtonDown();

                    if (inputSource.GetButtonDown(InputSource.Button.Triangle))
                        receiver.OnPreservedButtonDown();

                    if (inputSource.GetButtonDown(InputSource.Button.Circle))
                        receiver.OnAbility2Skill1ButtonDown();

                    if (inputSource.GetButtonDown(InputSource.Button.Cross))
                        receiver.OnAbility2Skill0ButtonDown();

                    if (inputSource.GetButtonDown(InputSource.Button.L1))
                        receiver.OnAbility1Skill1ButtonDown();

                    if (inputSource.GetButtonDown(InputSource.Button.L2))
                        receiver.OnAbility1Skill0ButtonDown();

                    if (inputSource.GetButtonDown(InputSource.Button.R1))
                        receiver.OnAbility0Skill1ButtonDown();

                    if (inputSource.GetButtonDown(InputSource.Button.R2))
                        receiver.OnAbility0Skill0ButtonDown();
                    else if (inputSource.GetButtonUp(InputSource.Button.R2))
                        receiver.OnAbility0Skill0ButtonUp();
                }
            }
            else if(lastPermission)
            {
                foreach (var receiver in inputReceivers)
                {
                    receiver.OnLeftStick(Vector2.zero);
                    receiver.OnRightStick(Vector2.zero);

                    receiver.OnAbility0Skill0ButtonUp();
                }
            }

            lastPermission = currentPermission;
        }

        public void AddInputReceiverRegisterAwaiter(IInputReceiver inputReceiver)
        {
            inputReceiverRegisterAwaiters.Add(inputReceiver);
        }

        public void AddInputReceiverUnregisterAwaiter(IInputReceiver inputReceiver)
        {
            inputReceiverUnregisterAwaiters.Add(inputReceiver);
        }

        public enum Type
        {
            Player,
            Ai
        }

        public interface IInputReceiver
        {
            void OnLeftStick(Vector2 leftStick);
            void OnRightStick(Vector2 mousePosition);

            void OnInteractButtonDown();
            void OnPreservedButtonDown();

            void OnAbility2Skill1ButtonDown();
            void OnAbility2Skill0ButtonDown();

            void OnAbility1Skill1ButtonDown();
            void OnAbility1Skill1ButtonUp();
            void OnAbility1Skill0ButtonDown();
            void OnAbility1Skill0ButtonUp();

            void OnAbility0Skill1ButtonDown();
            void OnAbility0Skill1ButtonUp();
            void OnAbility0Skill0ButtonDown();
            void OnAbility0Skill0ButtonUp();
        }
    }

}
