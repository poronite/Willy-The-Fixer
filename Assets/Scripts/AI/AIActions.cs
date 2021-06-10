using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using Panda;

public class AIActions : MonoBehaviour
{
    public PlayerDetection Detection;
    public GameObject Target;

    [SerializeField]
    private float relaxedSpeed = 0, runAwaySpeed = 0, hidingTime = 0;

    [SerializeField]
    private NavMeshAgent agent = null;

    private bool runningAwaySucceded, hiding;
    public bool ReachedTarget;

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
    void HasSeenPlayer()
    {
        if (Detection.SeeingPlayer)
        {
            Task.current.Succeed();
        }
        else
        {
            Task.current.Fail();
        }
    }

    [Task]
    bool IsSeeing()
    {
        return Detection.SeeingPlayer;
    }

    [Task]
    bool IsAware()
    {
        return Detection.AwareOfPlayer;
    }

    [Task]
    void ForgetPlayer()
    {
        Detection.AwareOfPlayer = false;
    }

    [Task]
    void IsHiding()
    {
        if (hiding)
        {
            Task.current.Succeed();
        }
        else if(!hiding)
        {
            Task.current.Fail();
        }
    }

    [Task]
    void GoToTarget() //go to target
    {
        agent.speed = relaxedSpeed;
        agent.isStopped = false;
        ReachedTarget = false;

        bool everythingDestroyed = true;
        string pianoZone = SceneManager.GetActiveScene().name;
        List<GameObject> targets = new List<GameObject>();
        List<GameObject> potentialTargets = new List<GameObject>();

        //choose list of targets based on scene
        switch (pianoZone)
        {
            case "UpperZonePiano":
                targets.AddRange(Manager.ManagerInstance.Pins);
                break;
            case "LowerZonePiano":
                targets.AddRange(Manager.ManagerInstance.Keys);
                break;
            default:
                break;
        }

        //get available targets
        for (int i = 0; i < targets.Count; i++)
        {
            if (targets[i].GetComponent<PianoComponent>().IsRepaired)
            {
                potentialTargets.Add(targets[i]);
                everythingDestroyed = false;
            }
        }

        //if there are targets everythingDestroyed is false so AI gets a target
        if (!everythingDestroyed)
        {
            Target = potentialTargets[Random.Range(0, potentialTargets.Count)];

            //in order to guarantee that ReachedTarget becomes true when the AI gets close to the target,
            //the AI has to move closer to the collider,
            //which collider it doesn't matter both have similar X,
            //just need the collider's center's X to add to the target's position
            //because some keys have pivots far to the object and the collider
            float targetCollider = Target.GetComponent<BoxCollider>().center.x; 

            Vector3 targetRealPosition = new Vector3(Target.transform.position.x + targetCollider, Target.transform.position.y, Target.transform.position.z);

            agent.SetDestination(targetRealPosition);
            Task.current.Fail();
        }
        else
        {
            //depending on whether they change zone after everything is destroyed change this
            Task.current.Succeed();
        }
    }

    [Task]
    void HasReachedTarget()
    {
        //if not reached target succeed so that the repeat continues
        if (!ReachedTarget)
        {
            Task.current.Succeed();
        }
        else //else fail in order to break out of the repeat
        {
            Task.current.Fail();
        }
    }

    [Task]
    void ReadyDestroy()
    {
        //play yama destroy animation

        Task.current.Fail();
    }


    [Task]
    void DestroyTarget()
    {
        //get status of target
        PianoComponent target = Target.GetComponent<PianoComponent>();

        //in case target gets destroyed by another yama before being destroyed
        if (!target.IsRepaired)
        {
            Debug.Log("Target already destroyed.");
            Task.current.Fail();
        }

        //destroy component
        if (target.CompareTag("Key"))
        {
            target.gameObject.GetComponent<RepairDestroy>().KeyAnimator.Play("Destroy", 0);
        }
        else
        {
            target.DestroyComponent();
        }

        //stop yama destroy animation

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
        while (!runningAwaySucceded)
        {
            Debug.Log("Running Away...");
            yield return null;
        }
    }

    [Task]
    void LookBehind()
    {
        StartCoroutine(RotateView(1f, 2f)); 

        Task.current.Succeed();
    }

    //this is when the Yama is looking behind to see if Willy is after him
    private IEnumerator RotateView(float duration, float speed) 
    {
        Quaternion startRotation = Quaternion.Euler(transform.forward);
        Vector3 endRotation = -transform.forward;
        float t = 0.0f;

        //perform rotation
        while (t <= duration)
        {
            transform.rotation = Quaternion.Slerp(startRotation, Quaternion.LookRotation(endRotation), t/duration);
            t += Time.deltaTime * speed;
            yield return null;
        }

        //to guarantee that the yama rotates completely
        transform.rotation = Quaternion.Slerp(startRotation, Quaternion.LookRotation(endRotation), 1); 
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
            StartCoroutine(TeleportAI());
        }

        if (other.gameObject == Target)
        {
            ReachedTarget = true;
        }
    }

    private IEnumerator TeleportAI()
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
