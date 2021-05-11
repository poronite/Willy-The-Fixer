using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    #region Variables
    public PlayerInput Input;
    private Rigidbody playerRigidbody;
    private Camera MainCamera;

    private Vector2 move;

    [SerializeField]
    private float Acceleration = 0,
    maxSpeed = 0,
    SprintMultiplier = 0, 
    JumpForce = 0, 
    GroundThreshhold = 5.0f, 
    JumpGravityScale = 1.0f, 
    FallGravityScale = 1.0f, 
    DashSpeed = 0, 
    DashDuration = 0;

    private bool onAir;
    private float dashTimer = 0;
    private bool isSprinting = false;
    private bool isDashing = false;
    private bool isOnStrings = true;
    private bool hasClimbed = false;

    private GameObject NearestInteractable;
    public GameObject TuneMinigame;
    public string LastInputDevice;
    #endregion

    #region Inputs
    private void Awake()
    {
        playerRigidbody = gameObject.GetComponent<Rigidbody>();
        MainCamera = Camera.main;
        SetInputs();
    }

    private void SetInputs()
    {
        Input = new PlayerInput();

        //Movement
        Input.Player.Move.performed += context => move = context.ReadValue<Vector2>();
        Input.Player.Move.canceled += context => move = Vector2.zero;

        //Sprint
        Input.Player.Sprint.performed += context => isSprinting = true;
        Input.Player.Sprint.canceled += context => isSprinting = false;

        //Jump
        Input.Player.Jump.performed += context => Jump();

        //Dash
        Input.Player.Dash.performed += context => StartDash();

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
        if (collision.gameObject.CompareTag("Strings") && hasClimbed == false)
        {
            hasClimbed = true;

            //"Climb" the cords by changing y of the player
            Vector3 climbStrings = gameObject.transform.position;
            climbStrings.y = 15f;
            gameObject.transform.position = climbStrings;

            //Re-use the Vector3 just to cancel the speed of the jump
            climbStrings = playerRigidbody.velocity; 
            climbStrings.y = 0.0f;
            playerRigidbody.velocity = climbStrings;

            Manager.ManagerInstance.ChangeCameraOffset(5f);
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        //When the player is on the ground 
        if (collision.gameObject.CompareTag("Ground"))
        {
            float angle = Vector3.Angle(Vector3.up, collision.GetContact(0).normal);

            if (angle < GroundThreshhold)
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

        if (collision.gameObject.CompareTag("AI") && isDashing == true)
        {
            Destroy(collision.gameObject);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        //When the player jumps from the ground or cords
        if (collision.gameObject.CompareTag("Ground") || (collision.gameObject.CompareTag("Strings") && isOnStrings == true))
        {
            onAir = true;
            isOnStrings = false;
        }

        //for tune game
        if (collision.gameObject.CompareTag("Tune"))
        {
            NearestInteractable = null;
        }
    }

    private void OnTriggerEnter(Collider exit)
    {
        //to change between zones of the piano
        if (exit.gameObject.CompareTag("Exit"))
        {
            Input.asset.FindActionMap("Player").Disable();

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

    private void OnTriggerStay(Collider other)
    {
        //for tune game
        if (other.gameObject.CompareTag("Tune"))
        {
            NearestInteractable = other.gameObject;
        }
    }

    #endregion

    #region PlayerMechanics
    public void Movement()
    {
        //Vector3 horizontal = Vector3.Cross(-MainCamera.transform.forward, playerRigidbody.transform.up).normalized;
        //Vector3 vertical = Vector3.Cross(horizontal, Vector3.up).normalized;

        if (!isDashing)
        {
            float finalAcceleration = Acceleration;

            if (isSprinting)
            {
                finalAcceleration *= SprintMultiplier;
            }

            Vector3 movement = new Vector3(-move.x, 0.0f, -move.y).normalized * finalAcceleration;


            if (movement != Vector3.zero)
            {
                playerRigidbody.rotation = Quaternion.Slerp(playerRigidbody.rotation, Quaternion.LookRotation(movement), 0.15f);
            }

            playerRigidbody.AddForce(movement);

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
        if (onAir == false)
        {
            playerRigidbody.AddForce(Vector3.up * JumpForce, ForceMode.Impulse);
        }
    }

    public void ApplyCustomGravity()
    {
        Vector3 gravity = Physics.gravity;

        if (playerRigidbody.velocity.y > 0)
        {
            gravity*= JumpGravityScale;
        }

        if (playerRigidbody.velocity.y < 0)
        {
            gravity*= FallGravityScale;
        }

        playerRigidbody.AddForce(gravity, ForceMode.Acceleration);
    }

    public void StartDash()
    {
        if (isDashing == false)
        {
            //Vector3 horizontal = Vector3.Cross(-MainCamera.transform.forward, playerRigidbody.transform.up).normalized;
            //Vector3 vertical = Vector3.Cross(horizontal, Vector3.up).normalized;

            playerRigidbody.AddForce(new Vector3(-move.x, 0.0f, -move.y) * DashSpeed, ForceMode.Impulse);

            //playerRigidbody.velocity = new Vector3(-move.x, 0.0f, -move.y) * DashSpeed;
            dashTimer = 0;
            isDashing = true;
        }
    }

    public void EndDash()
    {
        if (isDashing == true)
        {
            dashTimer += Time.deltaTime;

            if (dashTimer >= DashDuration)
            {
                Vector3 cancelDash = playerRigidbody.velocity;
                cancelDash.x = 0.0f;
                cancelDash.z = 0.0f;
                playerRigidbody.velocity = cancelDash;
                isDashing = false;
            }
        }
    }

    public void Descend()
    {
        if (isOnStrings == true)
        {
            //Same logic as climbing
            Vector3 descendCords = gameObject.transform.position;
            descendCords.y = 13.5f;
            gameObject.transform.position = descendCords;

            Manager.ManagerInstance.ChangeCameraOffset(1f);

            onAir = true;
            isOnStrings = false;
            hasClimbed = false;
        }
    }

    public void Interact()
    {
        if (NearestInteractable != null)
        {
            switch (NearestInteractable.tag)
            {
                case "Tune":
                    startTuneMinigame();
                    break;
                default:
                    break;
            }
        }
        else
        {
            Debug.Log("Nothing to Interact Found");
            return;
        }
        
    }

    private void startTuneMinigame()
    {
        //enter mini game
        TuneMinigame.SetActive(true);
        Input.asset.FindActionMap("Player").Disable();
        TuneMinigame.GetComponent<TuneManager>().TuneMinigame();
    }

    #endregion

    private void FixedUpdate()
    {
        EndDash();

        Movement();

        ApplyCustomGravity();
    }
}
