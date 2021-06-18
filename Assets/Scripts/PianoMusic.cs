using UnityEngine;
using UnityEngine.SceneManagement;
using FMODUnity;
using UnityEngine.Playables;

public class PianoMusic : MonoBehaviour
{
    public static PianoMusic Music = null;

    public PlayableDirector Director;
    public GameObject ClapsOrigin;

    private FMOD.Studio.EventInstance instance;

    private void Awake()
    {
        if (Music == null)
        {
            Music = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneChange;
        }
        else 
        {
            Destroy(gameObject);
        }
    }

    //destroy the PianoMusic game object when leaving game to the main menu so that the music doesn't play in the menu
    private void OnSceneChange(Scene destinationScene, LoadSceneMode mode)
    {
        switch (destinationScene.name)
        {
            case "MainMenu":
                Music = null;
                Destroy(gameObject);
                break;
            default:
                break;
        }
    }

    public void TriggerKeyAnimation(GameObject keyRepresentation)
    {
        string keyName = keyRepresentation.name;

        if (SceneManager.GetActiveScene().name == "LowerZonePiano")
        {
            GameObject key = GameObject.Find(keyName);

            if (key.GetComponent<PianoComponent>().IsRepaired)
            {
                key.GetComponent<Animator>().Play("playKey", 0);
                Debug.Log($"Key {keyName} played.");
            }
        }
        else if(SceneManager.GetActiveScene().name == "UpperZonePiano")
        {
            Debug.Log($"Key {keyName} played.");
        }
    }

    public void ClapsTrigger()
    {
        Debug.Log("Audience Clapped");
        instance = RuntimeManager.CreateInstance("event:/SFX/Characters/Footsteps");
        instance.set3DAttributes(RuntimeUtils.To3DAttributes(ClapsOrigin));
        instance.start();
        instance.release();
    }
}
