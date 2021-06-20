using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PianoComponent : MonoBehaviour
{
    [HideInInspector]
    public MeshRenderer ComponentMaterial;
    [HideInInspector]
    public Animator KeyAnimator;

    public Material RepairedMaterial;
    public Material DestroyedMaterial;

    //the Keys pivots are messed up so we need to at least offset the x so that the AI or the Waypoint can target the right position
    //the Pins don't actually need this but just to be safe
    public Vector3 ComponentRealPosition;

    [SerializeField] 
    private KeyMinigame KeyMinigameUI = null;

    public bool IsRepaired = false;
    public int index = 0;

    private void Awake()
    {
        //get mesh renderer of Pin, if Key get mesh renderer of the child "Tecla" of the Key
        if (gameObject.CompareTag("Pin"))
        {
            ComponentMaterial = gameObject.GetComponent<MeshRenderer>();
        }
        else
        {
            ComponentMaterial = transform.GetChild(transform.childCount - 2).gameObject.GetComponent<MeshRenderer>();
            KeyAnimator = gameObject.GetComponent<Animator>();
        }

        ComponentRealPosition = new Vector3(transform.position.x + GetComponent<BoxCollider>().center.x, transform.position.y, transform.position.z);
    }

    public void RepairComponent()
    {
        IsRepaired = true;

        ComponentMaterial.material = RepairedMaterial;

        UpdateComponentArray();

        UpdateNumRepaired();

        Manager.ManagerInstance.IsGameWon();
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


    public void SetRepair()
    {
        KeyAnimator.Play("SetRepair", 0);
    }

    public void SetDestroy()
    {
        KeyAnimator.Play("SetDestroy", 0);
    }

    public void OnRepairPhaseComplete()
    {
        AnimatorStateInfo currentAnimation = KeyAnimator.GetCurrentAnimatorStateInfo(0);
        //get repair progress and add 1 frame to it 
        //(so that it doesn't trigger the same event next time it plays)
        //then send it to Key Minigame
        KeyMinigameUI.RepairPhaseComplete(currentAnimation.normalizedTime + (1 / (currentAnimation.length * 60)));
    }

    public void OnCompleteRepair()
    {
        KeyMinigameUI.CompleteRepair();
    }
}
