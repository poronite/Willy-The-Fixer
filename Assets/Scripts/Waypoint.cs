using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Waypoint : MonoBehaviour
{
    [SerializeField]
    private GameObject Player = null;

    [SerializeField] //testing purposes
    private GameObject repairSuggestion = null;

    private void Start()
    {
        AssignSuggestion();
    }

    public void AssignSuggestion() //this will trigger when the player repairs something or changes scene
    {
        List<GameObject> zonePianoParts = new List<GameObject>();
        List<GameObject> potentialSuggestions = new List<GameObject>();
        bool everythingRepaired = true;

        //find the parts of the piano to repair depending on scene
        switch (SceneManager.GetActiveScene().name)
        {
            case "UpperZonePiano":
                zonePianoParts.AddRange(GameObject.FindGameObjectsWithTag("Pin"));
                break;
            case "LowerZonePiano":
                zonePianoParts.AddRange(GameObject.FindGameObjectsWithTag("Key"));
                break;
            default:
                break;
        }

        //group everything that is destroyed
        foreach (GameObject component in zonePianoParts)
        {
            if (!component.GetComponent<PianoComponent>().IsRepaired)
            {
                potentialSuggestions.Add(component);
                everythingRepaired = false;
            }
        }

        //if there's at least something destroyed, suggest something to repair
        if (!everythingRepaired)
        {
            float lowestDistance = Mathf.Infinity;

            foreach (GameObject suggestion in potentialSuggestions)
            {
                float distanceToObject = (suggestion.transform.position - Player.transform.position).sqrMagnitude;

                if (distanceToObject < lowestDistance)
                {
                    lowestDistance = distanceToObject;
                    repairSuggestion = suggestion;
                }
            }
        }
        else
        {
            //suggest to go to the other scene
        }
    }

    void Update()
    {
        //the one line of code that gets the work done
        transform.LookAt(new Vector3(repairSuggestion.transform.position.x, transform.position.y, repairSuggestion.transform.position.z));
    }
}
