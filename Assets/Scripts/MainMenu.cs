using UnityEngine;
using UnityEngine.UI;
using FMODUnity;

public class MainMenu : MonoBehaviour
{
    [SerializeField]
    private CanvasGroup InputBlocker = null;

    private string selectedButtonName;

    private FMOD.Studio.EventInstance instance;

    [SerializeField]
    private Sprite playButtonSprite = null, 
    playButtonGlowSprite = null, 
    exitButtonSprite = null, 
    exitButtonGlowSprite = null;

    [SerializeField]
    private Image playButtonImage = null, 
    exitButtonImage = null;

    private void Start()
    {
        selectedButtonName = "PlayButton";
    }

    public void Play() 
    {
        InputBlocker.interactable = false;
        Manager.ManagerInstance.ChangeScene("UpperZonePiano");
    }

    public void ExitGame()
    {
        InputBlocker.interactable = false;
        Application.Quit();
    }

    public void OnHover(Button button)
    {
        string buttonName = button.gameObject.name;

        if (buttonName != selectedButtonName && InputBlocker.interactable == true)
        {
            OnHoverSound();

            selectedButtonName = buttonName;

            switch (selectedButtonName)
            {
                case "PlayButton":
                    playButtonImage.sprite = playButtonGlowSprite;
                    exitButtonImage.sprite = exitButtonSprite;
                    break;
                case "ExitButton":
                    exitButtonImage.sprite = exitButtonGlowSprite;
                    playButtonImage.sprite = playButtonSprite;
                    break;
                default:
                    break;
            }
        }
    }

    public void OnHoverSound()
    {
        instance = RuntimeManager.CreateInstance("event:/UI/ButtonHover");
        instance.start();
        instance.release();
    }

    public void OnGameStartSound()
    {
        instance = RuntimeManager.CreateInstance("event:/UI/NewGame");
        instance.start();
        instance.release();
    }
}
