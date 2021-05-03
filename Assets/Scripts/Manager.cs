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
    public static Manager ManagerInstance = null;
    public GameObject LoadingScreen;
    public GameObject LoadingText;
    public CanvasGroup LoadingScreenCanvas;

    public float FadeIn;
    public float FadeOut;
    #endregion

    #region Awake&Start
    private void Awake()
    {
        ManagerInstance = this;
        DontDestroyOnLoad(gameObject);
        DontDestroyOnLoad(LoadingScreen);
    }

    private void Start()
    {
        ChangeScene("UpperZonePiano");
    }
    #endregion

    #region SceneManagement

    public void ChangeScene(string targetScene)
    {
        StartCoroutine(changeSceneCoroutine(targetScene));
    }

    IEnumerator changeSceneCoroutine(string targetScene)
    {
        LoadingScreen.SetActive(true);

        if (SceneManager.GetActiveScene().name == "Preload")
        {
            LoadingScreenCanvas.alpha = 1;
        }
        else
        {
            yield return StartCoroutine(FadeLoadingScreen(1, FadeIn));
        }

        LoadingText.SetActive(true);

        AsyncOperation operation = SceneManager.LoadSceneAsync(targetScene);
        while (!operation.isDone)
        {
            yield return null;
        }

        LoadingText.SetActive(false);

        yield return StartCoroutine(FadeLoadingScreen(0, FadeOut));

        LoadingScreen.SetActive(false);
    }

    IEnumerator FadeLoadingScreen(float targetValue, float duration)
    {
        float startValue = LoadingScreenCanvas.alpha;
        float time = 0;

        while (time < duration)
        {
            LoadingScreenCanvas.alpha = Mathf.Lerp(startValue, targetValue, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        LoadingScreenCanvas.alpha = targetValue;
    }
    #endregion


    public void ChangeCameraOffset(float height)
    {
        CinemachineVirtualCamera virCamera = GameObject.FindGameObjectWithTag("VirtualCamera").GetComponent<CinemachineVirtualCamera>();
        //CinemachineVirtualCamera virCamera = FindObjectOfType<CinemachineVirtualCamera>(); Previous line of code
        virCamera.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset.y = height;
    }
}
