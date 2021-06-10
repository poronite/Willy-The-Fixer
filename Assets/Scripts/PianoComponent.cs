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
        UpdateComponentArray();

        FindObjectOfType<Waypoint>().AssignSuggestion();
    }

    public void DestroyComponent()
    {
        IsRepaired = false;
        UpdateComponentArray();
    }

    private void UpdateComponentArray()
    {
        switch (gameObject.tag)
        {
            case "Pin":
                Manager.ManagerInstance.RepairedPins[index] = IsRepaired;
                break;
            case "Key":
                Manager.ManagerInstance.RepairedKeys[index] = IsRepaired;
                break;
            default:
                break;
        }
    }
}
