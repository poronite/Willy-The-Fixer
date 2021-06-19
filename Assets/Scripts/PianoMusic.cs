using UnityEngine;
using UnityEngine.SceneManagement;
using FMODUnity;
using UnityEngine.Playables;

public class PianoMusic : MonoBehaviour
{
    public static PianoMusic Music = null;

    public PlayableDirector Director;
    public GameObject ClapsOrigin;

    [HideInInspector]
    public GameObject KeysParent;

    public FMOD.Studio.EventInstance ClapsInstance;

    private void Awake()
    {
        if (Music == null)
        {
            Music = this;
            DontDestroyOnLoad(gameObject);
            ClapsInstance = RuntimeManager.CreateInstance("event:/Amb/Crowd");
            ClapsInstance.set3DAttributes(RuntimeUtils.To3DAttributes(ClapsOrigin));
        }
        else 
        {
            Destroy(gameObject);
        }
    }

    public void TriggerKeyAnimation(string keyName)
    {
        if (SceneManager.GetActiveScene().name == "LowerZonePiano")
        {
            GameObject key = KeysParent.transform.Find(keyName).gameObject;

            if (key.GetComponent<PianoComponent>().IsRepaired)
            {
                key.GetComponent<Animator>().Play("playKey", 0);
                //Debug.Log($"Key {key.name} played.");
            }
        }
        else if(SceneManager.GetActiveScene().name == "UpperZonePiano")
        {
            //Debug.Log($"Key {keyName} played.");
        }
    }

    public void ClapsTrigger()
    {
        //Debug.Log("Audience Clapped");
        ClapsInstance.start();
    }
}
