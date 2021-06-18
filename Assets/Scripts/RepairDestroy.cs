using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using FMODUnity;

public class RepairDestroy : MonoBehaviour
{
    #region Variables
    [SerializeField]
    private Player PlayerInputRef = null;
    [SerializeField]
    private Slider QuickTimeSlider = null;
    [SerializeField]
    private GameObject SliderGlow = null, EKeyPrompt = null, SquareKeyPrompt = null;
    [SerializeField]
    private Animator WillyAnimator = null;

    private float repairProgress, 
    timeLeft, 
    clickableAreaStart, 
    clickableAreaEnd;

    private bool fixingPhase = false;
    [SerializeField]
    private bool potentialPhase = false;
    private PianoComponent KeyStatus;

    //this is to aim the camera when fixing the Key
    private Transform KeyCameraTarget = null;
    //this is to return the camera back to the player after ending minigame
    [SerializeField]
    private Transform PlayerHeadCameraTarget = null;

    private string deviceInUse;

    [HideInInspector]
    public Animator KeyAnimator;
    #endregion

    private void Awake()
    {
        KeyAnimator = GetComponent<Animator>();
    }

    private void Start()
    {
        KeyCameraTarget = transform.GetChild(transform.childCount - 1);
    }

    public void StartKeyMinigame(string device)
    {
        deviceInUse = device;

        KeyStatus = gameObject.GetComponent<PianoComponent>();

        if (!KeyStatus.IsRepaired)
        {
            repairProgress = 0.0f;
            
            SetInputs();

            WillyAnimator.Play("StartFix", 0);
            PlayerInputRef.transform.LookAt(new Vector3(KeyStatus.ComponentRealPosition.x, PlayerInputRef.transform.position.y, KeyStatus.ComponentRealPosition.z));

            Manager.ManagerInstance.ChangeCameraTarget(KeyCameraTarget);
            Manager.ManagerInstance.ChangeCameraY(3.5f);

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
            if (potentialPhase)
            {
                fixingPhase = false;
                TriggerRepairPhase();

                FMOD.Studio.EventInstance quickTimeSuccessInstance;

                quickTimeSuccessInstance = RuntimeManager.CreateInstance("event:/SFX/Key Minigame/Quick Time Event Success");
                quickTimeSuccessInstance.set3DAttributes(RuntimeUtils.To3DAttributes(gameObject));
                quickTimeSuccessInstance.start();
                quickTimeSuccessInstance.release();
            }
            else if (!potentialPhase)
            {
                CancelRepair();
            }
        };
    }

    private void TriggerRepairPhase()
    {
        StopCoroutine("QuickTimeEvent");
        QuickTimeSlider.gameObject.SetActive(false);
        KeyAnimator.enabled = true;

        //play animation from checkpoint
        KeyAnimator.Play("Repair", 0, repairProgress);
    }

    private void CancelRepair()
    {
        SetDestroy();
        StopMinigame();       
    }

    private void StopMinigame()
    {
        StopCoroutine("QuickTimeEvent");

        WillyAnimator.SetTrigger("StopRepair");

        Manager.ManagerInstance.ChangeCameraTarget(PlayerHeadCameraTarget);
        Manager.ManagerInstance.ChangeCameraY(1f);

        QuickTimeSlider.gameObject.SetActive(false);
        PlayerInputRef.Input.RepairMinigame.Disable();
        PlayerInputRef.Input.Player.Enable();
    }

    public void PhaseComplete()
    {
        //get current animation
        AnimatorStateInfo currentAnimation = KeyAnimator.GetCurrentAnimatorStateInfo(0);

        //get repair progress and add 1 frame to it 
        //(so that it doesn't trigger the same event next time it plays)
        repairProgress = currentAnimation.normalizedTime + (1 / (currentAnimation.length * 60));

        //stop animation
        KeyAnimator.enabled = false;

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
        fixingPhase = true;

        while (timeLeft >= 0)
        {
            timeLeft -= Time.deltaTime * speed;
            QuickTimeSlider.value = Mathf.Lerp(QuickTimeSlider.value, timeLeft, QuickTimeSlider.value / timeLeft);

            //activate glow of handle to show that it's time to repair
            //button sprite also changes to show the player which button to press depending on device
            if (fixingPhase && timeLeft <= clickableAreaStart && timeLeft >= clickableAreaEnd)
            {
                SliderGlow.SetActive(true);

                if (deviceInUse == "Mouse" || deviceInUse == "Keyboard")
                {
                    EKeyPrompt.SetActive(true);
                }
                else
                {
                    SquareKeyPrompt.SetActive(true);
                }

                potentialPhase = true;
            }
            else
            {
                SliderGlow.SetActive(false);

                EKeyPrompt.SetActive(false);
                SquareKeyPrompt.SetActive(false);

                potentialPhase = false;
            }

            yield return null;
        }

        if (timeLeft <= 0f)
        {
            //after x seconds player can't press button
            fixingPhase = false;
            CancelRepair();
        }
    }

    public void CompleteRepair()
    {
        FMOD.Studio.EventInstance RepairComplete;

        RepairComplete = RuntimeManager.CreateInstance("event:/SFX/Key Minigame/Repair Complete");
        RepairComplete.set3DAttributes(RuntimeUtils.To3DAttributes(gameObject));
        RepairComplete.start();
        RepairComplete.release();

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
