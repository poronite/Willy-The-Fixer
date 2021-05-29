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

    private bool runningAwaySucceded, hiding;
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
    bool isAware()
    {
        return Detection.AwareOfPlayer;
    }

    [Task]
    void ForgetPlayer()
    {
        Detection.AwareOfPlayer = false;
    }

    [Task]
    void isHiding()
    {
        if (hiding == true)
        {
            Task.current.Succeed();
        }
        else if(hiding == false)
        {
            Task.current.Fail();
        }
    }

    [Task]
    void GoToTarget() //go destroy stuff
    {
        agent.speed = relaxedSpeed;
        agent.isStopped = false;

        GameObject target = GameObject.FindGameObjectWithTag("TestAI");

        agent.SetDestination(new Vector3(target.transform.position.x, target.transform.position.y, target.transform.position.z));

        Task.current.Succeed();
    }

    [Task]
    void HideFromPlayer()
    {
        runningAwaySucceded = false;
        agent.speed = runAwaySpeed;

        float distance = Mathf.Infinity;
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

        Debug.Log($"Hole: {currentHole}");
        agent.SetDestination(hideHoles[currentHole].transform.position);

        StartCoroutine(RunningAway());

        Task.current.Succeed();
    }

    private IEnumerator RunningAway()
    {
        while (runningAwaySucceded == false)
        {
            Debug.Log("Running Away...");
            yield return null;
        }
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
            runningAwaySucceded = true;
            StartCoroutine(teleportAI());
        }
    }

    private IEnumerator teleportAI()
    {
        hiding = true;
        int choice = Random.Range(1, hideHoles.Count);
        Transform spawnPoint = hideHoles[choice].gameObject.transform.GetChild(0).GetComponentInChildren<Transform>();

        AIRenderer.gameObject.SetActive(false);
        AICollider.enabled = false;

        agent.Warp(new Vector3(spawnPoint.position.x, spawnPoint.position.y, spawnPoint.position.z));

        yield return new WaitForSeconds(hidingTime);

        AIRenderer.gameObject.SetActive(true);
        AICollider.enabled = true;
        hiding = false;

        yield return null;
    }
}
