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
    private GameObject loadingScreen = null, loadingIcon = null, gameEnd = null;

    [SerializeField]
    private CanvasGroup canvasLoadingScreen = null;

    public static Manager ManagerInstance = null;

    public GameObject YamaPrefab;

    //the RepairedPins/Keys arrays are used to store the status of the Pins/Keys when going to another scene
    private bool hasEnteredUpperZone = false;
    public bool[] RepairedPins = new bool[233];
    public GameObject[] Pins = new GameObject[233];
    public int NumRepairedPins;
    public int NumUpperZoneYamas;

    private bool hasEnteredLowerZone = false;
    public bool[] RepairedKeys = new bool[88]; 
    public GameObject[] Keys = new GameObject[88];
    public int NumRepairedKeys;
    public int NumLowerZoneYamas;

    public bool MovementTutorialDone = false, 
    RollTutorialDone = false, 
    JumpTutorialDone = false, 
    DescendTutorialDone = false, 
    InteractTutorialDone = false;

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
            yield return StartCoroutine(FadeLoadingScreen(1, fadeInLoadingScreen, canvasLoadingScreen));
        }

        loadingIcon.SetActive(true);

        //start loading
        AsyncOperation operation = SceneManager.LoadSceneAsync(targetScene);
        while (!operation.isDone)
        {
            yield return null;
        }

        loadingIcon.SetActive(false);

        //end loading and start fading out
        yield return StartCoroutine(FadeLoadingScreen(0, fadeOutLoadingScreen, canvasLoadingScreen));

        loadingScreen.SetActive(false);
    }

    IEnumerator FadeLoadingScreen(float targetValue, float duration, CanvasGroup uiElement)
    {
        float startValue = uiElement.alpha;
        float time = 0;

        while (time < duration)
        {
            uiElement.alpha = Mathf.Lerp(startValue, targetValue, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        //this is here to guarantee that the alpha is 1 (or 0) instead of a very close float value
        uiElement.alpha = targetValue; 
    }

    private void OnSceneChange(Scene destinationScene, LoadSceneMode mode)
    {
        switch (destinationScene.name)
        {
            case "MainMenu":
                //reset just in case player starts a new game
                hasEnteredUpperZone = false;
                NumRepairedPins = 0;
                NumUpperZoneYamas = 2;
                hasEnteredLowerZone = false;
                NumRepairedKeys = 0;
                NumLowerZoneYamas = 2;
                MovementTutorialDone = false;
                RollTutorialDone = false;
                JumpTutorialDone = false;
                DescendTutorialDone = false;
                InteractTutorialDone = false;
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
        GameUI enemyCount = GameObject.FindGameObjectWithTag("EnemyCount").transform.GetChild(0).GetComponent<GameUI>();

        enemyCount.UpdateEnemyCountUI(numYamas);

        if (!firstTimeInZone) //when entering scene for the first time
        {
            int numBrokenParts = 0;

            int maxInitialBrokenParts = 0;

            //define maximum number of parts that can break at the start of the game
            switch (SceneManager.GetActiveScene().name)
            {
                case "UpperZonePiano":
                    maxInitialBrokenParts = 15;
                    break;
                case "LowerZonePiano":
                    maxInitialBrokenParts = 5;
                    break;
                default:
                    break;
            }

            for (int i = 0; i < components.Length; i++)
            {
                PianoComponent componentStats = components[i].GetComponent<PianoComponent>();
                componentStats.index = i;

                if (numBrokenParts <= maxInitialBrokenParts)
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
                        NumRepairedKeys++;
                        components[i].GetComponent<RepairDestroy>().SetRepair();
                    }
                    else
                    {
                        NumRepairedPins++;
                    }
                    
                    componentStats.ComponentMaterial.material = componentStats.RepairedMaterial;
                }
                else
                {
                    numBrokenParts++;

                    repairedComponents[i] = false;

                    if (components[i].CompareTag("Key"))
                    {
                        components[i].GetComponent<RepairDestroy>().SetDestroy();
                    }
                    
                    componentStats.ComponentMaterial.material = componentStats.DestroyedMaterial;
                }
            }
        }
        else //when entering the scene normally
        {
            for (int i = 0; i < components.Length; i++)
            {
                PianoComponent componentStats = components[i].GetComponent<PianoComponent>();
                componentStats.index = i;
                componentStats.IsRepaired = repairedComponents[i];

                if (componentStats.IsRepaired)
                {
                    if (components[i].CompareTag("Key"))
                    {
                        components[i].GetComponent<RepairDestroy>().SetRepair();
                    }
                    
                    componentStats.ComponentMaterial.material = componentStats.RepairedMaterial;
                }
                else
                {
                    if (components[i].CompareTag("Key"))
                    {
                        components[i].GetComponent<RepairDestroy>().SetDestroy();
                    }
                    
                    componentStats.ComponentMaterial.material = componentStats.DestroyedMaterial;
                }
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

    public IEnumerator GameClear()
    {
        loadingScreen.SetActive(true);

        gameEnd.SetActive(true);
        loadingIcon.SetActive(false);

        canvasLoadingScreen.alpha = 0;

        StartCoroutine(FadeLoadingScreen(1, 1.5f, canvasLoadingScreen));

        AsyncOperation operation = SceneManager.LoadSceneAsync("MainMenu");
        while (!operation.isDone)
        {
            yield return null;
        }

        StartCoroutine(FadeLoadingScreen(0, 1.5f, canvasLoadingScreen));

        gameEnd.SetActive(false);
        loadingIcon.SetActive(true);

        loadingScreen.SetActive(false);
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

    public void ChangeCameraTarget(Transform target)
    {
        CinemachineVirtualCamera virCamera = GameObject.FindGameObjectWithTag("VirtualCamera").GetComponent<CinemachineVirtualCamera>();
        virCamera.Follow = target;
        virCamera.LookAt = target;
    }

    #endregion
}
