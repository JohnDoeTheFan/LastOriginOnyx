using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Onyx.Input
{
    public class VirtualInputSource : InputSource
    {

        public float inputOverlappingPreventTime = 0.5f;
        private Dictionary<Button, float> lastInputTimes;
        private Dictionary<Button, bool> isDowns;
        private Dictionary<Button, bool> isUps;
        private Vector2 leftStick = Vector2.zero;
        private Vector2 rightStick = Vector2.zero;

        private void Awake()
        {
            lastInputTimes = new Dictionary<Button, float>
        {
            { Button.Cross, 0f },
            { Button.Circle, 0f },
            { Button.Square, 0f },
            { Button.Triangle, 0f },
            { Button.L1, 0f },
            { Button.L2, 0f },
            { Button.R1, 0f },
            { Button.R2, 0f }
        };

            isDowns = new Dictionary<Button, bool>
        {
            { Button.Cross, false },
            { Button.Circle, false },
            { Button.Square, false },
            { Button.Triangle, false },
            { Button.L1, false },
            { Button.L2, false },
            { Button.R1, false },
            { Button.R2, false }
        };

            isUps = new Dictionary<Button, bool>
        {
            { Button.Cross, false },
            { Button.Circle, false },
            { Button.Square, false },
            { Button.Triangle, false },
            { Button.L1, false },
            { Button.L2, false },
            { Button.R1, false },
            { Button.R2, false }
        };
        }

        public void SetButtonDown(Button button)
        {
            if (Time.unscaledTime - lastInputTimes[button] > inputOverlappingPreventTime)
            {
                isDowns[button] = true;
                lastInputTimes[button] = Time.unscaledTime;
            }
        }

        public override bool GetButtonDown(Button button)
        {
            bool retVal = isDowns[button];
            isDowns[button] = false;

            if (retVal)
                isUps[button] = true;

            return retVal;
        }
        public override bool GetButtonUp(Button button)
        {
            bool retVal = isUps[button];
            isUps[button] = false;
            return retVal;
        }

        public void SetLeftStick(Vector2 value)
        {
            leftStick = value;
        }

        public void SetRightStick(Vector2 value)
        {
            rightStick = value;
        }

        public override Vector2 GetLeftStick()
        {
            Vector2 retVal = leftStick;
            leftStick = Vector2.zero;
            return retVal;
        }

        public override Vector2 GetRightStick()
        {
            Vector2 retVal = rightStick;
            rightStick = Vector2.zero;
            return retVal;
        }
    }
}