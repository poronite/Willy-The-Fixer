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
    fadeOutLoadingScreen = 0.0f,
    fadeInEndGame = 0.0f,
    fadeOutEndGame = 0.0f;

    [SerializeField]
    private GameObject loadingScreen = null, background = null, loadingIcon = null, gameEnd = null;

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
    InteractTutorialDone = false,
    EndGame = false,
    WinGame = false;

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

        //starting a new game or going through upper and lower zone piano
        if (targetScene != "MainMenu") 
        {
            gameEnd.SetActive(false);
            background.SetActive(true);
            yield return StartCoroutine(FadeLoadingScreen(1, fadeInLoadingScreen, canvasLoadingScreen));
            loadingIcon.SetActive(true);
        }
        else
        {
            if (!WinGame) //if exit game to the main menu
            {
                gameEnd.SetActive(false);
                background.SetActive(true);
                yield return StartCoroutine(FadeLoadingScreen(1, fadeInLoadingScreen, canvasLoadingScreen));
                loadingIcon.SetActive(true);
            }
            else //if piano is fixed
            {
                gameEnd.SetActive(true);
                background.SetActive(false);
                yield return StartCoroutine(FadeLoadingScreen(1, fadeInEndGame, canvasLoadingScreen));
                loadingIcon.SetActive(false);
            }
        }

        //start loading
        AsyncOperation operation = SceneManager.LoadSceneAsync(targetScene);
        while (!operation.isDone)
        {
            yield return null;
        }

        //end loading and start fading out
        loadingIcon.SetActive(false);

        if (!WinGame) //normal fade out
        {
            yield return StartCoroutine(FadeLoadingScreen(0, fadeOutLoadingScreen, canvasLoadingScreen));
        }
        else //piano is fixed fade out
        {
            yield return StartCoroutine(FadeLoadingScreen(0, fadeOutEndGame, canvasLoadingScreen));
        }
        

        loadingScreen.SetActive(false);
    }

    IEnumerator FadeLoadingScreen(float targetValue, float duration, CanvasGroup uiElement)
    {
        float startValue = uiElement.alpha;
        float time = 0f;
        AudioSource music = null;

        if (EndGame) //music will be fade out when ending game, regardless of winning or ending through pause menu
        {
            music = GameObject.FindGameObjectWithTag("Music").GetComponent<AudioSource>();

            //if audience is clapping fade out (doesn't work, probably because the object gets destroyed mid way)
            FMOD.Studio.PLAYBACK_STATE clapsPlaybackState;

            PianoMusic.Music.ClapsInstance.getPlaybackState(out clapsPlaybackState);
            if (clapsPlaybackState == FMOD.Studio.PLAYBACK_STATE.PLAYING)
            {
                PianoMusic.Music.ClapsInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                PianoMusic.Music.ClapsInstance.release();
            }
        }

        while (time < duration)
        {
            if (EndGame)
            {
                music.volume = Mathf.Lerp(1, 0, time / duration);
            }
            uiElement.alpha = Mathf.Lerp(startValue, targetValue, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        //this is here to guarantee that the alpha (and the volume if ending game) is 1 (or 0) instead of a very close float value
        uiElement.alpha = targetValue;

        if (EndGame)
        {
            music.volume = 0;
        }
    }

    private void OnSceneChange(Scene destinationScene, LoadSceneMode mode)
    {
        switch (destinationScene.name)
        {
            case "MainMenu":
                //reset just in case player starts a new game
                if (PianoMusic.Music != null)
                {
                    Destroy(PianoMusic.Music.gameObject);
                }

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
                EndGame = false;
                WinGame = false;
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
                        components[i].GetComponent<PianoComponent>().SetRepair();
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
                        components[i].GetComponent<PianoComponent>().SetDestroy();
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
                        components[i].GetComponent<PianoComponent>().SetRepair();
                    }
                    
                    componentStats.ComponentMaterial.material = componentStats.RepairedMaterial;
                }
                else
                {
                    if (components[i].CompareTag("Key"))
                    {
                        components[i].GetComponent<PianoComponent>().SetDestroy();
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
    #endregion

    #region Victory
    public void IsGameWon() //has game ended? If not assign the next Waypoint Suggestion
    {
        if (NumRepairedPins == 233 && NumRepairedKeys == 88)
        {
            EndGame = true;
            WinGame = true;
            ChangeScene("MainMenu");
        }
        else
        {
            FindObjectOfType<Waypoint>().AssignSuggestion();
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

    public void ChangeCameraTarget(Transform target)
    {
        CinemachineVirtualCamera virCamera = GameObject.FindGameObjectWithTag("VirtualCamera").GetComponent<CinemachineVirtualCamera>();
        virCamera.Follow = target;
        virCamera.LookAt = target;
    }

    #endregion
}
