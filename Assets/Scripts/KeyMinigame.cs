using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FMODUnity;

public class KeyMinigame : MonoBehaviour
{
    #region Variables
    //player
    [SerializeField]
    private Player Player = null;
    [SerializeField]
    private Transform PlayerHeadCameraTarget = null;
    [SerializeField]
    private Animator WillyAnimator = null;

    //quick time event slider
    [SerializeField]
    private Slider QuickTimeSlider = null;
    [SerializeField]
    private GameObject SliderGlow = null, EKeyPrompt = null, SquareKeyPrompt = null;
    
    //gameplay variables
    private float repairProgress,
    timeLeft,
    clickableZoneStart,
    clickableZoneEnd;
    private bool onQuickTimeEvent = false,
    insideClickableZone = false;

    [SerializeField]
    private float quickTimeEventSpeed = 0f,
    maxTimeLeft = 0f;

    //key being repaired
    private GameObject Key;
    private Animator KeyAnimator;
    private PianoComponent KeyStatus;
    private Transform KeyCameraTarget;

    private string inputDeviceInUse;

    #endregion

    #region SetupKeyMinigame
    public void StartKeyMinigame(GameObject key, string lastInputDevice)
    {
        inputDeviceInUse = lastInputDevice;

        //setup key stuff
        Key = key;
        KeyAnimator = Key.GetComponent<Animator>();
        KeyStatus = Key.GetComponent<PianoComponent>();

        //camera will be aiming at the wippen of the key during minigame
        KeyCameraTarget = Key.transform.GetChild(Key.transform.childCount - 1);

        if (!KeyStatus.IsRepaired)
        {
            repairProgress = 0.0f; //start over repair

            SetInputs();

            WillyAnimator.Play("StartFix", 0);
            Player.transform.LookAt(new Vector3(KeyStatus.ComponentRealPosition.x, Player.transform.position.y, KeyStatus.ComponentRealPosition.z));

            Manager.ManagerInstance.ChangeCameraTarget(KeyCameraTarget);
            Manager.ManagerInstance.ChangeCameraY(3.5f);

            StartQuickTimeEvent();
        }
    }

    private void SetInputs()
    {
        Player.Input.Player.Disable();
        Player.Input.RepairMinigame.Enable();

        Player.Input.RepairMinigame.Repair.performed += context =>
        {
            if (onQuickTimeEvent)
            {
                onQuickTimeEvent = false;

                if (insideClickableZone)
                {
                    ContinueRepair();

                    FMOD.Studio.EventInstance quickTimeSuccessInstance;

                    quickTimeSuccessInstance = RuntimeManager.CreateInstance("event:/SFX/Key Minigame/Quick Time Event Success");
                    quickTimeSuccessInstance.set3DAttributes(RuntimeUtils.To3DAttributes(gameObject));
                    quickTimeSuccessInstance.start();
                    quickTimeSuccessInstance.release();
                }
                else if (!insideClickableZone)
                {
                    StopRepair();
                }
            }
        };

        Player.Input.RepairMinigame.Cancel.performed += context =>
        {
            if (onQuickTimeEvent)
            {
                EndKeyMinigame();
                KeyStatus.SetDestroy();
            }
        };
    }

    #endregion

    #region QuickTimeEvent

    private void StartQuickTimeEvent()
    {
        timeLeft = maxTimeLeft;
        QuickTimeSlider.maxValue = maxTimeLeft;
        QuickTimeSlider.value = timeLeft;

        //determine clickable zone
        clickableZoneStart = Random.Range(1.01f, maxTimeLeft - 0.5f);
        clickableZoneEnd = clickableZoneStart - 1f;

        QuickTimeSlider.gameObject.SetActive(true);
        SliderGlow.SetActive(false);
        EKeyPrompt.SetActive(false);
        SquareKeyPrompt.SetActive(false);
        insideClickableZone = false;
        onQuickTimeEvent = true;
    }

    private void DuringQuickTimeEvent()
    {
        if (onQuickTimeEvent && timeLeft >= 0)
        {
            timeLeft -= Time.deltaTime * quickTimeEventSpeed;
            QuickTimeSlider.value = Mathf.Lerp(QuickTimeSlider.value, timeLeft, QuickTimeSlider.value / timeLeft);

            //verify if player can click to win this event
            if (timeLeft <= clickableZoneStart && timeLeft >= clickableZoneEnd)
            {
                SliderGlow.SetActive(true);

                if (inputDeviceInUse == "Mouse" || inputDeviceInUse == "Keyboard")
                {
                    EKeyPrompt.SetActive(true);
                }
                else
                {
                    SquareKeyPrompt.SetActive(true);
                }

                insideClickableZone = true;
            }
            else
            {
                SliderGlow.SetActive(false);
                EKeyPrompt.SetActive(false);
                SquareKeyPrompt.SetActive(false);

                insideClickableZone = false;
            }
        }
        else if (onQuickTimeEvent && timeLeft <= 0)
        {
            onQuickTimeEvent = false;
            StopRepair();
        }
    }

    #endregion

    #region ProgressManagement

    private void ContinueRepair()
    {
        QuickTimeSlider.gameObject.SetActive(false);
        KeyAnimator.enabled = true;

        //play animation from checkpoint
        KeyAnimator.Play("Repair", 0, repairProgress);
    }

    public void RepairPhaseComplete(float progress)
    {
        repairProgress = progress;

        //stop animation
        KeyAnimator.enabled = false;

        //start the quick time event again
        StartQuickTimeEvent();
    }

    private void StopRepair()
    {
        EndKeyMinigame();
        KeyStatus.SetDestroy();
    }

    public void CompleteRepair()
    {
        FMOD.Studio.EventInstance RepairComplete;

        RepairComplete = RuntimeManager.CreateInstance("event:/SFX/Key Minigame/Repair Complete");
        RepairComplete.set3DAttributes(RuntimeUtils.To3DAttributes(gameObject));
        RepairComplete.start();
        RepairComplete.release();

        EndKeyMinigame();
        KeyStatus.RepairComponent();
    }

    private void EndKeyMinigame()
    {
        QuickTimeSlider.gameObject.SetActive(false);
        KeyAnimator.enabled = true;

        Manager.ManagerInstance.ChangeCameraTarget(PlayerHeadCameraTarget);
        Manager.ManagerInstance.ChangeCameraY(1f);

        WillyAnimator.SetTrigger("StopRepair");
        Player.Input.RepairMinigame.Disable();
        Player.Input.Player.Enable();
    }

    #endregion

    private void FixedUpdate()
    {
        DuringQuickTimeEvent();
    }
}
