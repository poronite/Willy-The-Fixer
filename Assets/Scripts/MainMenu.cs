using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public void Play() 
    {
        Manager.ManagerInstance.ChangeScene("UpperZonePiano");
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
