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
    public Footsteps YamaFootsteps;

    [SerializeField]
    private float relaxedSpeed = 0, runAwaySpeed = 0, hidingTime = 0;

    [SerializeField]
    private NavMeshAgent agent = null;

    [SerializeField]
    private Animator YamaAnimator = null;

    private bool hiding;
    public bool ReachedTarget;

    private Transform AIRenderer;
    private CapsuleCollider AICollider;
    private GameUI EnemyCountUI;

    private List<GameObject> hideHoles = new List<GameObject>();

    private void Awake()
    {
        EnemyCountUI = GameObject.FindGameObjectWithTag("EnemyCount").transform.GetChild(0).GetComponent<GameUI>();
        hideHoles.AddRange(GameObject.FindGameObjectsWithTag("HideHole"));
        AIRenderer = gameObject.transform.GetChild(0);
        AICollider = gameObject.GetComponent<CapsuleCollider>();
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

        //define maximum number of parts of the piano that the AI can break depending on the scene
        int limitOfBrokenParts = 0;

        switch (SceneManager.GetActiveScene().name)
        {
            case "UpperZonePiano":
                limitOfBrokenParts = 30;
                break;
            case "LowerZonePiano":
                limitOfBrokenParts = 15;
                break;
            default:
                break;
        }

        //if there are targets everythingDestroyed is false so AI gets a target
        //the AI moves to another scene if 30 pins or keys are destroyed
        if (!everythingDestroyed && potentialTargets.Count > potentialTargets.Count - limitOfBrokenParts)
        {
            Target = potentialTargets[Random.Range(0, potentialTargets.Count)];

            agent.SetDestination(Target.GetComponent<PianoComponent>().ComponentRealPosition);
            YamaAnimator.Play("GoToTarget", 0);
        }
        else
        {
            //move to another scene
            List<GameObject> exits = new List<GameObject>();
            exits.AddRange(GameObject.FindGameObjectsWithTag("Exit"));
            Target = exits[Random.Range(0, exits.Count)];
            agent.SetDestination(Target.transform.position);
            YamaAnimator.Play("GoToTarget", 0);
        }

        Task.current.Fail();
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
        agent.isStopped = true;
        transform.LookAt(Target.GetComponent<PianoComponent>().ComponentRealPosition);

        //play yama destroy animation
        YamaAnimator.Play("DestroyTarget", 0);

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
            target.gameObject.GetComponent<PianoComponent>().KeyAnimator.Play("Destroy", 0);
        }
        else
        {
            target.DestroyComponent();
        }

        //stop yama destroy animation
        YamaAnimator.Play("LookAware", 0);

        Task.current.Succeed();
    }

    [Task]
    void HideFromPlayer()
    {
        agent.isStopped = false;
        agent.speed = runAwaySpeed;

        YamaAnimator.Play("RunAway", 0);

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

        Task.current.Succeed();
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
            StartCoroutine(TeleportAI());
        }

        if (other.CompareTag("Exit"))
        {
            switch (SceneManager.GetActiveScene().name)
            {
                case "UpperZonePiano":
                    Manager.ManagerInstance.NumUpperZoneYamas--;
                    Manager.ManagerInstance.NumLowerZoneYamas++;
                    EnemyCountUI.UpdateEnemyCountUI(Manager.ManagerInstance.NumUpperZoneYamas);
                    break;
                case "LowerZonePiano":
                    Manager.ManagerInstance.NumLowerZoneYamas--;
                    Manager.ManagerInstance.NumUpperZoneYamas++;
                    EnemyCountUI.UpdateEnemyCountUI(Manager.ManagerInstance.NumLowerZoneYamas);
                    break;
                default:
                    break;
            }

            Destroy(gameObject);
        }
    }

    
    private void OnTriggerStay(Collider other)
    {
        //AI got stuck sometimes when this was inside OnTriggerEnter,
        //probably because they assigned the target while already inside the trigger collider
        if (!ReachedTarget && other.gameObject == Target)
        {
            ReachedTarget = true;
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        Debug.Log(collision.gameObject.tag);
        YamaFootsteps.ChangeSurfaceType(collision.gameObject.tag);
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
        Detection.AwareOfPlayer = false;

        yield return null;
    }
}
