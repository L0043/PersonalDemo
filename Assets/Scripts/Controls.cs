using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody)), RequireComponent(typeof(PlayerInput))]
public class Controls : MonoBehaviour
{
    [SerializeField] Rigidbody _rigidbody;
    public float MoveSpeed = 5f;
    public float Sensitivity = 0.1f;
    Vector2 _movementDirection = new Vector2();
    Vector2 _lookDirection = new Vector2();
    Vector3 _wantedDir = new Vector3();
    InputAction _aimAction;
    InputAction _moveAction;
    InputAction _jumpAction;
    InputAction _dashAction;
    float _viewPitch = 0.0f;
    float _viewYaw = 0.0f;
    [SerializeField] Transform _cameraTransform;

    void OnMoveInputRecieved(InputAction.CallbackContext context)
    {
        _movementDirection = context.ReadValue<Vector2>();
    }

    void OnAimInputRecieved(InputAction.CallbackContext context)
    {
        _lookDirection = context.ReadValue<Vector2>() * Sensitivity;
        // Handle aim direction
    }

    void OnJumpInputRecieved(InputAction.CallbackContext context)
    {
        // Handle jump input
        if (context.performed)
        {
            //Jump();
            _rigidbody.AddForce(Vector3.up * 5f, ForceMode.Impulse); // Example jump force
        }
    }

    void OnDashInputRecieved(InputAction.CallbackContext context)
    {
        // Handle dash input
        if (context.performed)
        {
            //Dash();
            if(_wantedDir != Vector3.zero)
                _rigidbody.AddForce(_wantedDir * 10f, ForceMode.Impulse); // Example dash force
            else
                _rigidbody.AddForce(transform.forward * 10f, ForceMode.Impulse); // Example dash force

        }
    }

    // Start is called before the first frame update
    void Start()
    {
        //lock mouse to game window
        Cursor.lockState = CursorLockMode.Locked;


        if (!_rigidbody)
            _rigidbody = GetComponent<Rigidbody>();
        var inputMap = GetComponent<PlayerInput>().currentActionMap;
        _moveAction = inputMap.FindAction("Move");
        _aimAction = inputMap.FindAction("Look");
        _jumpAction = inputMap.FindAction("Jump");
        _dashAction = inputMap.FindAction("Dash");

        _moveAction.performed += OnMoveInputRecieved;
        _moveAction.canceled += OnMoveInputRecieved;
        _aimAction.performed += OnAimInputRecieved;
        _aimAction.canceled += OnAimInputRecieved;
        _jumpAction.performed += OnJumpInputRecieved;
        _dashAction.performed += OnDashInputRecieved;
    }

   
    void Update()
    {
        // MOVEMENT
        _wantedDir = transform.forward * _movementDirection.y + transform.right * _movementDirection.x;
        if (_wantedDir != Vector3.zero)
            _rigidbody.AddForce(_wantedDir * MoveSpeed, ForceMode.Force);


        // CAMERA
        _viewPitch = Mathf.Clamp(_viewPitch - _lookDirection.y, -80.0f, 70.0f);
        _viewYaw += _lookDirection.x;
       
        //rotate around
        transform.rotation = Quaternion.Euler(0, _viewYaw, 0);
        // aim up and down
        _cameraTransform.localRotation = Quaternion.Euler(_viewPitch,0f, 0f);


    }
}
