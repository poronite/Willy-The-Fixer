using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MainMenu : MonoBehaviour
{
    private string selectedButtonName;

    private FMOD.Studio.EventInstance instance;

    private void Start()
    {
        selectedButtonName = "PlayButton";
    }

    public void Play() 
    {
        Manager.ManagerInstance.ChangeScene("UpperZonePiano");
    }

    public void ExitGame()
    {
        Application.Quit();
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
        instance = FMODUnity.RuntimeManager.CreateInstance("event:/UI/ButtonHover");
        instance.start();
        instance.release();
    }

    public void OnGameStartSound()
    {
        instance = FMODUnity.RuntimeManager.CreateInstance("event:/UI/NewGame");
        instance.start();
        instance.release();
    }
}
