using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PianoComponent : MonoBehaviour
{
    //the Keys pivots are messed up so we need to at least offset the x so that the AI or the Waypoint can target the right position
    //the Pins don't actually need this but I don't want to be always verifying if the component is a Key or not
    public Vector3 ComponentRealPosition; 

    public bool IsRepaired = false;
    public int index = 0;

    private void Awake()
    {
        ComponentRealPosition = new Vector3(transform.position.x + GetComponent<BoxCollider>().center.x, transform.position.y, transform.position.z);
    }

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
