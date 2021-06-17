using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using FMODUnity;

public class TuneManager : MonoBehaviour
{
    #region Variables
    [SerializeField]
    private float targetNum = 0f, //value that the player must reach to successfully tune the pin
    minNum = 0f, //lower tune limit
    maxNum = 0f, //upper tune limit
    timeSinceLastPinTest = 0f,//time since a piano key was pressed to test the pin
    pinLevel; //determines how the test pin sound sounds


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

    private FMOD.Studio.EventInstance tuningPinInstance;
    private FMOD.Studio.EventInstance testingPinInstance;
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

            //setup and start the sound that will play when tuning the pin (not to confuse with the one used to test the pin)
            tuningPinInstance = RuntimeManager.CreateInstance("event:/SFX/Tune Minigame/Tuning Pin");
            tuningPinInstance.set3DAttributes(RuntimeUtils.To3DAttributes(PinStatus.gameObject));
            tuningPinInstance.start();

            //the sound is paused until the player starts tuning
            //setPaused makes it so that the sound can continue where it was paused
            tuningPinInstance.setPaused(true);

            //setup the sound that will play once in a while to check the pin of the piano
            testingPinInstance = RuntimeManager.CreateInstance("event:/SFX/Tune Minigame/Testing Pin");
        }
    }

    private void SetInputs()
    {
        playerInputRef.Input.Player.Disable();
        playerInputRef.Input.TuneMinigame.Enable();

        playerInputRef.Input.TuneMinigame.Tune.performed += context =>
        {
            tuneIntensity = context.ReadValue<float>();
            tuningPinInstance.setPaused(false);
            playerInputRef.LastInputDevice = context.control.device.name;
        };

        playerInputRef.Input.TuneMinigame.Tune.canceled += context =>
        {
            tuneIntensity = 0f;
            tuningPinInstance.setPaused(true);
        };
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
            pinLevel = Mathf.Clamp(rangeNum, -4, 4);
        }
        else if ((rangeNum >= -40 && rangeNum <= -30) || (rangeNum <= 40 && rangeNum >= 30))
        {
            ChangeColor("#FF3369");
            pinLevel = Mathf.Clamp(rangeNum, -3, 3);
        }
        else if ((rangeNum >= -30 && rangeNum <= -20) || (rangeNum <= 30 && rangeNum >= 20))
        {
            ChangeColor("#FF668E");
            pinLevel = Mathf.Clamp(rangeNum, -2, 2);
        }
        else if ((rangeNum >= -20 && rangeNum <= -10) || (rangeNum <= 20 && rangeNum >= 10))
        {
            ChangeColor("#FF99B4");
            pinLevel = Mathf.Clamp(rangeNum, -1, 1);
        }
        else if ((rangeNum >= -10 && rangeNum <= -1) || (rangeNum <= 10 && rangeNum >= 1))
        {
            ChangeColor("#FFCCD9");
            pinLevel = 0;
        }
    }

    #endregion

    private void ChangeColor(string code) //hex > rgb
    {
        Color color;
        if (ColorUtility.TryParseHtmlString(code, out color))
        { 
            waveImage.color = color;
            circleImage.color = color;
        }
    }

    private void TestPin()
    {
        timeSinceLastPinTest += Time.deltaTime;

        if (timeSinceLastPinTest >= 10f)
        {
            testingPinInstance.setParameterByName("PinTest", pinLevel);
            testingPinInstance.start();
            timeSinceLastPinTest = 0f;
        }
    }

    public IEnumerator EndTuneMinigame()
    {
        Debug.Log("Complete");

        testingPinInstance.start();

        playerInputRef.Input.TuneMinigame.Disable();
        tuningPinInstance.release();
        testingPinInstance.release();

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

        PinStatus.RepairComponent();

        playerInputRef.Input.Player.Enable();
        gameObject.SetActive(false);
    }

    

    public void Update()
    {
        TestPin();

        Tuning();

        TuningVerification();
    }
}
