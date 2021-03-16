using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Manager : MonoBehaviour
{
    public static Manager ManagerInstance = null;

    private void Awake()
    {
        ManagerInstance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        SceneManager.LoadSceneAsync(1);
    }
}
