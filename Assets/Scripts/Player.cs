using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    PlayerInput input;
    private Rigidbody playerRigidbody;
    Vector2 move;
    public float Speed = 0;
    public float SpeedMultiplier = 0;
    public float JumpForce = 0;
    private bool isJumping;
    

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
    }

    private void OnEnable()
    {
        input.Player.Enable();
    }

    private void OnDisable()
    {
        input.Player.Disable();
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isJumping = false;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isJumping = true;
        }
    }

    public void Jump()
    {
        if (isJumping == false)
        {
            playerRigidbody.AddForce(Vector3.up * JumpForce, ForceMode.Impulse);
        }
    }

    private void FixedUpdate()
    {
        Vector3 movement = new Vector3(move.x, 0.0f, move.y) * Speed * Time.deltaTime;
        transform.Translate(movement, Space.World);
    }
}
