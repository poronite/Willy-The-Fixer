using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PianoComponent : MonoBehaviour
{
    [HideInInspector]
    public MeshRenderer ComponentMaterial;

    [SerializeField]
    private Animator WillyAnimator = null;

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

        WillyAnimator.SetTrigger("StopRepair");

        UpdateComponentArray();

        UpdateNumRepaired();

        IsGameWon();

        FindObjectOfType<Waypoint>().AssignSuggestion();
    }

    public void DestroyComponent()
    {
        IsRepaired = false;

        ComponentMaterial.material = DestroyedMaterial;

        UpdateComponentArray();

        UpdateNumRepaired();
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

    private void UpdateNumRepaired()
    {
        switch (gameObject.tag)
        {
            case "Pin":
                if (IsRepaired)
                {
                    Manager.ManagerInstance.NumRepairedPins++;
                }
                else
                {
                    Manager.ManagerInstance.NumRepairedPins--;
                }
                break;
            case "Key":
                if (IsRepaired)
                {
                    Manager.ManagerInstance.NumRepairedKeys++;
                }
                else
                {
                    Manager.ManagerInstance.NumRepairedKeys--;
                }
                break;
            default:
                break;
        }
    }

    private void IsGameWon()
    {
        if (Manager.ManagerInstance.NumRepairedPins == 233 && Manager.ManagerInstance.NumRepairedKeys == 88)
        {
            StartCoroutine(Manager.ManagerInstance.GameClear());
        }
    }
}
