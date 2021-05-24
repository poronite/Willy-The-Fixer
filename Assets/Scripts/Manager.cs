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
    public List<bool> RepairedPins;
    public List<bool> RepairedStrings;
    public List<GameObject> Pins;
    public List<GameObject> Strings;

    private bool hasEnteredLowerZone = false;
    public List<bool> RepairedKeys;
    public List<GameObject> Keys;

    #endregion

    #region Awake&Start
    private void Awake()
    {
        ManagerInstance = this;
        DontDestroyOnLoad(gameObject);
        DontDestroyOnLoad(loadingScreen);
        SceneManager.activeSceneChanged += OnSceneChange;
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

    private void OnSceneChange(Scene currentScene, Scene destinationScene)
    {
        switch (destinationScene.name)
        {
            case "MainMenu":
                hasEnteredUpperZone = false;
                hasEnteredLowerZone = false;
                RepairedPins.Clear();
                RepairedStrings.Clear();
                RepairedKeys.Clear();
                break;
            case "UpperZonePiano":
                Pins.AddRange(GameObject.FindGameObjectsWithTag("Pin"));
                //Strings.AddRange(GameObject.FindGameObjectsWithTag("Strings"));

                if (!hasEnteredUpperZone)
                {
                    hasEnteredUpperZone = true;

                    foreach (GameObject pin in Pins)
                    {
                        RepairedPins.Add(pin.GetComponent<PianoComponent>().IsRepaired = Random.value > 0.5);
                    }

                    //add strings
                }

                //add rest of logic

                break;
            case "LowerZonePiano":
                if (hasEnteredLowerZone == false)
                {
                    hasEnteredLowerZone = true;

                    foreach (GameObject key in Keys)
                    {
                        RepairedKeys.Add(key.GetComponent<PianoComponent>().IsRepaired = Random.value > 0.5);
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
