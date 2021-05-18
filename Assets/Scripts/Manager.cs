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
    #endregion

    #region Awake&Start
    private void Awake()
    {
        ManagerInstance = this;
        DontDestroyOnLoad(gameObject);
        DontDestroyOnLoad(loadingScreen);
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
    #endregion


    public void ChangeCameraOffset(float height)
    {
        CinemachineVirtualCamera virCamera = GameObject.FindGameObjectWithTag("VirtualCamera").GetComponent<CinemachineVirtualCamera>();
        virCamera.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset.y = height;
    }
}
