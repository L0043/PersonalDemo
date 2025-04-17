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
    public float AirMoveSpeed = 5f;
    public float MoveSpeedLimit = 15f;
    public float Sensitivity = 0.1f;
    public float JumpForce = 5f;
    public float SlamForce = 5f;
    public float DashForce = 10f;
    public float AirMoveInputContraint = 0.1f;
    public float AirSpeedLimitMultiplier = 1.1f;


    [SerializeField] Transform _cameraTransform;
    Vector2 _movementDirection = new Vector2();
    Vector2 _lookDirection = new Vector2();
    Vector3 _wantedDir = new Vector3();
    Vector3 _airDirection = new Vector3();
    InputAction _aimAction;
    InputAction _moveAction;
    InputAction _jumpAction;
    InputAction _dashAction;
    InputAction _slamAction;
    InputAction _teleportAction;
    float _viewPitch = 0.0f;
    float _viewYaw = 0.0f;
    bool _onGround = false;

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
        _slamAction = inputMap.FindAction("Slam");
        _teleportAction = inputMap.FindAction("Teleport");

        _moveAction.performed += OnMoveInputRecieved;
        _moveAction.canceled += OnMoveInputRecieved;
        _aimAction.performed += OnAimInputRecieved;
        _aimAction.canceled += OnAimInputRecieved;
        _jumpAction.performed += OnJumpInputRecieved;
        _dashAction.performed += OnDashInputRecieved;
        _slamAction.performed += OnSlamInputRecieved;
        _teleportAction.performed += OnTeleportInputRecieved;
    }

   
    void FixedUpdate()
    {
        // MOVEMENT
        _wantedDir = transform.forward * _movementDirection.y + transform.right * _movementDirection.x;

        if (_onGround)
        {
            _rigidbody.velocity = new Vector3(_wantedDir.x * MoveSpeed * Time.fixedDeltaTime, _rigidbody.velocity.y, _wantedDir.z * MoveSpeed * Time.fixedDeltaTime);
        }
        else 
        {
            Vector3 movementDirection = new Vector3(_wantedDir.x * MoveSpeed * Time.fixedDeltaTime, _rigidbody.velocity.y, _wantedDir.z * MoveSpeed * Time.fixedDeltaTime);
            _airDirection.y = 0f;
            // movedir > 0 and velocity < movedir or movedir < 0 and velocity > movedir
            // if the player is moving in the air, we want to apply a force in the direction of the movement
            // but only if the player is not already moving in that direction

            if ((movementDirection.x > 0f && _rigidbody.velocity.x < movementDirection.x) || (movementDirection.x < 0f && _rigidbody.velocity.x > movementDirection.x))
            {
                _airDirection.x = movementDirection.x;
            }
            else
            {
                _airDirection.x = 0f;
            }
            if ((movementDirection.z > 0f && _rigidbody.velocity.z < movementDirection.z) || (movementDirection.z < 0f && _rigidbody.velocity.z > movementDirection.z))
            {
                _airDirection.z = movementDirection.z;
            }
            else
            {
                _airDirection.z = 0f;
            }
            _rigidbody.AddForce(_airDirection.normalized * AirMoveSpeed * Time.fixedDeltaTime);
        }
    }

    private void Update()
    {
        // CAMERA
        _viewPitch = Mathf.Clamp(_viewPitch - _lookDirection.y, -80.0f, 70.0f);
        _viewYaw += _lookDirection.x;

        //rotate around
        transform.rotation = Quaternion.Euler(0, _viewYaw, 0);
        // aim up and down
        _cameraTransform.localRotation = Quaternion.Euler(_viewPitch, 0f, 0f);
    }
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
            _rigidbody.velocity = new Vector3(_rigidbody.velocity.x, 0f, _rigidbody.velocity.z);
            _rigidbody.AddForce(Vector3.up * JumpForce * 1000f);
        }
    }

    void OnDashInputRecieved(InputAction.CallbackContext context)
    {
        // Handle dash input
        if (context.performed)
        {
            //Dash();
            if (_wantedDir != Vector3.zero)
                _rigidbody.AddForce(_wantedDir * DashForce, ForceMode.VelocityChange); // Example dash force
            else
                _rigidbody.AddForce(transform.forward * DashForce, ForceMode.VelocityChange); // Example dash force

        }
    }

    void OnSlamInputRecieved(InputAction.CallbackContext context)
    {
        // Handle slam input
        if (context.performed)
        {
            Slam();
        }
    }

    void Slam() 
    {
        _rigidbody.velocity = Vector3.zero;
        _rigidbody.AddForce(Vector3.down * SlamForce * 1000f, ForceMode.Impulse);
    }

    void OnTeleportInputRecieved(InputAction.CallbackContext context)
    {
        // Handle teleport input
        if (context.performed)
        {
            Teleport();
        }
    }

    void Teleport() 
    {
        // Teleport the player to a random position within a certain range
        // Change velocity to be in direction of teleport aiming

        Vector3 teleportPosition = transform.position + _cameraTransform.forward * 10f;
        transform.position = teleportPosition;

        _rigidbody.velocity = Vector3.Project(_rigidbody.velocity, transform.position + _cameraTransform.forward);

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
