using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using FMODUnity;

public class PauseMenu : MonoBehaviour
{
    private string selectedButtonName;

    private FMOD.Studio.EventInstance instance;

    public GameObject ResumeButton, ExitButton, AmbDrone;

    public void PauseGame()
    {
        selectedButtonName = "ResumeGameButton";
        EventSystem.current.SetSelectedGameObject(ResumeButton);
        Time.timeScale = 0.0f;
        PianoMusic.Music.Director.Pause();
        AmbDrone.GetComponent<StudioEventEmitter>().EventInstance.setPaused(true);
    }

    public void ResumeGame()
    {
        Time.timeScale = 1.0f;
        PianoMusic.Music.Director.Resume();
        AmbDrone.GetComponent<StudioEventEmitter>().EventInstance.setPaused(false);
        gameObject.SetActive(false);
    }

    public void ExitToMainMenu()
    {
        ResumeGame();
        Manager.ManagerInstance.ChangeScene("MainMenu");
    }

    public void OnHover(Button button)
    {
        string buttonName = button.gameObject.name;

        if (buttonName != selectedButtonName)
        {
            OnHoverSound();

            selectedButtonName = buttonName;

            Debug.Log(selectedButtonName);
        }
    }

    public void OnHoverSound()
    {
        instance = RuntimeManager.CreateInstance("event:/UI/ButtonHover");
        instance.start();
        instance.release();
    }
}
