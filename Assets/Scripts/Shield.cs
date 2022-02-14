using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Shield : MonoBehaviour
{
    [SerializeField] [Range(-180f, +180f)] float blockingAngle = 0;
    [SerializeField] [Range(0f, 360f)] float blockingArc = 180f;
    [SerializeField] UnityEvent onFunctionalityEnabled;
    [SerializeField] UnityEvent onFunctionalityDisabled;

    public float RotatedBlockingAngle => (transform.rotation.eulerAngles.z + blockingAngle) * ((transform.eulerAngles.y == 0)? 1:-1);
    public float BlockingAngle => blockingAngle;
    public float BlockingArc => blockingArc;

    public bool IsFunctionalityEnabled { get; private set; } = true;

    public void EnableFunctionality(bool enable)
    {
        if (IsFunctionalityEnabled && !enable)
        {
            IsFunctionalityEnabled = enable;
            onFunctionalityDisabled.Invoke();
        }
        else if (!IsFunctionalityEnabled && enable)
        {
            IsFunctionalityEnabled = enable;
            onFunctionalityEnabled.Invoke();
        }
    }
}

#if UNITY_EDITOR
[UnityEditor.CustomEditor(typeof(Shield))]
[UnityEditor.CanEditMultipleObjects]
public class ShieldEditor : UnityEditor.Editor
{
    public void OnSceneGUI()
    {
        var shield = target as Shield;
        var position = shield.transform.position;
        UnityEditor.Handles.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);

        var center = position;
        var normal = shield.transform.rotation * Vector3.forward;
        var from = Quaternion.Euler(0, 0, shield.RotatedBlockingAngle - (shield.BlockingArc / 2)) * Vector3.up;
        var angle = shield.BlockingArc;
        var radius = 1f;
        UnityEditor.Handles.DrawSolidArc(center, normal, from, angle, radius);
    }
}
#endif
