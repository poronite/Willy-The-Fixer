using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

//Verifies if the game started on the preload scene, and goes to that scene if not.

public class VerifyPreload : MonoBehaviour
{
    private void Awake()
    {
        if (Manager.ManagerInstance == null)
        {
            SceneManager.LoadSceneAsync("Preload");
        }
    }
}
