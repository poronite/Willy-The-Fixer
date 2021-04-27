using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    #region Variables
    public PlayerInput Input;
    private Rigidbody playerRigidbody;
    private Camera MainCamera;

    Vector2 move;

    public float Speed = 0;
    public float SpeedMultiplier = 0;

    public float JumpForce = 0;
    private bool onAir;
    public float GroundThreshhold = 5.0f;
    private Vector3 groundUp = Vector3.up;

    public float JumpGravityScale = 1.0f;
    public float FallGravityScale = 1.0f;

    public float DashSpeed = 0;
    public float DashDuration = 0;
    private float dashTimer = 0;
    private bool isDashing = false;

    private bool isOnCords = true;

    private GameObject nearestTune;
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
        Input.Player.Sprint.performed += context => Speed *= SpeedMultiplier;
        Input.Player.Sprint.canceled += context => Speed /= SpeedMultiplier;

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
        if (collision.gameObject.CompareTag("Cords") && playerRigidbody.velocity.y > 0)
        {
            //"Climb" the cords by changing y of the player
            Vector3 climbCords = gameObject.transform.position;
            climbCords.y = 15f;
            gameObject.transform.position = climbCords;

            //Re-use the Vector3 just to cancel the speed of the jump
            climbCords = playerRigidbody.velocity;
            climbCords.y = 0.0f;
            playerRigidbody.velocity = climbCords;
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        //When the player is on the ground 
        if (collision.gameObject.CompareTag("Ground"))
        {
            float angle = Vector3.Angle(groundUp, collision.contacts[0].normal);

            if (angle < GroundThreshhold)
            {
                onAir = false;
            }
        }

        //When the player is on the cords
        if (collision.gameObject.CompareTag("Cords"))
        {
            onAir = false;
            isOnCords = true;
        }

        //for tune game
        if (collision.gameObject.CompareTag("Tune"))
        {
            nearestTune = collision.gameObject;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        //When the player jumps from the ground or cords
        if (collision.gameObject.CompareTag("Ground") || (collision.gameObject.CompareTag("Cords") && isOnCords == true))
        {
            onAir = true;
            isOnCords = false;
        }

        //for tune game
        if (collision.gameObject.CompareTag("Tune"))
        {
            nearestTune = null;
        }
    }
    #endregion

    #region PlayerMechanics
    public void Movement()
    {
        Vector3 horizontal = Vector3.Cross(-MainCamera.transform.forward, playerRigidbody.transform.up).normalized;
        Vector3 vertical = Vector3.Cross(horizontal, Vector3.up).normalized;

        Vector3 movement = new Vector3(move.x * horizontal.x, 0.0f, move.y * vertical.z).normalized * Speed * Time.deltaTime;
        transform.Translate(movement, Space.World);
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
            gravity *= JumpGravityScale;
        }

        if (playerRigidbody.velocity.y < 0)
        {
            gravity *= FallGravityScale;
        }

        playerRigidbody.AddForce(gravity, ForceMode.Acceleration);
    }

    public void StartDash()
    {
        if (isDashing == false)
        {
            Vector3 horizontal = Vector3.Cross(-MainCamera.transform.forward, playerRigidbody.transform.up).normalized;
            Vector3 vertical = Vector3.Cross(horizontal, Vector3.up).normalized;

            playerRigidbody.velocity = new Vector3(move.x * horizontal.x, 0.0f, move.y * vertical.z) * DashSpeed;
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
        if (isOnCords == true)
        {
            //Same logic as climbing
            Vector3 descendCords = gameObject.transform.position;
            descendCords.y = 14.3f;
            gameObject.transform.position = descendCords;

            onAir = true;
            isOnCords = false;
        }
    }

    public void Interact()
    {
        switch (nearestTune.tag)
        {
            case "Tune":
                if (nearestTune != null)
                {
                    //enter mini game
                    TuneMinigame.SetActive(true);
                    Input.asset.FindActionMap("Player").Disable();
                    TuneMinigame.GetComponent<TuneManager>().TuneMinigame();
                }
                break;
            default:
                break;
        }
    }

    #endregion

    private void FixedUpdate()
    {
        EndDash();

        Movement();

        ApplyCustomGravity();
    }
}
