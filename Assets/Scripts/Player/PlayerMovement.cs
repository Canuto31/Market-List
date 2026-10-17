using System;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInputHandler))]

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")] 
    [SerializeField] private float walkSpeed = 4f;
    [SerializeField] private float sprintSpeed = 7f;
    
    [Header("Gravity")]
    [SerializeField] private float _gravity = -9.81f;
    [SerializeField] private float _gorundedForce = -2f;
    
    private CharacterController _controller;
    private PlayerInputHandler _inputHandler;
    
    private float _verticalVelocity;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _inputHandler = GetComponent<PlayerInputHandler>();
    }

    private void Update()
    {
        HandleMovement();
        HandleGravity();
    }

    private void HandleMovement()
    {
        Vector2 input = Vector2.ClampMagnitude(_inputHandler.MoveInput, 1f);

        float speed = _inputHandler.IsSprinting ? sprintSpeed : walkSpeed;

        Vector3 direction = transform.right * input.x + transform.forward * input.y;

        Vector3 horizontalMovement = direction * (speed * Time.deltaTime);
        
        _controller.Move(horizontalMovement);
    }

    private void HandleGravity()
    {
        if (_controller.isGrounded && _verticalVelocity < 0)
            _verticalVelocity = _gorundedForce;
        else
            _verticalVelocity += _gravity * Time.deltaTime;
        
        Vector3 verticalMovement = Vector3.up * (_verticalVelocity * Time.deltaTime);
        
        _controller.Move(verticalMovement);
    }
    
}
