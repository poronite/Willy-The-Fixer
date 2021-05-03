using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Dot product for AI cone of vision: http://blog.wolfire.com/2009/07/linear-algebra-for-game-developers-part-2/

public class PlayerDetection : MonoBehaviour
{
    public GameObject Player;

    [SerializeField]
    private float maxSightDistance = 0, coneOfVision = 0;

    public bool AwarePlayer, SeeingPlayer;

    private void Awake()
    {
        Player = GameObject.FindGameObjectWithTag("Player").gameObject;
    }

    private void Update()
    {
        RaycastHit hit;

        Vector3 aiToWilly = (Player.transform.position - transform.position).normalized;

        if (Physics.Raycast(transform.position, aiToWilly, out hit, maxSightDistance))
        {
            Debug.DrawRay(transform.position, aiToWilly, Color.red, maxSightDistance);

            if (hit.collider != null && hit.collider.CompareTag("Player"))
            {
                Vector3 aiFront = transform.forward;

                float angle = Vector3.Angle(aiFront, aiToWilly);                                

                //is AI seeing player?
                if (angle <= coneOfVision / 2)
                {
                    SeeingPlayer = true;
                }

                AwarePlayer = true;
            }
            else if (hit.collider == null || !hit.collider.CompareTag("Player"))
            {
                SeeingPlayer = false;
            }
        }
    }
}
