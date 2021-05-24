using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerDetection : MonoBehaviour
{
    private GameObject player;

    [SerializeField]
    private float maxSightDistance = 0, coneOfVision = 0;

    public bool SeeingPlayer;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").gameObject;
    }

    private void Update()
    {
        //send a raycast in player direction and check if player is in AI field of view
        RaycastHit hit;

        Vector3 aiToWilly = (player.transform.position - transform.position).normalized;

        if (Physics.Raycast(transform.position, aiToWilly, out hit, maxSightDistance))
        {
            if (hit.collider != null && hit.collider.CompareTag("Player"))
            {
                Vector3 aiFront = transform.forward;

                float angle = Vector3.Angle(aiFront, aiToWilly);                                

                //is AI seeing player?
                if (angle <= coneOfVision / 2)
                {
                    SeeingPlayer = true;
                }
            }
            else if (hit.collider == null || !hit.collider.CompareTag("Player"))
            {
                SeeingPlayer = false;
            }
        }
    }
}
