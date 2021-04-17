using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    #region Variables
    PlayerInput input;
    private Rigidbody playerRigidbody;

    Vector2 move;

    public float Speed = 0;
    public float SpeedMultiplier = 0;

    public float JumpForce = 0;
    private bool isJumping;
    public float JumpGravityScale = 1.0f;
    public float FallGravityScale = 1.0f;

    public float DashSpeed = 0;
    public float DashDuration = 0;
    private float dashTimer = 0;
    private bool isDashing = false;
    private bool isOnCords = true;
    #endregion

    #region Inputs
    private void Awake()
    {
        playerRigidbody = gameObject.GetComponent<Rigidbody>();
        SetInputs();
    }

    private void SetInputs()
    {
        input = new PlayerInput();

        //Movement
        input.Player.Move.performed += context => move = context.ReadValue<Vector2>();
        input.Player.Move.canceled += context => move = Vector2.zero;

        //Sprint
        input.Player.Sprint.performed += context => Speed *= SpeedMultiplier;
        input.Player.Sprint.canceled += context => Speed /= SpeedMultiplier;

        //Jump
        input.Player.Jump.performed += context => Jump();

        //Dash
        input.Player.Dash.performed += context => StartDash();

        //Fall
        input.Player.Descend.performed += context => Descend();
    }
    
    private void OnEnable()
    {
        input.Player.Enable();
    }

    private void OnDisable()
    {
        input.Player.Disable();
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
            isJumping = false;
        }

        //When the player is on the cords
        if (collision.gameObject.CompareTag("Cords"))
        {
            isJumping = false;
            isOnCords = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        //When the player jumps from the ground or cords
        if (collision.gameObject.CompareTag("Ground") || (collision.gameObject.CompareTag("Cords") && isOnCords == true))
        {
            isJumping = true;
            isOnCords = false;
        }
    }
    #endregion

    #region PlayerMechanics
    public void Movement()
    {
        Vector3 movement = new Vector3(move.x, 0.0f, move.y) * Speed * Time.deltaTime;
        transform.Translate(movement, Space.World);
    }

    public void Jump()
    {
        if (isJumping == false)
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
            playerRigidbody.velocity = new Vector3(move.x, 0.0f, move.y) * DashSpeed;
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

            isJumping = true;
            isOnCords = false;
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
