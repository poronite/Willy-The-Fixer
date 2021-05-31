using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PianoComponent : MonoBehaviour
{
    public Animator KeyAnimator;

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
        GameObject.FindObjectOfType<Waypoint>().AssignSuggestion();
    }

    public void DestroyComponent()
    {
        IsRepaired = false;
        Manager.ManagerInstance.RepairedKeys[index] = false;
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
