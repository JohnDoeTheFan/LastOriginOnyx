using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Onyx.GameElement
{
    public class ActivatorByPlatform : MonoBehaviour
    {
        [SerializeField]
        RuntimePlatform platform;
        [SerializeField]
        List<GameObject> objectsToActivate;
        [SerializeField]
        List<GameObject> objectsToDeactivate;

        void Start()
        {
            RuntimePlatform currentPlatform = Application.platform;
            if (currentPlatform == RuntimePlatform.WindowsEditor)
                currentPlatform = RuntimePlatform.WindowsPlayer;

            if (currentPlatform == platform)
            {
                foreach (GameObject objectToActivate in objectsToActivate)
                    objectToActivate.SetActive(true);
                foreach (GameObject objectToDeactivate in objectsToDeactivate)
                    objectToDeactivate.SetActive(false);
            }
        }
    }

}
