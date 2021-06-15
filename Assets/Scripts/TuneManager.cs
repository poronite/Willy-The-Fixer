using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class TuneManager : MonoBehaviour
{
    #region Variables

    [SerializeField]
    private float targetNum = 0f, //value that the player must reach to successfully tune the pin
    minNum = 0f, //lower tune limit
    maxNum = 0f; //upper tune limit

    [SerializeField]
    private Player playerInputRef = null;

    [SerializeField]
    private GameObject mouse = null, controller = null;

    [SerializeField]
    private Image waveImage = null, circleImage = null;
    
    [SerializeField]
    private Animator waveAnimator = null, WillyAnimator = null;

    private float currentNum, //value that the player will be moving
    rangeNum, //distance between currentNum and targetNum
    tuneIntensity; //amount to move each time player presses the button

    private bool isCompleted;

    private PianoComponent PinStatus;
    #endregion


    public void StartTuneMinigame(GameObject pin)
    {
        PinStatus = pin.GetComponent<PianoComponent>();

        if (!PinStatus.IsRepaired)
        {
            SetInputs();

            //setup the tuning session
            currentNum = Random.Range(minNum, maxNum + 1);
            isCompleted = false;

            WillyAnimator.Play("StartFix", 0);
            playerInputRef.transform.LookAt(new Vector3(PinStatus.ComponentRealPosition.x, playerInputRef.transform.position.y, PinStatus.ComponentRealPosition.z));

            //code to find control scheme in use
            if (playerInputRef.LastInputDevice == "Keyboard" || playerInputRef.LastInputDevice == "Mouse")
            {
                controller.SetActive(false);
                mouse.SetActive(true);
                mouse.GetComponent<Animator>().SetInteger("Input", 0);
            }
            else
            {
                mouse.SetActive(false);
                controller.SetActive(true);
                controller.GetComponent<Animator>().SetInteger("Input", 1);
            }
        }
    }

    private void SetInputs()
    {
        playerInputRef.Input.Player.Disable();
        playerInputRef.Input.TuneMinigame.Enable();

        playerInputRef.Input.TuneMinigame.Tune.performed += context =>
        {
            tuneIntensity = context.ReadValue<float>();
            playerInputRef.LastInputDevice = context.control.device.name;
        };

        playerInputRef.Input.TuneMinigame.Tune.canceled += context => tuneIntensity = 0f;
    }

    #region TuneGameplay

    public void Tuning()
    {
        //inside limits
        if (currentNum >= minNum && currentNum <= maxNum) 
        {
            currentNum += tuneIntensity;
        }
        //prevent from going outside the limits
        else if (currentNum < minNum)
        {
            currentNum = minNum;
        }
        else if (currentNum > maxNum)
        {
            currentNum = maxNum;
        }
    }

    public void TuningVerification()
    {
        //calculate distance between current and target
        rangeNum = currentNum - targetNum;

        //small wave animation that is in the middle of the circle
        waveAnimator.SetFloat("RangeTune", rangeNum);

        //Set Color depending on distance to target
        if (rangeNum <= 1 && rangeNum >= -1 && isCompleted == false)
        {
            ChangeColor("#FFFFFF");
            isCompleted = true;
            StartCoroutine(EndTuneMinigame());
        }
        else if ((rangeNum >= -50 && rangeNum <= -40) || (rangeNum <= 50 && rangeNum >= 40))
        {
            ChangeColor("#FF0037");
        }
        else if ((rangeNum >= -40 && rangeNum <= -30) || (rangeNum <= 40 && rangeNum >= 30))
        {
            ChangeColor("#FF3369");
        }
        else if ((rangeNum >= -30 && rangeNum <= -20) || (rangeNum <= 30 && rangeNum >= 20))
        {
            ChangeColor("#FF668E");
        }
        else if ((rangeNum >= -20 && rangeNum <= -10) || (rangeNum <= 20 && rangeNum >= 10))
        {
            ChangeColor("#FF99B4");
        }
        else if ((rangeNum >= -10 && rangeNum <= -1) || (rangeNum <= 10 && rangeNum >= 1))
        {
            ChangeColor("#FFCCD9");
        }
    }

    #endregion

    public void ChangeColor(string code) //hex > rgb
    {
        Color color;
        if (ColorUtility.TryParseHtmlString(code, out color))
        { 
            waveImage.color = color;
            circleImage.color = color;
        }
    }

    public IEnumerator EndTuneMinigame()
    {
        Debug.Log("Complete");
        PinStatus.RepairComponent();

        playerInputRef.Input.TuneMinigame.Disable();

        WillyAnimator.SetTrigger("StopRepair");
        if (playerInputRef.LastInputDevice == "Keyboard" || playerInputRef.LastInputDevice == "Mouse")
        {
            yield return new WaitForSeconds(2.0f);
        }
        else
        {
            Gamepad.current.SetMotorSpeeds(0.2f, 0.2f);
            yield return new WaitForSeconds(1.0f);
            Gamepad.current.SetMotorSpeeds(0.0f, 0.0f);
            yield return new WaitForSeconds(1.0f);
        }

        playerInputRef.Input.Player.Enable();
        gameObject.SetActive(false);
    }

    

    public void Update()
    {
        Tuning();

        TuningVerification();
    }
}
