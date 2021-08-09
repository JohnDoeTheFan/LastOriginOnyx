using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Onyx.GameElement
{
    public class DoNotDestory : MonoBehaviour
    {
        // Start is called before the first frame update
        void Awake()
        {
            DontDestroyOnLoad(this);
        }

    }
}