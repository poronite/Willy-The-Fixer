using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class RepairDestroy : MonoBehaviour
{
    #region Variables
    [SerializeField]
    private Player PlayerInputRef = null;
    [SerializeField]
    private Slider QuickTimeSlider = null;
    [SerializeField]
    private Image SliderKnob = null;

    private float repairProgress, 
    timeLeft, 
    clickableAreaStart, 
    clickableAreaEnd;

    private bool fixingFase = false;
    private bool successFase = false;
    public Animator KeyAnimator;
    private PianoComponent KeyStatus;
    #endregion

    private void Awake()
    {
        KeyAnimator = GetComponent<Animator>();
    }

    public void StartKeyMinigame()
    {
        KeyStatus = gameObject.GetComponent<PianoComponent>();
        if (!KeyStatus.IsRepaired)
        {
            repairProgress = 0.0f;
            
            SetInputs();

            Manager.ManagerInstance.ChangeCameraTarget(gameObject);
            Manager.ManagerInstance.ChangeCameraX(3f);
            Manager.ManagerInstance.ChangeCameraY(5f);

            KeyAnimator.Play("SetDestroy", 0);

            QuickTimeSlider.gameObject.SetActive(true);
            StartCoroutine("QuickTimeEvent");
        }
    }

    private void SetInputs()
    {
        PlayerInputRef.Input.Player.Disable();
        PlayerInputRef.Input.RepairMinigame.Enable();

        PlayerInputRef.Input.RepairMinigame.Repair.performed += context => 
        {
            if (fixingFase && timeLeft <= clickableAreaStart && timeLeft >= clickableAreaEnd)
            {
                successFase = true;
            }
            else if (fixingFase && timeLeft > clickableAreaStart && timeLeft < clickableAreaEnd)
            {
                CancelRepair();
            }
        };

        PlayerInputRef.Input.RepairMinigame.Cancel.performed += context => CancelRepair();
    }

    private void TriggerRepairFase()
    {
        StopCoroutine("QuickTimeEvent");
        QuickTimeSlider.gameObject.SetActive(false);
        KeyAnimator.enabled = true;

        //play animation from checkpoint
        KeyAnimator.Play("Repair", 0, repairProgress);
    }

    private void CancelRepair()
    {
        KeyAnimator.Play("SetDestroy", 0);
        StopMinigame();       
    }

    private void StopMinigame()
    {
        StopCoroutine("QuickTimeEvent");

        Manager.ManagerInstance.ChangeCameraTarget(PlayerInputRef.gameObject);
        Manager.ManagerInstance.ChangeCameraX(0f);
        Manager.ManagerInstance.ChangeCameraY(5f);

        QuickTimeSlider.gameObject.SetActive(false);
        PlayerInputRef.Input.RepairMinigame.Disable();
        PlayerInputRef.Input.Player.Enable();
    }

    public void FaseComplete()
    {
        //get current animation
        AnimatorStateInfo currentAnimation = KeyAnimator.GetCurrentAnimatorStateInfo(0);

        //get repair progress and add 1 frame to it 
        //(so that it doesn't trigger the same event next time it plays)
        repairProgress = currentAnimation.normalizedTime + (1 / (currentAnimation.length * 60));

        //stop animation
        KeyAnimator.enabled = false;
        successFase = false;

        //start quicktime event again
        QuickTimeSlider.gameObject.SetActive(true);
        StartCoroutine("QuickTimeEvent");
    }

    private IEnumerator QuickTimeEvent()
    {
        float speed = 2f;

        timeLeft = 5f;
        QuickTimeSlider.maxValue = timeLeft;
        QuickTimeSlider.value = timeLeft;

        clickableAreaStart = Random.Range(1f, timeLeft - 0.5f);
        clickableAreaEnd = clickableAreaStart - 1f;

        //player is able to press button
        fixingFase = true;

        while (timeLeft >= 0)
        {
            timeLeft -= Time.deltaTime * speed;
            QuickTimeSlider.value = Mathf.Lerp(QuickTimeSlider.value, timeLeft, QuickTimeSlider.value / timeLeft);

            //change color of knob to show that button can be pressed
            if (fixingFase && (timeLeft <= clickableAreaStart && timeLeft >= clickableAreaEnd))
            {
                SliderKnob.color = Color.green;
            }
            else
            {
                SliderKnob.color = Color.red;
            }

            if (successFase == true)
            {
                fixingFase = false;
                TriggerRepairFase();
            }

            yield return null;
        }

        if (successFase == false && timeLeft <= 0f)
        {
            //after x seconds player can't press button
            fixingFase = false;
            CancelRepair();
        }
    }

    public void CompleteRepair()
    {
        StopMinigame();
        KeyStatus.RepairComponent();
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
