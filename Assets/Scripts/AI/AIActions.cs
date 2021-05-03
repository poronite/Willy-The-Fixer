using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Panda;

public class AIActions : MonoBehaviour
{
    public PlayerDetection Detection;

    [SerializeField]
    private float relaxedSpeed = 0, runAwaySpeed = 0, hidingTime = 0;

    [SerializeField]
    private NavMeshAgent agent = null;

    private bool Hiding;
    private Transform AIRenderer;
    private CapsuleCollider AICollider;

    private List<GameObject> hideHoles = new List<GameObject>();

    private void Awake()
    {
        hideHoles.AddRange(GameObject.FindGameObjectsWithTag("HideHole"));
        AIRenderer = gameObject.transform.GetChild(0);
        AICollider = gameObject.GetComponent<CapsuleCollider>();
    }

    [Task]
    void hasSeenPlayer()
    {
        if (Detection.SeeingPlayer == true)
        {
            Task.current.Succeed();
        }
        else
        {
            Task.current.Fail();
        }
    }

    [Task]
    bool isSeeing()
    {
        return Detection.SeeingPlayer;
    }

    [Task]
    void isHiding()
    {
        if (Hiding == true)
        {
            Task.current.Succeed();
        }
        else if(Hiding == false)
        {
            Task.current.Fail();
        }
    }

    [Task]
    void GoToTarget()
    {
        agent.speed = relaxedSpeed;
        agent.isStopped = false;

        agent.SetDestination(new Vector3(0.0f, transform.position.y, 0.0f));

        Task.current.Succeed();
    }

    [Task]
    void HideFromPlayer()
    {
        agent.speed = runAwaySpeed;

        float distance = 9999999f;
        int currentHole = 0;

        for (int i = 0; i < hideHoles.Count; i++)
        {
            agent.SetDestination(hideHoles[i].transform.position);

            if (distance > agent.remainingDistance)
            {
                distance = agent.remainingDistance;
                currentHole = i;
            }
        }

        agent.SetDestination(hideHoles[currentHole].transform.position);

        Task.current.Succeed();
    }

    [Task]
    void LookBehind()
    {
        StartCoroutine(RotateView(0.35f));

        Task.current.Succeed();
    }

    private IEnumerator RotateView(float duration)
    {
        Quaternion startRotation = Quaternion.Euler(transform.forward);
        Vector3 endRotation = -transform.forward;
        float t = 0.0f;

        while (t < duration)
        {
            transform.rotation = Quaternion.Slerp(startRotation, Quaternion.LookRotation(endRotation), t/duration);
            yield return null;
            t += Time.deltaTime;
        }
    }

    [Task]
    void StopRunningAway()
    {
        agent.isStopped = true;

        Task.current.Succeed();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("HideHole"))
        {
            StartCoroutine(teleportAI());
        }
    }

    private IEnumerator teleportAI()
    {
        Hiding = true;
        int choice = Random.Range(1, hideHoles.Count);
        Transform spawnPoint = hideHoles[choice].gameObject.transform.GetChild(0).GetComponentInChildren<Transform>();

        AIRenderer.gameObject.SetActive(false);
        AICollider.enabled = false;

        agent.Warp(new Vector3(spawnPoint.position.x, spawnPoint.position.y, spawnPoint.position.z));

        yield return new WaitForSeconds(hidingTime);

        AIRenderer.gameObject.SetActive(true);
        AICollider.enabled = true;
        Hiding = false;

        yield return null;
    }
}
