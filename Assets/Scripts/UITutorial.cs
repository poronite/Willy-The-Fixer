using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UITutorial : MonoBehaviour
{
    //this script is to activate the images that help the user
    //figuring out how to play the game little by little
    //they turn on and off at certain times

    //Player
    public Player PlayerInput;

    //Movement
    public GameObject WASD;
    public GameObject LeftStick;

    //Roll
    public GameObject RightMouse;
    public GameObject Circle;

    //Jump
    public GameObject Space;
    public GameObject X;

    //Descend
    public GameObject CTRL;
    public GameObject DPadDown;

    //Interact
    public GameObject E;
    public GameObject Square;

    private void Start()
    {
        if (Manager.ManagerInstance.MovementTutorialDone == false)
        {
            ActivateTutorial(WASD, LeftStick);
        }
    }

    public void ActivateTutorial(GameObject keyboardMouseKey, GameObject controllerKey)
    {
        ClearAllTutorial();

        if (PlayerInput.LastInputDevice == "Keyboard" || PlayerInput.LastInputDevice == "Mouse")
        {
            controllerKey.SetActive(false);
            keyboardMouseKey.SetActive(true);
        }
        else
        {
            keyboardMouseKey.SetActive(false);
            controllerKey.SetActive(true);
        }
    }

    public void DeactivateTutorial(GameObject keyboardKey, GameObject controllerKey) 
    {
        keyboardKey.SetActive(false);
        controllerKey.SetActive(false);
    }

    //this is just in case a tutorial is interromped by another
    public void ClearAllTutorial() 
    {
        WASD.SetActive(false);
        LeftStick.SetActive(false);
        RightMouse.SetActive(false);
        Circle.SetActive(false);
        Space.SetActive(false);
        X.SetActive(false);
        CTRL.SetActive(false);
        DPadDown.SetActive(false);
        E.SetActive(false);
        Square.SetActive(false);
    }
}
