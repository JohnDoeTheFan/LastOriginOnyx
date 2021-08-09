using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestoryAfterSeconds : MonoBehaviour
{
    [SerializeField]
    private float seconds;

    void Start()
    {
        StartCoroutine(DelayedDestory(seconds));
    }

    IEnumerator DelayedDestory(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        Destroy(gameObject);
    }
}
