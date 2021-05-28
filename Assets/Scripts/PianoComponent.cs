using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PianoComponent : MonoBehaviour
{
    private Animator KeyAnimator;

    public bool IsRepaired = false;
    public int index = 0;

    private void Awake()
    {
        KeyAnimator = gameObject.GetComponent<Animator>();
    }


    public void RepairComponent()
    {
        IsRepaired = true;
        Manager.ManagerInstance.RepairedKeys[index] = true;
        SetRepair();
    }

    public void DestroyComponent()
    {
        IsRepaired = false;
        Manager.ManagerInstance.RepairedKeys[index] = false;
        SetDestroy();
    }

    //just to guarantee that the models appearance match their status
    public void SetRepair()
    {
        KeyAnimator.Play("SetRepair", 0);
    }

    public void SetDestroy()
    {
        KeyAnimator.Play("SetDestroy", 0);
    }

    
}
