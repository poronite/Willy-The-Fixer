using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PianoZonesTransition : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            switch (SceneManager.GetActiveScene().name)
            {
                case "UpperZonePiano":
                    Manager.ManagerInstance.GetComponent<Manager>().ChangeScene("LowerZonePiano");
                    break;
                case "LowerZonePiano":
                    Manager.ManagerInstance.GetComponent<Manager>().ChangeScene("UpperZonePiano");
                    break;
                default:
                    break;
            }
        }
    }
}
