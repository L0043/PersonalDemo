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
    public float JumpForce = 5f;
    public float DashForce = 10f;


    [SerializeField] Transform _cameraTransform;
    Vector2 _movementDirection = new Vector2();
    Vector2 _lookDirection = new Vector2();
    Vector3 _wantedDir = new Vector3();
    InputAction _aimAction;
    InputAction _moveAction;
    InputAction _jumpAction;
    InputAction _dashAction;
    float _viewPitch = 0.0f;
    float _viewYaw = 0.0f;
    bool _onGround = true;

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
        if (!_onGround)
            return;
        // Handle jump input
        if (context.performed)
        {
            //Jump();
            _rigidbody.AddForce(transform.up * JumpForce, ForceMode.Impulse); // Example jump force
        }
    }

    void OnDashInputRecieved(InputAction.CallbackContext context)
    {
        // Handle dash input
        if (context.performed)
        {
            //Dash();
            if(_wantedDir != Vector3.zero)
                _rigidbody.AddForce(_wantedDir * DashForce, ForceMode.Impulse); // Example dash force
            else
                _rigidbody.AddForce(transform.forward * DashForce, ForceMode.Impulse); // Example dash force

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
        Debug.Log(_rigidbody.velocity.magnitude);

        
        Debug.LogWarning(_wantedDir);
        Vector3 moveForce = _wantedDir * MoveSpeed;

        if (_onGround)
        {
            if (_wantedDir != Vector3.zero)
                _rigidbody.AddForce(moveForce, ForceMode.Force);

            // limit the velocity gained from input to a certain value
            if (_rigidbody.velocity.magnitude > MoveSpeed)
            {
                _rigidbody.velocity = _rigidbody.velocity.normalized * MoveSpeed;
            }
        }
        else 
        {
            if (_wantedDir != Vector3.zero)
            {
                //reduce the force from inputs while in air to prevent gliding
                moveForce = _wantedDir * MoveSpeed * 0.1f;
                _rigidbody.AddForce(moveForce, ForceMode.Force);
            }
            // limit the velocity gained from input to a certain value
            if (_rigidbody.velocity.magnitude > MoveSpeed * 1.1f)
            {
                _rigidbody.velocity = _rigidbody.velocity.normalized * MoveSpeed * 1.1f;
            }

        }


        // CAMERA
        _viewPitch = Mathf.Clamp(_viewPitch - _lookDirection.y, -80.0f, 70.0f);
        _viewYaw += _lookDirection.x;
       
        //rotate around
        transform.rotation = Quaternion.Euler(0, _viewYaw, 0);
        // aim up and down
        _cameraTransform.localRotation = Quaternion.Euler(_viewPitch,0f, 0f);


    }

    private void OnCollisionEnter(Collision collision)
    {
        var mask = LayerMask.NameToLayer("Environment");

        if (collision.gameObject.layer != mask)
            return;

        if (collision.contacts.Length > 0)
        {
            // Check if the contact point is below the player
            foreach (ContactPoint contact in collision.contacts)
            {
                if (Vector3.Dot(contact.normal, Vector3.up) > 0.5f)
                {
                    _onGround = true;
                    return;
                }
            }
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        // unless something else is added to this function,
        // there is no need to waste performance checking for collisions
        if (_onGround)
            return;

        var mask = LayerMask.NameToLayer("Environment");

        if (collision.gameObject.layer != mask)
            return;

        if (collision.contacts.Length > 0)
        {
            // Check if the contact point is below the player
            foreach (ContactPoint contact in collision.contacts)
            {
                if (Vector3.Dot(contact.normal, Vector3.up) > 0.5f)
                {
                    _onGround = true;
                    return;
                }
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.layer != LayerMask.NameToLayer("Environment"))
            return;
        _onGround = false;
    }

}
