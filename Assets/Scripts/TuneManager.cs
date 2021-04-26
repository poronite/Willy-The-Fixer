using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TuneManager : MonoBehaviour
{
    #region Variables
    public Player PlayerInputRef;
    public Text DistanceDebug;

    private float tuneIntensity;
    [SerializeField]
    private float currentNum, targetNum, rangeNum, minNum = 0f, maxNum = 0f;

    #endregion


    public void TuneMinigame()
    {
        SetInputs();

        currentNum = Random.Range(minNum, maxNum + 1);
        targetNum = Random.Range(minNum, maxNum + 1);
    }

    #region Inputs
    private void SetInputs()
    {
        PlayerInputRef.Input.asset.FindActionMap("UI").Enable();

        PlayerInputRef.Input.UI.Tune.performed += context => tuneIntensity = context.ReadValue<float>();
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
        rangeNum = Mathf.Abs(currentNum - targetNum);

        if (rangeNum <= 1)
        {
            EndTuneMinigame();
        }
        //for now this is for debug
        else if (rangeNum >= 1 && rangeNum <= maxNum / 10)
        {
            DistanceDebug.text = "Almost there";
        }
        else if (rangeNum >= maxNum / 10 && rangeNum <= maxNum / 3)
        {
            DistanceDebug.text = "Close";
        }
        else if (rangeNum >= maxNum / 3)
        {
            DistanceDebug.text = "Far";
        }
    }

    #endregion

    public void EndTuneMinigame()
    {
        Debug.Log("Complete");
        PlayerInputRef.Input.asset.FindActionMap("UI").Disable();
        PlayerInputRef.Input.asset.FindActionMap("Player").Enable();
        gameObject.SetActive(false);
    }

    public void Update()
    {
        Tuning();

        TuningVerification();
    }
}
