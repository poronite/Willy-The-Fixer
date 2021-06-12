using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PauseMenu : MonoBehaviour
{
    public GameObject ResumeButton, ExitButton;

    public void PauseGame()
    {
        EventSystem.current.SetSelectedGameObject(ResumeButton);
        Time.timeScale = 0.0f;
    }

    public void ResumeGame()
    {
        Time.timeScale = 1.0f;
        gameObject.SetActive(false);
    }

    public void ExitToMainMenu()
    {
        ResumeGame();
        Manager.ManagerInstance.ChangeScene("MainMenu");
    }
}
