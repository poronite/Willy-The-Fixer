using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Cinemachine;

//Source: https://gamedevbeginner.com/how-to-load-a-new-scene-in-unity-with-a-loading-screen/#persistent_loading_object

public class Manager : MonoBehaviour
{
    #region Variables
    [SerializeField]
    private float fadeInLoadingScreen = 0.0f,
    fadeOutLoadingScreen = 0.0f;

    [SerializeField]
    private GameObject loadingScreen = null, loadingText = null;

    [SerializeField]
    private CanvasGroup canvasLoadingScreen = null;

    public static Manager ManagerInstance = null;

    private bool hasEnteredUpperZone = false;
    public int NumRepairedPins;
    public int NumRepairedStrings;
    public List<GameObject> Pins;
    public List<GameObject> Strings;

    private bool hasEnteredLowerZone = false;
    public int NumRepairedKeys;
    public List<GameObject> Keys;

    #endregion

    #region Awake&Start
    private void Awake()
    {
        ManagerInstance = this;
        DontDestroyOnLoad(gameObject);
        DontDestroyOnLoad(loadingScreen);
        SceneManager.sceneLoaded += OnSceneChange;
    }

    private void Start()
    {
        ChangeScene("LowerZonePiano");
    }
    #endregion

    #region SceneManagement

    public void ChangeScene(string targetScene)
    {
        StartCoroutine(changeSceneCoroutine(targetScene));
    }

    IEnumerator changeSceneCoroutine(string targetScene)
    {
        //activate LoadingScreen gameobject and start fading in
        loadingScreen.SetActive(true);

        if (SceneManager.GetActiveScene().name == "Preload") //this is here because Preload scene is a empty scene
        {
            canvasLoadingScreen.alpha = 1;
        }
        else
        {
            yield return StartCoroutine(FadeLoadingScreen(1, fadeInLoadingScreen));
        }

        loadingText.SetActive(true);

        //start loading
        AsyncOperation operation = SceneManager.LoadSceneAsync(targetScene);
        while (!operation.isDone)
        {
            yield return null;
        }

        loadingText.SetActive(false);

        //end loading and start fading out
        yield return StartCoroutine(FadeLoadingScreen(0, fadeOutLoadingScreen));

        loadingScreen.SetActive(false);
    }

    IEnumerator FadeLoadingScreen(float targetValue, float duration)
    {
        float startValue = canvasLoadingScreen.alpha;
        float time = 0;

        while (time < duration)
        {
            canvasLoadingScreen.alpha = Mathf.Lerp(startValue, targetValue, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        //this is here to guarantee that the alpha is 1 (or 0) instead of a very close float value
        canvasLoadingScreen.alpha = targetValue; 
    }

    private void OnSceneChange(Scene destinationScene, LoadSceneMode mode)
    {
        switch (destinationScene.name)
        {
            case "MainMenu":
                //reset just in case player starts a new game
                hasEnteredUpperZone = false;
                hasEnteredLowerZone = false;
                NumRepairedPins = 0;
                NumRepairedStrings = 0;
                NumRepairedKeys = 0;
                break;
            case "UpperZonePiano":
                Pins.AddRange(GameObject.FindGameObjectsWithTag("Pin"));
                //Strings.AddRange(GameObject.FindGameObjectsWithTag("Strings"));

                //when entering scene for the first time
                if (!hasEnteredUpperZone)
                {
                    hasEnteredUpperZone = true;

                    foreach (GameObject pin in Pins)
                    {
                        pin.GetComponent<PianoComponent>().IsRepaired = Random.value > 0.5;
                        NumRepairedPins++;
                    }

                    //add strings
                }

                //when entering a scene normally
                int numPinsRepaired = NumRepairedPins;

                foreach (GameObject pin in Pins)
                {
                    if (numPinsRepaired > 0)
                    {
                        pin.GetComponent<PianoComponent>().IsRepaired = true;
                        numPinsRepaired--;
                    }
                }

                //add rest of logic for the strings

                break;
            case "LowerZonePiano":
                if (hasEnteredLowerZone == false)
                {
                    hasEnteredLowerZone = true;

                    foreach (GameObject key in Keys)
                    {
                        key.GetComponent<PianoComponent>().IsRepaired = Random.value > 0.5;
                        NumRepairedKeys++;
                    }                    
                }

                int numKeysRepaired = NumRepairedKeys;

                foreach (GameObject key in Keys)
                {
                    if (numKeysRepaired > 0)
                    {
                        key.GetComponent<PianoComponent>().IsRepaired = true;
                        numKeysRepaired--;
                    }
                }

                //add rest of logic

                break;
            default:
                break;
        }
    }
    #endregion


    public void ChangeCameraOffset(float height)
    {
        CinemachineVirtualCamera virCamera = GameObject.FindGameObjectWithTag("VirtualCamera").GetComponent<CinemachineVirtualCamera>();
        virCamera.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset.y = height;
    }
}
