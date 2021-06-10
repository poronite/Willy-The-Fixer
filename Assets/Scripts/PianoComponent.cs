using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PianoComponent : MonoBehaviour
{
    public bool IsRepaired = false;
    public int index = 0;


    public void RepairComponent()
    {
        IsRepaired = true;
        Manager.ManagerInstance.RepairedKeys[index] = true;
        FindObjectOfType<Waypoint>().AssignSuggestion();
    }

    public void DestroyComponent()
    {
        IsRepaired = false;
        Manager.ManagerInstance.RepairedKeys[index] = false;
    }
}
