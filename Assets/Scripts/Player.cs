using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    PlayerInput input;
    Vector2 move;
    public float Speed = 0;
    public float SpeedMultiplier = 0;

    private void Awake()
    {
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
    }

    private void OnEnable()
    {
        input.Player.Enable();
    }

    private void OnDisable()
    {
        input.Player.Disable();
    }

    private void FixedUpdate()
    {
        Vector3 movement = new Vector3(move.x, 0.0f, move.y) * Speed * Time.deltaTime;
        transform.Translate(movement, Space.World);
    }
}
