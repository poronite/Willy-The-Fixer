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

    public GameObject YamaPrefab;

    //the RepairedPins/Keys arrays are used to store the status of the Pins/Keys when going to another scene
    private bool hasEnteredUpperZone = false;
    public bool[] RepairedPins = new bool[233];
    public GameObject[] Pins = new GameObject[233];
    public int NumUpperZoneYamas;

    private bool hasEnteredLowerZone = false;
    public bool[] RepairedKeys = new bool[88]; 
    public GameObject[] Keys = new GameObject[88];
    public int NumLowerZoneYamas;

    private GameObject[] spawnPoints = new GameObject[2];

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
        ChangeScene("MainMenu");
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
                NumUpperZoneYamas = 2;
                hasEnteredLowerZone = false;
                NumLowerZoneYamas = 2;
                break;
            case "UpperZonePiano":
                //pins
                GameObject.FindGameObjectsWithTag("Pin").CopyTo(Pins, 0);
                SetupLevel(Pins, RepairedPins, hasEnteredUpperZone, NumUpperZoneYamas);

                hasEnteredUpperZone = true;
                break;
            case "LowerZonePiano":
                //keys
                GameObject.FindGameObjectsWithTag("Key").CopyTo(Keys, 0);
                SetupLevel(Keys, RepairedKeys, hasEnteredLowerZone, NumLowerZoneYamas);

                hasEnteredLowerZone = true;
                break;
            default:
                break;
        }
    }

    private void SetupLevel(GameObject[] components, bool[] repairedComponents, bool firstTimeInZone, int numYamas)
    {
        if (!firstTimeInZone) //when entering scene for the first time
        {
            int numBrokenParts = 0;

            for (int i = 0; i < components.Length; i++)
            {
                PianoComponent componentStats = components[i].GetComponent<PianoComponent>();
                componentStats.index = i;

                if (numBrokenParts == 15)
                {
                    componentStats.IsRepaired = Random.value > 0.5;
                }
                else
                {
                    componentStats.IsRepaired = true;
                }
                
                if (componentStats.IsRepaired)
                {
                    repairedComponents[i] = true;

                    if (components[i].CompareTag("Key"))
                    {
                        components[i].GetComponent<RepairDestroy>().SetRepair();
                    } 
                }
                else
                {
                    numBrokenParts++;

                    repairedComponents[i] = false;

                    if (components[i].CompareTag("Key"))
                    {
                        components[i].GetComponent<RepairDestroy>().SetDestroy();
                    }
                }
            }
        }
        else //when entering the scene normally
        {
            for (int i = 0; i < components.Length; i++)
            {
                components[i].GetComponent<PianoComponent>().IsRepaired = repairedComponents[i];
            }
        }

        //get spawnPoints
        GameObject.FindGameObjectsWithTag("SpawnPoint").CopyTo(spawnPoints, 0);

        //spawn the enemies
        if (numYamas > 0)
        {
            for (int i = 0; i < numYamas; i++)
            {
                Instantiate(YamaPrefab, spawnPoints[i].transform);
            }
        }
    }
    #endregion

    #region Camera
    public void ChangeCameraX(float x)
    {
        CinemachineVirtualCamera virCamera = GameObject.FindGameObjectWithTag("VirtualCamera").GetComponent<CinemachineVirtualCamera>();
        virCamera.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset.x = x;
    }

    public void ChangeCameraY(float y)
    {
        CinemachineVirtualCamera virCamera = GameObject.FindGameObjectWithTag("VirtualCamera").GetComponent<CinemachineVirtualCamera>();
        virCamera.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset.y = y;
    }

    public void ChangeCameraZ(float z)
    {
        CinemachineVirtualCamera virCamera = GameObject.FindGameObjectWithTag("VirtualCamera").GetComponent<CinemachineVirtualCamera>();
        virCamera.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset.z = z;
    }

    public void ChangeCameraTarget(GameObject target)
    {
        CinemachineVirtualCamera virCamera = GameObject.FindGameObjectWithTag("VirtualCamera").GetComponent<CinemachineVirtualCamera>();
        virCamera.Follow = target.transform;
        virCamera.LookAt = target.transform;
    }

    #endregion
}
