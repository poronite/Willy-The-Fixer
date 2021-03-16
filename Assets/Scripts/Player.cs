using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    PlayerInput input;
    Vector2 move;
    public float speed = 0;

    private void Awake()
    {
        input = new PlayerInput();
        input.Player.Move.performed += context => move = context.ReadValue<Vector2>();
        input.Player.Move.canceled += context => move = Vector2.zero;
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
        Vector3 movement = new Vector3(move.x, 0.0f, move.y) * speed * Time.deltaTime;
        transform.Translate(movement, Space.World);
    }
}
