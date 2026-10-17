using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    private PlayerControls _controls;
    
    public Vector2 MoveInput { get; private set; }
    public Vector2 LookInput { get; private set; }
    public bool IsSprinting { get; private set; }

    private void Awake()
    {
        _controls = new PlayerControls();
    }

    private void OnEnable()
    {
        _controls.Player.Move.performed += OnMove;
        _controls.Player.Move.canceled += OnMove;
        
        _controls.Player.Look.performed += OnLook;
        _controls.Player.Look.canceled += OnLook;
        
        _controls.Player.Sprint.performed += OnSprint;
        _controls.Player.Sprint.canceled += OnSprint;

        _controls.Player.Enable();
    }

    private void OnDisable()
    {
        _controls.Player.Disable();
        
        _controls.Player.Move.performed -= OnMove;
        _controls.Player.Move.canceled -= OnMove;
        
        _controls.Player.Look.performed -= OnLook;
        _controls.Player.Look.canceled -= OnLook;
        
        _controls.Player.Sprint.performed -= OnSprint;
        _controls.Player.Sprint.canceled -= OnSprint;
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        MoveInput = context.ReadValue<Vector2>();
    }

    private void OnLook(InputAction.CallbackContext context)
    {
        LookInput = context.ReadValue<Vector2>();
    }

    private void OnSprint(InputAction.CallbackContext context)
    {
        IsSprinting = context.ReadValueAsButton();
    }
}
