using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleToAnimator : MonoBehaviour
{
    Animator animator;

    public bool IsOn { set
        {
            animator.SetBool("IsOn", value);
        }
    }
    public bool IsInteractable
    {
        set
        {
            animator.SetBool("IsInteractable", value);
        }
    }

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }
}
