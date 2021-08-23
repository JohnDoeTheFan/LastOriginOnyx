using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Onyx.GameElement
{
    public class Follower : MonoBehaviour
    {

        [SerializeField] private Transform target;
        [SerializeField] private float followSpeed = 0.05f;

        public Mode mode;
        public MinMax2 limit;

        [SerializeField] private bool shouldLimitMinX;
        [SerializeField] private bool shouldLimitMinY;
        [SerializeField] private bool shouldLimitMaxX;
        [SerializeField] private bool shouldLimitMaxY;
        [SerializeField] private Vector2 margin;

        public Vector2 Margin => margin;

        // Update is called once per frame
        void Update()
        {
            if (target != null)
            {
                switch (mode)
                {
                    case Mode.Teleport:
                        Teleport();
                        break;
                    case Mode.Follow:
                        Follow();
                        break;
                    default:
                        break;
                }
            }
        }

        public void SetTarget(Transform target)
        {
            this.target = target;
        }

        void Teleport()
        {
            Vector3 translation = CalcTranslation();

            transform.Translate(translation);
        }

        void Follow()
        {
            Vector3 translation = CalcTranslation();

            if (Vector3.Magnitude(translation) > followSpeed)
                transform.Translate(translation.normalized * followSpeed);
            else
                transform.Translate(translation);
        }

        Vector3 CalcTranslation()
        {
            Vector3 targetPosition = target.transform.position;
            targetPosition.x += margin.x;
            targetPosition.y += margin.y;
            targetPosition = LimitX(targetPosition);
            targetPosition = LimitY(targetPosition);
            targetPosition.z = transform.position.z;

            return targetPosition - transform.position;
        }

        Vector3 LimitY(Vector3 value)
        {
            if (shouldLimitMinY && shouldLimitMaxY && limit.min.y > limit.max.y)
                value.y = (limit.min.y + limit.max.y) / 2;
            else
            {
                if (shouldLimitMinY)
                    value.y = Mathf.Max(value.y, limit.min.y);
                if (shouldLimitMaxY)
                    value.y = Mathf.Min(value.y, limit.max.y);
            }

            return value;
        }

        Vector3 LimitX(Vector3 value)
        {
            if (shouldLimitMinX && shouldLimitMaxX && limit.min.x > limit.max.x)
                value.x = (limit.min.x + limit.max.x) / 2;
            else
            {
                if (shouldLimitMinX)
                    value.x = Mathf.Max(value.x, limit.min.x);
                if (shouldLimitMaxX)
                    value.x = Mathf.Min(value.x, limit.max.x);
            }

            return value;
        }

        public void SetShouldLimit(bool minX, bool minY, bool maxX, bool maxY)
        {
            shouldLimitMinX = minX;
            shouldLimitMinY = minY;
            shouldLimitMaxX = maxX;
            shouldLimitMaxY = maxY;
        }

        public void SetMargin(Vector2 margin)
        {
            this.margin = margin;
        }

        public void SetShouldLimit(bool all)
        {
            SetShouldLimit(all, all, all, all);
        }
        public enum Mode
        {
            Teleport,
            Follow
        }
    }
}