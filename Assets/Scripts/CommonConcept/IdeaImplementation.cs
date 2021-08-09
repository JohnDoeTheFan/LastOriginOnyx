using System;
using System.Collections;
using UnityEngine;

namespace IdeaImplementation
{
    public class MusicBox
    {
        readonly MonoBehaviour coroutineStartProxy;
        readonly private float tickTime;

        readonly Action onTick;
        readonly Action onEnd;

        bool shouldStop = false;

        public enum Type
        {
            DoWhileStyle,
            WhileStyle
        }


        public MusicBox(MyObject coroutineStartProxy, Action onTick, Action onEnd, Type type, float duration, float tickTime)
        {
            this.coroutineStartProxy = coroutineStartProxy;
            this.onTick = onTick;
            this.onEnd = onEnd;
            this.tickTime = tickTime;

            if (type == Type.DoWhileStyle)
                coroutineStartProxy.StartCoroutine(DoWhileStyle(duration));
            else
                coroutineStartProxy.StartCoroutine(WhileStyle(duration));
        }

        ~MusicBox()
        {
            shouldStop = true;
        }

        public void Stop()
        {
            shouldStop = true;
        }

        private IEnumerator DoWhileStyle(float leftTime)
        {
            onTick();

            leftTime -= tickTime;
            if (leftTime >= 0)
            {
                yield return new WaitForSeconds(tickTime);
                if (!shouldStop)
                    coroutineStartProxy.StartCoroutine(DoWhileStyle(leftTime));
            }
            else
            {
                onEnd();
            }
        }
        private IEnumerator WhileStyle(float leftTime)
        {
            leftTime -= tickTime;
            if (leftTime >= 0)
            {
                yield return new WaitForSeconds(tickTime);
                if (!shouldStop)
                {
                    onTick();
                    coroutineStartProxy.StartCoroutine(WhileStyle(leftTime));
                }
            }
            else
            {
                onEnd();
            }
        }
    }

}
