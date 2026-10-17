using System;
using UnityEngine;

[RequireComponent(typeof(PlayerInputHandler))]
public class PlayerLook : MonoBehaviour
{
    [SerializeField] private Transform _cameraHolder;
    [SerializeField] private float _sensitivity = 0.12f;
    [SerializeField] private float _maxLookAngle = 85f;
    
    private PlayerInputHandler _inputHandler;
    private float verticalRotation;

    private void Awake()
    {
        _inputHandler = GetComponent<PlayerInputHandler>();
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        Vector2 look = _inputHandler.LookInput;
        
        transform.Rotate(Vector3.up, look.x * _sensitivity);
        
        verticalRotation -= look.y * _sensitivity;
        verticalRotation = Mathf.Clamp(verticalRotation, -_maxLookAngle, _maxLookAngle);
        
        _cameraHolder.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
    }
}
