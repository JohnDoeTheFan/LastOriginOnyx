using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Onyx.Input
{
    public class InputSource : MonoBehaviour
    {
        static public Func<float> GetLeftStickHorizontal;

        [SerializeField]
        private Camera cameraForProjection;

        public Vector3 mouseAngleCenterPointAdjust;

        public enum Button
        {
            Triangle,
            Square,
            Circle,
            Cross,
            L1,
            L2,
            R1,
            R2,
        }

        private Dictionary<Button, string> buttonStrings;

        private void Awake()
        {
            buttonStrings = new Dictionary<Button, string>()
        {
            {Button.Triangle, "Preserved" },
            {Button.Square, "Interact" },
            {Button.Circle, "A2_S1" },
            {Button.Cross, "A2_S0" },
            {Button.L1, "A1_S1" },
            {Button.L2, "A1_S0" },
            {Button.R1, "A0_S1" },
            {Button.R2, "A0_S0" }
        };
        }

        public virtual bool GetButtonDown(Button button)
        {
            return UnityEngine.Input.GetButtonDown(buttonStrings[button]);
        }

        public virtual bool GetButtonUp(Button button)
        {
            return UnityEngine.Input.GetButtonUp(buttonStrings[button]);
        }

        public virtual Vector2 GetLeftStick()
        {
            if (GetLeftStickHorizontal != null)
            {
                float guiInput = GetLeftStickHorizontal();

                if (guiInput != 0)
                    return new Vector2(guiInput, UnityEngine.Input.GetAxis("Vertical")).normalized;
                else
                    return new Vector2(UnityEngine.Input.GetAxis("Horizontal"), UnityEngine.Input.GetAxis("Vertical")).normalized;
            }
            else
            {
                return new Vector2(UnityEngine.Input.GetAxis("Horizontal"), UnityEngine.Input.GetAxis("Vertical")).normalized;
            }
        }

        public virtual Vector2 GetRightStick()
        {
            Vector2 centerPosition = transform.position + mouseAngleCenterPointAdjust;
            Vector2 screenPosition = cameraForProjection.WorldToScreenPoint(centerPosition);
            Vector2 mousePosition = UnityEngine.Input.mousePosition;

            return (mousePosition - screenPosition).normalized;
        }
    }
}