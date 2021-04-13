﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

//Source: https://gamedevbeginner.com/how-to-load-a-new-scene-in-unity-with-a-loading-screen/#persistent_loading_object

public class Manager : MonoBehaviour
{
    #region Variables
    public static Manager ManagerInstance = null;
    public GameObject LoadingScreen;
    public GameObject LoadingText;
    public CanvasGroup LoadingScreenCanvas;

    private string targetScene;
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
        targetScene = "Game";
        StartCoroutine(ChangeScene());
    }
    #endregion

    #region SceneManagement
    IEnumerator ChangeScene()
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
}
