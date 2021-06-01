using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    #region Variables

    [SerializeField]
    private float acceleration = 0,
    maxSpeed = 0,
    jumpForce = 0,
    groundThreshhold = 5.0f,
    jumpGravityScale = 1.0f,
    fallGravityScale = 1.0f,
    rollForce = 0,
    rollDuration = 0;

    private bool onAir = false,
    isRolling = false,
    isOnStrings = false,
    hasClimbed = false;

    private Rigidbody playerRigidbody;
    private Vector2 move;
    private float rollTimer = 0;
    private List<GameObject> nearbyInteractables = new List<GameObject>();
    private GameObject nearestInteractable;

    public PlayerInput Input;
    public GameObject TuneMinigameUI;

    [HideInInspector]
    public string LastInputDevice;
    #endregion

    private void Awake()
    {
        playerRigidbody = gameObject.GetComponent<Rigidbody>();
        SetInputs();
    }

    #region Inputs
    private void SetInputs()
    {
        Input = new PlayerInput();

        Input.Player.Enable();
        Input.TuneMinigame.Disable();
        Input.RepairMinigame.Disable();

        //Movement
        Input.Player.Move.performed += context => move = context.ReadValue<Vector2>();
        Input.Player.Move.canceled += context => move = Vector2.zero;

        //Jump
        Input.Player.Jump.performed += context => Jump();

        //Roll
        Input.Player.Roll.performed += context => StartRoll();

        //Fall
        Input.Player.Descend.performed += context => Descend();

        //Interact
        Input.Player.Interact.performed += context =>
        {
            LastInputDevice = context.control.device.name;
            Interact();
        };
    }

    private void OnEnable()
    {
        Input.Player.Enable();
    }

    private void OnDisable()
    {
        Input.Player.Disable();        
    }

    #endregion

    #region Collisions

    private void OnCollisionEnter(Collision collision)
    {
        //When the player jumps to climb the cords
        if (collision.gameObject.CompareTag("Strings") && !hasClimbed)
        {
            hasClimbed = true; //this is to prevent the player from bugging

            //"Climb" the cords by changing y of the player
            Vector3 climbStrings = gameObject.transform.position;
            climbStrings.y = 15f;
            gameObject.transform.position = climbStrings;

            //Re-use the Vector3 just to cancel the speed of the jump
            climbStrings = playerRigidbody.velocity;
            climbStrings.y = 0.0f;
            playerRigidbody.velocity = climbStrings;

            Manager.ManagerInstance.ChangeCameraY(5f);
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        //When the player is on the ground 
        if (collision.gameObject.CompareTag("Ground"))
        {
            float angle = Vector3.Angle(Vector3.up, collision.GetContact(0).normal);

            if (angle < groundThreshhold)
            {
                onAir = false;
            }
        }

        //When the player is on the cords
        if (collision.gameObject.CompareTag("Strings"))
        {
            onAir = false;
            isOnStrings = true;
        }

        //catch Yama
        if (collision.gameObject.CompareTag("AI") && isRolling)
        {
            Destroy(collision.gameObject);
            switch (SceneManager.GetActiveScene().name)
            {
                case "UpperZonePiano":
                    Manager.ManagerInstance.NumUpperZoneYamas--;
                    break;
                case "LowerZonePiano":
                    Manager.ManagerInstance.NumLowerZoneYamas--;
                    break;
                default:
                    break;
            }            
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        //When the player jumps from the ground or cords
        if (collision.gameObject.CompareTag("Ground") || (collision.gameObject.CompareTag("Strings") && isOnStrings))
        {
            onAir = true;
            isOnStrings = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Pin") || other.gameObject.CompareTag("Key"))
        {
            Debug.Log(other.name);
            nearbyInteractables.Add(other.gameObject);
        }
        else if (other.gameObject.CompareTag("Exit"))
        {
            Input.Player.Disable();

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

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Pin") || other.gameObject.CompareTag("Key"))
        {
            Debug.Log(other.name);
            nearbyInteractables.Remove(other.gameObject);
        }
    }

    #endregion

    #region PlayerMechanics
    public void Movement()
    {
        if (!isRolling) //otherwise the dash would stop too soon
        {
            //Set direction and speed of movement
            Vector3 movement = new Vector3(-move.x, 0.0f, -move.y).normalized * acceleration;

            //rotate the player to the movement direction
            if (movement != Vector3.zero)
            {
                playerRigidbody.rotation = Quaternion.Slerp(playerRigidbody.rotation, Quaternion.LookRotation(movement), 0.3f);
            }

            //move the player
            playerRigidbody.AddForce(movement);

            //clamp the movement otherwise too intense
            Vector3 velocity = playerRigidbody.velocity;

            float verticalVelocity = velocity.y;

            velocity.y = 0;

            velocity = Vector3.ClampMagnitude(velocity, maxSpeed);

            velocity.y = verticalVelocity;

            playerRigidbody.velocity = velocity;
        }
    }

    public void Jump()
    {
        if (!onAir && !isRolling) //can't jump while rolling
        {
            playerRigidbody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    public void ApplyCustomGravity()
    {
        Vector3 gravity = Physics.gravity;

        if (playerRigidbody.velocity.y > 0)
        {
            gravity *= jumpGravityScale;
        }

        if (playerRigidbody.velocity.y < 0)
        {
            gravity *= fallGravityScale;
        }

        playerRigidbody.AddForce(gravity, ForceMode.Acceleration);
    }

    public void StartRoll()
    {
        if (!isRolling && !onAir) //can't roll while jumping
        {
            //roll movement
            playerRigidbody.AddForce(new Vector3(-move.x, 0.0f, -move.y) * rollForce, ForceMode.Impulse);

            //start roll timer
            rollTimer = 0;
            isRolling = true;
        }
    }

    public void EndDash()
    {
        if (isRolling)
        {
            //in the middle of dash
            rollTimer += Time.deltaTime;

            if (rollTimer >= rollDuration)
            {
                //end dash
                Vector3 cancelRoll = playerRigidbody.velocity;
                cancelRoll.x = 0.0f;
                cancelRoll.z = 0.0f;
                playerRigidbody.velocity = cancelRoll;
                isRolling = false;
            }
        }
    }

    public void Descend()
    {
        if (isOnStrings)
        {
            //Same logic as climbing
            Vector3 descendCords = gameObject.transform.position;
            descendCords.y = 13.5f;
            gameObject.transform.position = descendCords;

            Manager.ManagerInstance.ChangeCameraY(1f); //Change perspetive

            onAir = true;
            isOnStrings = false;
            hasClimbed = false;
        }
    }

    public void Interact()
    {
        float nearestDistance = Mathf.Infinity;

        foreach (GameObject interactable in nearbyInteractables)
        {
            float distanceInteractable = (interactable.transform.position - transform.position).sqrMagnitude;

            if (distanceInteractable < nearestDistance)
            {
                nearestDistance = distanceInteractable;
                nearestInteractable = interactable;
            }
        }

        if (nearestInteractable != null)
        {
            switch (nearestInteractable.tag)
            {
                case "Pin":
                    TuneMinigame();
                    break;
                case "Key":
                    KeyMinigame();
                    break;
                default:
                    break;
            }
        }
        else
        {
            Debug.Log("Nothing to Interact Found");
        }
    }

    private void TuneMinigame()
    {
        TuneMinigameUI.SetActive(true);
        TuneMinigameUI.GetComponent<TuneManager>().StartTuneMinigame(nearestInteractable);
    }

    private void KeyMinigame()
    {
        nearestInteractable.GetComponent<RepairDestroy>().StartKeyMinigame();
    }

    #endregion

    private void FixedUpdate()
    {
        EndDash();

        Movement();

        ApplyCustomGravity();
    }
}
