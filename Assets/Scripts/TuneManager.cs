using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class TuneManager : MonoBehaviour
{
    #region Variables
    public Player PlayerInputRef;

    private float tuneIntensity;
    [SerializeField]
    private float currentNum, targetNum = 0f, rangeNum, minNum = 0f, maxNum = 0f;
    private bool isCompleted;

    public GameObject Mouse;
    public GameObject Controller;
    public Image WaveImage;
    public Image CircleImage;
    public Animator WaveAnimator;
    public Animator CircleAnimator;

    #endregion


    public void TuneMinigame()
    {
        SetInputs();

        currentNum = Random.Range(minNum, maxNum + 1);
        isCompleted = false;

        //code to find control scheme in use
        if (PlayerInputRef.LastInputDevice == "Keyboard" || PlayerInputRef.LastInputDevice == "Mouse")
        {
            Controller.SetActive(false);
            Mouse.SetActive(true);
            Mouse.GetComponent<Animator>().SetInteger("Input", 0);
        }
        else
        {
            Mouse.SetActive(false);
            Controller.SetActive(true);
            Controller.GetComponent<Animator>().SetInteger("Input", 1);
        }
    }

    #region Inputs
    private void SetInputs()
    {
        PlayerInputRef.Input.asset.FindActionMap("UI").Enable();

        PlayerInputRef.Input.UI.Tune.performed += context =>
        {
            tuneIntensity = context.ReadValue<float>();
            PlayerInputRef.LastInputDevice = context.control.device.name;
        };
        PlayerInputRef.Input.UI.Tune.canceled += context => tuneIntensity = 0f;
    }


    private void OnEnable()
    {
        PlayerInputRef.Input.UI.Enable();
    }

    private void OnDisable()
    {
        PlayerInputRef.Input.UI.Disable();
    }

    #endregion

    #region TuneGameplay

    public void Tuning()
    {
        if (currentNum >= minNum && currentNum <= maxNum)
        {
            currentNum += tuneIntensity;
        }
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
        //to calculate distance between current and target
        rangeNum = currentNum - targetNum;

        WaveAnimator.SetFloat("RangeTune", rangeNum);

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

    public void ChangeColor(string code)
    {
        Color color;
        if (ColorUtility.TryParseHtmlString(code, out color))
        { 
            WaveImage.color = color;
            CircleImage.color = color;
        }
    }

    public IEnumerator EndTuneMinigame()
    {
        Debug.Log("Complete");
        PlayerInputRef.Input.asset.FindActionMap("UI").Disable();

        if (PlayerInputRef.LastInputDevice == "Keyboard" || PlayerInputRef.LastInputDevice == "Mouse")
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

        PlayerInputRef.Input.asset.FindActionMap("Player").Enable();
        gameObject.SetActive(false);
    }

    

    public void Update()
    {
        Tuning();

        TuningVerification();
    }
}
