using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using FMODUnity;

public class PauseMenu : MonoBehaviour
{
    private string selectedButtonName;

    private FMOD.Studio.EventInstance instance;
    private FMOD.Studio.PLAYBACK_STATE clapsPlaybackState;

    public GameObject ResumeButton, ExitButton, AmbDrone;

    public void PauseGame()
    {
        selectedButtonName = "ResumeGameButton";
        EventSystem.current.SetSelectedGameObject(ResumeButton);

        Time.timeScale = 0.0f;

        PianoMusic.Music.Director.Pause();
        AmbDrone.GetComponent<StudioEventEmitter>().EventInstance.setPaused(true);

        PianoMusic.Music.ClapsInstance.getPlaybackState(out clapsPlaybackState);
        if (clapsPlaybackState != FMOD.Studio.PLAYBACK_STATE.PLAYING)
        {
            PianoMusic.Music.ClapsInstance.setPaused(false);
        }
        
    }

    public void ResumeGame()
    {
        Time.timeScale = 1.0f;

        //resume sound except sound dependent on animations
        PianoMusic.Music.Director.Resume();
        AmbDrone.GetComponent<StudioEventEmitter>().EventInstance.setPaused(false);

        PianoMusic.Music.ClapsInstance.getPlaybackState(out clapsPlaybackState);
        if (clapsPlaybackState != FMOD.Studio.PLAYBACK_STATE.PLAYING)
        {
            PianoMusic.Music.ClapsInstance.setPaused(true);
        }

        gameObject.SetActive(false);
    }

    public void ExitToMainMenu()
    {
        PianoMusic.Music.ClapsInstance.release();
        Manager.ManagerInstance.ChangeScene("MainMenu");
        gameObject.SetActive(false);
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
