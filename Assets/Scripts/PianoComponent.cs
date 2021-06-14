using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PianoComponent : MonoBehaviour
{
    public MeshRenderer ComponentMaterial;

    public Material RepairedMaterial;
    public Material DestroyedMaterial;

    //the Keys pivots are messed up so we need to at least offset the x so that the AI or the Waypoint can target the right position
    //the Pins don't actually need this but I don't want to be always verifying if the component is a Key or not
    public Vector3 ComponentRealPosition;

    public bool IsRepaired = false;
    public int index = 0;

    private void Awake()
    {
        //get mesh renderer of Pin, if Key get mesh renderer of the "Tecla" of the Key
        if (gameObject.CompareTag("Pin"))
        {
            ComponentMaterial = gameObject.GetComponent<MeshRenderer>();
        }
        else
        {
            ComponentMaterial = transform.GetChild(transform.childCount - 2).gameObject.GetComponent<MeshRenderer>();
        }

        ComponentRealPosition = new Vector3(transform.position.x + GetComponent<BoxCollider>().center.x, transform.position.y, transform.position.z);
    }

    public void RepairComponent()
    {
        IsRepaired = true;

        ComponentMaterial.material = RepairedMaterial;

        UpdateComponentArray();

        FindObjectOfType<Waypoint>().AssignSuggestion();
    }

    public void DestroyComponent()
    {
        IsRepaired = false;

        ComponentMaterial.material = DestroyedMaterial;

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
