using UnityEngine;


public class PlayerDetection : MonoBehaviour
{
    private GameObject player;

    [SerializeField]
    private float maxSightDistance = 0, coneOfVision = 0;

    public bool SeeingPlayer, AwareOfPlayer;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").gameObject;
    }

    private void FixedUpdate()
    {
        //send a raycast in player direction and check if player is in AI field of view
        RaycastHit hit;

        Vector3 aiTruePosition = new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z);
        Vector3 aiToWilly = (new Vector3(player.transform.position.x, player.transform.position.y + 0.5f, player.transform.position.z) - aiTruePosition).normalized;

        Debug.DrawRay(aiTruePosition, aiToWilly + new Vector3(0, 0, maxSightDistance), Color.red, Time.deltaTime);

        if (Physics.Raycast(aiTruePosition, aiToWilly, out hit, maxSightDistance))
        {
            if (hit.collider != null && hit.collider.CompareTag("Player"))
            {
                Vector3 aiFront = transform.forward;

                float angle = Vector3.Angle(aiToWilly, aiFront);

                Debug.Log(angle);

                //is AI seeing player?
                if (angle <= coneOfVision / 2)
                {
                    SeeingPlayer = true;
                    AwareOfPlayer = true;
                }
            }
            else if (hit.collider == null || !hit.collider.CompareTag("Player"))
            {
                SeeingPlayer = false;
            }
        }
    }
}
