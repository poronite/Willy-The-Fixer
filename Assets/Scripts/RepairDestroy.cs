using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class RepairDestroy : MonoBehaviour
{
    #region Variables
    [SerializeField]
    private Player PlayerInputRef = null;

    private float repairProgress = 0.0f;
    private bool fixingFase = false;
    private Animator TestAnimator;

    public bool IsRepaired = false;
    #endregion

    public void StartRepairMinigame()
    {
        if (!IsRepaired)
        {
            TestAnimator = GetComponent<Animator>();
            SetInputs();
        }
    }

    private void SetInputs()
    {
        PlayerInputRef.Input.Player.Disable();
        PlayerInputRef.Input.RepairMinigame.Enable();

        PlayerInputRef.Input.RepairMinigame.Repair.performed += context => 
        {
            if (!fixingFase)
            {
                TriggerRepairFase();
            }
        };

        PlayerInputRef.Input.RepairMinigame.Cancel.performed += context => CancelRepair();
    }

    private void TriggerRepairFase()
    {
        TestAnimator.enabled = true;
        TestAnimator.SetTrigger("Repair");

        //play animation from checkpoint
        TestAnimator.Play("repairTest", 0, repairProgress);

        //prevent player from spamming button
        fixingFase = true;
    }

    private void CancelRepair()
    {
        PlayerInputRef.Input.RepairMinigame.Disable();
        PlayerInputRef.Input.Player.Enable();
    }

    public void FaseComplete()
    {
        //get current animation
        AnimatorStateInfo currentAnimation = TestAnimator.GetCurrentAnimatorStateInfo(0);

        //get repair progress and add 1 frame to it 
        //(so that it doesn't trigger the same event next time it plays)
        repairProgress = currentAnimation.normalizedTime + (1 / (currentAnimation.length * 60));

        //stop animation
        TestAnimator.enabled = false;
        TestAnimator.ResetTrigger("Repair");

        //allow player to press again
        fixingFase = false;
    }

    public void CompleteRepair()
    {
        Debug.Log("Complete Repair");
        IsRepaired = true;
        TestAnimator.SetBool("isRepaired", true);
        CancelRepair();
    }

    public void CompleteDestroy()
    {
        IsRepaired = false;
        repairProgress = 0.0f;
        TestAnimator.SetBool("isRepaired", false);
    }
}
