﻿using System.Collections.Generic;
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

    private bool isMoving = false, 
    onAir = false,
    isRolling = false,
    isOnStrings = false,
    canDescend = true,
    isPaused = false;

    public bool leavingZone = false;

    private Rigidbody playerRigidbody;

    private Vector2 move;
    private float rollTimer = 0;
    private List<GameObject> nearbyInteractables = new List<GameObject>();
    private GameObject nearestInteractable;

    public PlayerInput Input;
    public Animator WillyAnimator;
    public GameObject TuneMinigameUI;
    public GameObject PauseMenu;

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
            if (!onAir)
            {
                LastInputDevice = context.control.device.name;
                Interact();
            }
        };

        Input.Player.Pause.performed += context =>
        {
            if (SceneManager.GetActiveScene().name != "MainMenu")
            {
                if (isPaused)
                {
                    isPaused = false;
                    ResumeGame();
                }
                else
                {
                    isPaused = true;
                    PauseGame();
                }                
            }
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
        if (collision.gameObject.CompareTag("Strings") && gameObject.transform.position.y + 0.7f < 2.0f)
        {
            //"Climb" the cords by changing y of the player
            Vector3 climbStrings = gameObject.transform.position;
            climbStrings.y = 2.5f;
            gameObject.transform.position = climbStrings;

            //Re-use the Vector3 just to cancel the speed of the jump
            climbStrings = playerRigidbody.velocity;
            climbStrings.y = 0.0f;
            playerRigidbody.velocity = climbStrings;
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
        switch (other.gameObject.tag)
        {
            case "Pin":
            case "Key":
                nearbyInteractables.Add(other.gameObject);
                break;
            case "AI":
                if (isRolling)
                {
                    Destroy(other.gameObject);
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
                break;
            case "ChangePerspective":
                if (leavingZone == false)
                {
                    switch (other.name)
                    {
                        case "AboveStrings":
                            Manager.ManagerInstance.ChangeCameraY(5f);
                            break;
                        case "BelowStrings":
                            Manager.ManagerInstance.ChangeCameraY(1f);
                            break;
                        default:
                            break;
                    }
                }
                break;
            case "Exit":
                leavingZone = true;

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
                break;
            case "PreventDescend":
                canDescend = false;
                break;
            default:
                break;
        }        
    }

    private void OnTriggerExit(Collider other)
    {
        switch (other.gameObject.tag)
        {
            case "Pin":
            case "Key":
                nearbyInteractables.Remove(other.gameObject);
                break;
            case "PreventDescend":
                canDescend = true;
                break;
            default:
                break;
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
                isMoving = true;
                playerRigidbody.rotation = Quaternion.Slerp(playerRigidbody.rotation, Quaternion.LookRotation(movement), 0.3f);
                //playerRigidbody.rotation = Quaternion.LookRotation(movement);
            }
            else
            {
                isMoving = false;
            }

            WillyAnimator.SetBool("isMoving", isMoving);

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
            WillyAnimator.SetTrigger("Jump");            
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
            WillyAnimator.ResetTrigger("Jump"); //the trigger takes too long to reset without this, why???
        }

        WillyAnimator.SetFloat("VelocityY", playerRigidbody.velocity.y);
        WillyAnimator.SetBool("OnAir", onAir);

        playerRigidbody.AddForce(gravity, ForceMode.Acceleration);
    }

    public void StartRoll()
    {
        if (!isRolling && !onAir) //can't roll while jumping
        {
            WillyAnimator.Play("Run", 0);
            WillyAnimator.SetTrigger("Roll");
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
        if (isOnStrings && canDescend)
        {
            //Same logic as climbing
            Vector3 descendCords = gameObject.transform.position;
            descendCords.y = 0.4f;
            gameObject.transform.position = descendCords;

            onAir = true;
            isOnStrings = false;            
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
        if (!nearestInteractable.GetComponent<PianoComponent>().IsRepaired)
        {
            TuneMinigameUI.SetActive(true);
            TuneMinigameUI.GetComponent<TuneManager>().StartTuneMinigame(nearestInteractable);
        }
    }

    private void KeyMinigame()
    {
        nearestInteractable.GetComponent<RepairDestroy>().StartKeyMinigame(LastInputDevice);
    }

    public void PauseGame()
    {
        PauseMenu.SetActive(true);
        PauseMenu.GetComponent<PauseMenu>().PauseGame();
    }

    public void ResumeGame()
    {
        PauseMenu.GetComponent<PauseMenu>().ResumeGame();
    }

    #endregion

    private void FixedUpdate()
    {
        EndDash();

        Movement();

        ApplyCustomGravity();
    }
}
