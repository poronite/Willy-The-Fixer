using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using FMODUnity;

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

    [SerializeField]
    private GameObject KeysParent = null;

    private bool isMoving = false,
    onAir = false,
    isRolling = false,
    isOnStrings = false,
    canDescend = true,
    canClimb = true,
    isPaused = false;

    public bool leavingZone = false;

    private Rigidbody playerRigidbody;

    private Vector2 move;
    private float rollTimer = 0;
    private bool isBelowStrings = false;
    private List<GameObject> nearbyInteractables = new List<GameObject>();
    private GameObject nearestInteractable;

    public PlayerInput Input;
    public Animator WillyAnimator;
    public GameObject TuneMinigameUI;
    public GameObject KeyMinigameUI;
    public GameObject PauseMenu;
    public GameUI EnemyCount;
    public UITutorial Tutorial;
    public Footsteps WillyFootsteps;

    [HideInInspector]
    public string LastInputDevice;

    private float catchYamaTime = 0.0f;
    private float catchYamaCooldown = 0.5f;
    private bool caughtYama = false;
    #endregion

    private void Awake()
    {
        CheckDefaultInputDevice();
        playerRigidbody = gameObject.GetComponent<Rigidbody>();        
        SetInputs();
    }

    private void Start()
    {
        PianoMusic.Music.KeysParent = KeysParent;
    }

    #region Inputs
    private void SetInputs()
    {
        Input = new PlayerInput();

        Input.Player.Enable();
        Input.TuneMinigame.Disable();
        Input.RepairMinigame.Disable();

        //Movement
        Input.Player.Move.performed += context =>
        {
            LastInputDevice = context.control.device.name;

            if (Tutorial != null)
            {             
                Manager.ManagerInstance.MovementTutorialDone = true;
                Tutorial.DeactivateTutorial(Tutorial.WASD, Tutorial.LeftStick);
            }

            move = context.ReadValue<Vector2>();            
        };

        Input.Player.Move.canceled += context =>
        {
            LastInputDevice = context.control.device.name;
            move = Vector2.zero;

            if (!onAir)
            {
                Vector3 velocity = new Vector3(0, playerRigidbody.velocity.y, 0);

                playerRigidbody.velocity = velocity;
            }
        };

        //Jump
        Input.Player.Jump.performed += context =>
        {
            LastInputDevice = context.control.device.name;

            Jump();
        };

        //Roll
        Input.Player.Roll.performed += context =>
        {
            LastInputDevice = context.control.device.name;

            StartRoll();
        };

        //Fall
        Input.Player.Descend.performed += context =>
        {
            LastInputDevice = context.control.device.name;

            Descend();
        };

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
            LastInputDevice = context.control.device.name;

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

    private void CheckDefaultInputDevice()
    {
        Gamepad gamepad = Gamepad.current;

        if (gamepad != null)
        {
            LastInputDevice = "Gamepad";
        }
        else
        {
            LastInputDevice = "Keyboard";
        }
    }

    #endregion

    #region Collisions

    private void OnCollisionEnter(Collision collision)
    {
        //When the player jumps to climb the cords
        if (collision.gameObject.CompareTag("Strings") && isBelowStrings && canClimb)
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

        if (collision.gameObject.CompareTag("AI"))
        {
            if (isRolling && !caughtYama)
            {
                caughtYama = true;

                FMOD.Studio.EventInstance yamaCryInstance;

                yamaCryInstance = RuntimeManager.CreateInstance("event:/SFX/Characters/Yama/Yama Caught");
                yamaCryInstance.set3DAttributes(RuntimeUtils.To3DAttributes(collision.gameObject));
                yamaCryInstance.start();
                yamaCryInstance.release();


                Destroy(collision.gameObject);
                switch (SceneManager.GetActiveScene().name)
                {
                    case "UpperZonePiano":
                        Manager.ManagerInstance.NumUpperZoneYamas--;
                        EnemyCount.UpdateEnemyCountUI(Manager.ManagerInstance.NumUpperZoneYamas);
                        break;
                    case "LowerZonePiano":
                        Manager.ManagerInstance.NumLowerZoneYamas--;
                        EnemyCount.UpdateEnemyCountUI(Manager.ManagerInstance.NumLowerZoneYamas);
                        break;
                    default:
                        break;
                }
            }
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        //When the player is on the ground or strings
        if (collision.gameObject.CompareTag("WoodFloor") || collision.gameObject.CompareTag("MetalFloor") || collision.gameObject.CompareTag("Strings"))
        {
            float angle = Vector3.Angle(Vector3.up, collision.GetContact(0).normal);

            if (angle < groundThreshhold)
            {
                WillyFootsteps.ChangeSurfaceType(collision.gameObject.tag);
                onAir = false;
            }
        }

        //When the player is on the strings
        if (collision.gameObject.CompareTag("Strings"))
        {
            if (Tutorial != null && canDescend == true && Manager.ManagerInstance.DescendTutorialDone == false)
            {
                Tutorial.ActivateTutorial(Tutorial.CTRL, Tutorial.DPadDown);
            }

            isOnStrings = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        //When the player jumps from the ground or cords
        if (collision.gameObject.CompareTag("MetalFloor") || collision.gameObject.CompareTag("WoodFloor") || (collision.gameObject.CompareTag("Strings") && isOnStrings))
        {
            onAir = true;

            if (collision.gameObject.CompareTag("Strings") && isOnStrings)
            {
                if (Tutorial != null)
                {
                    Tutorial.DeactivateTutorial(Tutorial.CTRL, Tutorial.DPadDown);
                }
            }

            isOnStrings = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        switch (other.gameObject.tag)
        {
            case "Pin":
            case "Key":
                if (Tutorial != null && other.gameObject.GetComponent<PianoComponent>().IsRepaired == false && Manager.ManagerInstance.InteractTutorialDone == false)
                {
                    Tutorial.ActivateTutorial(Tutorial.E, Tutorial.Square);
                }
                if (other.GetComponent<PianoComponent>().IsRepaired == false)
                {
                    nearbyInteractables.Add(other.gameObject);
                }
                break;
            case "ChangePerspective":
                if (leavingZone == false)
                {
                    switch (other.name)
                    {
                        case "AboveStrings":
                            isBelowStrings = false;
                            Manager.ManagerInstance.ChangeCameraY(4f);
                            break;
                        case "BelowStrings":
                            isBelowStrings = true;
                            if (Tutorial != null && Manager.ManagerInstance.JumpTutorialDone == false)
                            {
                                Tutorial.ActivateTutorial(Tutorial.Space, Tutorial.X);
                            }
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

                WillyAnimator.gameObject.GetComponent<CapsuleCollider>().enabled = false; //just so this doesn't trigger twice

                Manager.ManagerInstance.ChangeCameraTarget(null);

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
            case "PreventClimb":
                canClimb = false;
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
                if (nearbyInteractables.Contains(other.gameObject))
                {
                    nearbyInteractables.Remove(other.gameObject);
                }
                if (Tutorial != null && nearbyInteractables.Count == 0)
                {
                    Tutorial.DeactivateTutorial(Tutorial.E, Tutorial.Square);
                }
                break;
            case "PreventDescend":
                canDescend = true;
                break;
            case "PreventClimb":
                canClimb = true;
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
                playerRigidbody.rotation = Quaternion.Slerp(playerRigidbody.rotation, Quaternion.LookRotation(movement), 0.5f);
                
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
            if (Tutorial != null)
            {                
                Manager.ManagerInstance.JumpTutorialDone = true;
                Tutorial.DeactivateTutorial(Tutorial.Space, Tutorial.X);
            }

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
            if (Tutorial != null)
            {
                Manager.ManagerInstance.RollTutorialDone = true;
                Tutorial.DeactivateTutorial(Tutorial.RightMouse, Tutorial.Circle);
            }

            FMOD.Studio.EventInstance RollInstance;

            RollInstance = RuntimeManager.CreateInstance("event:/SFX/Characters/Willy/Roll");
            RollInstance.set3DAttributes(RuntimeUtils.To3DAttributes(gameObject));
            RollInstance.start();
            RollInstance.release();

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
            if (Tutorial != null)
            {
                Manager.ManagerInstance.DescendTutorialDone = true;
                Tutorial.DeactivateTutorial(Tutorial.CTRL, Tutorial.DPadDown);
            }

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
            if (Tutorial != null)
            {
                Manager.ManagerInstance.InteractTutorialDone = true;
                Tutorial.DeactivateTutorial(Tutorial.E, Tutorial.Square);
            }

            TuneMinigameUI.SetActive(true);
            TuneMinigameUI.GetComponent<TuneManager>().StartTuneMinigame(nearestInteractable);
        }
    }

    private void KeyMinigame()
    {
        KeyMinigameUI.GetComponent<KeyMinigame>().StartKeyMinigame(nearestInteractable, LastInputDevice);
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

    private void hasCaughtYama()
    {
        if (caughtYama)
        {
            catchYamaTime += Time.deltaTime;
        }

        if (catchYamaTime >= catchYamaCooldown)
        {
            catchYamaTime = 0.0f;
            caughtYama = false;
        }
    }

    #endregion

    private void FixedUpdate()
    {
        hasCaughtYama(); //this is because the OnCollisionEnter is triggering twice

        EndDash();

        Movement();

        ApplyCustomGravity();
    }
}
