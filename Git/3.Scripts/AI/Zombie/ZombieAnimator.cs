using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class ZombieAnimator : NetworkBehaviour
{
    [SerializeField]
    private Animator zombieAnimator;
    private bool isMoving = false;
    private bool isHaveShield = false;

    public bool IsMoving
    {
        set
        {
            isMoving = value;
            zombieAnimator.SetBool("IsMoving", isMoving);
        }
    }

    public bool IsHaveShield
    {
        set
        {
            isMoving = value;
            zombieAnimator.SetBool("HaveShield", isMoving);
        }
    }

    public override void OnStartClient()
    {
        if (GetComponent<PuppetMasterController>() != null)
            zombieAnimator.enabled = false;
    }

    public void SetTrigger(string _name)
    {
        zombieAnimator.SetTrigger(_name);
    }

    public void SetAnimatorState(bool _state)
    {
        try
        {
            zombieAnimator.enabled = _state;
        }
        catch(Exception e)
        { 
            Debug.LogError($"Exception: {e.Message} | {e.StackTrace}");
        }
    }
}
