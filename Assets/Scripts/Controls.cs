using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEditor.Timeline.TimelinePlaybackControls;

[RequireComponent(typeof(Rigidbody)), RequireComponent(typeof(PlayerInput))]
public class Controls : MonoBehaviour
{
    [SerializeField] Rigidbody _rigidbody;
    [SerializeField] CapsuleCollider _collider;
    public float MoveSpeed = 5f;
    public float AirMoveSpeed = 5f;
    public float MoveSpeedLimit = 15f;
    public float Sensitivity = 0.1f;
    public float JumpForce = 5000f;
    public float DashForce = 10f;
    public float AirMoveInputContraint = 0.1f;
    public float AirSpeedLimitMultiplier = 1.1f;

    [Space]
    [Header("Slam")]
    public float SlamForce = 5000f;
    bool _isSlamming = false;


    [Space]
    [Header("Teleport Variables")]
    public float TeleportCooldown = 2f;
    public float TeleportDistance = 10f;
    public float DownwardTeleportVelocityReduction = 0.1f;
    [Tooltip("used as part of a dot product check to determine at what downward velocity angle the velocity reduction will take place")]
    [SerializeField] float _downTeleportReductionThreshold = 0.5f;
    float _teleportTimer = 0.0f;
    [Space]


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
        if(!_collider)
            _collider = GetComponent<CapsuleCollider>();

        var inputMap = GetComponent<PlayerInput>().currentActionMap;
        _moveAction = inputMap.FindAction("Move");
        _aimAction = inputMap.FindAction("Look");
        _jumpAction = inputMap.FindAction("Jump");
        _dashAction = inputMap.FindAction("Dash");
        _slamAction = inputMap.FindAction("Slam");
        _teleportAction = inputMap.FindAction("Teleport");

        _moveAction.started += OnMoveInputRecieved;
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

        if(_viewYaw >= 360f)
            _viewYaw -= 360f;
        else if (_viewYaw < 0f)
            _viewYaw += 360f;

        //rotate around
        transform.rotation = Quaternion.Euler(0, _viewYaw, 0);
        // aim up and down
        _cameraTransform.localRotation = Quaternion.Euler(_viewPitch, 0f, 0f);

        if(_teleportTimer >= 0.0f)
            _teleportTimer -= Time.deltaTime;

        // allow immediate movement after slamming
        if (_moveAction.IsInProgress() && !_isSlamming) 
        {
            // Call the moveinput received function
            _movementDirection = _moveAction.ReadValue<Vector2>();

        }

    }
    void OnMoveInputRecieved(InputAction.CallbackContext context)
    {
        // lock the player to slamming downwards, this can be canceled by any other ability being used however
        if(!_isSlamming)
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
            Jump();
        }
    }

    void Jump() 
    {
        _rigidbody.velocity = new Vector3(_rigidbody.velocity.x, 0f, _rigidbody.velocity.z);
        _rigidbody.AddForce(Vector3.up * JumpForce);
    }

    void OnDashInputRecieved(InputAction.CallbackContext context)
    {
        // Handle dash input
        if (context.performed)
        {
            Dash();

        }
    }

    void Dash()
    {
        // stop all velocity, then add force in the direction of movement
        _rigidbody.velocity = Vector3.zero;
        _isSlamming = false;
        if (_wantedDir != Vector3.zero)
            _rigidbody.AddForce(_wantedDir * DashForce, ForceMode.VelocityChange);
        else
            _rigidbody.AddForce(transform.forward * DashForce, ForceMode.VelocityChange);
    }

    void OnSlamInputRecieved(InputAction.CallbackContext context)
    {
        // Handle slam input
        if (context.performed)
        {
            if (!_onGround)
                Slam();
            else
                Slide();
        }
    }

    void Slam() 
    {
        _rigidbody.velocity = Vector3.zero;
        _rigidbody.AddForce(Vector3.down * SlamForce);
        _isSlamming = true;
    }

    void Slide() 
    {
        // bring the player closer to the ground and bump move speed

    }

    void OnTeleportInputRecieved(InputAction.CallbackContext context)
    {
        if (_teleportTimer >= 0f)
        {
            // give audio/visual indicator that teleport is on cooldown with ui element flash
            // perhaps a vignette as well
            return;
        }
        // Handle teleport input
        if (context.performed)
        {
            Teleport();
            // on teleport event could be called here, not sure what would be done though
        }
    }

    // teleport through the object, if the point will be inside the object, teleport to the nearest point on the outside of the object
    // NOTE: FOR VFX, THE Object could be made transparent while the player is trying to teleport and to point at which they are teleporting to is shown
    // The teleport will need to be changed to a press and hold system, probably using tap, on start reduce the timescale and release, bring back to 1
    void Teleport()
    {

        _teleportTimer = TeleportCooldown;
        _isSlamming = false;

        Vector3 teleportPosition = Vector3.zero;
        teleportPosition = transform.position + _cameraTransform.forward * TeleportDistance;

        #region Teleport Positioning
        //do not teleport if object cannot be teleported through
        RaycastHit hit;
        GameObject objectHit = null;
        Collider objectCollider = null;
        // if it hits something, check if it can be teleported through
        if (Physics.Raycast(transform.position, _cameraTransform.forward, out hit, TeleportDistance, LayerMask.GetMask("Environment"), QueryTriggerInteraction.Ignore))
        {
            objectHit = hit.collider.gameObject;
            objectCollider = objectHit.GetComponent<Collider>();
        }

        if (objectHit) 
        {
            // check if the player can teleport through it or not
            // if the tag is not teleportable, teleport in front of it
            if (objectHit.CompareTag("Teleportable") == false)
            {
                // add radius if collision is from the side, add height if above or below
                float dotproduct = Vector3.Dot(hit.normal, Vector3.up);
                // if the object is above or below the player, teleport to the point of impact
                if (dotproduct > 0.5f || dotproduct < -0.5f)
                    teleportPosition = hit.point + hit.normal * _collider.height / 2f;
                // if the object is to the side of the player, teleport to the point of impact
                else
                    // add the radius to the teleport position so the player is not inside the object
                    teleportPosition = hit.point + hit.normal * _collider.radius;
                    




            }
            // is tagged teleportable
            else 
            {
                // teleport to the outside of the object including the player collider radius so the player is not inside the object
                if (objectCollider.bounds.Contains(teleportPosition)) 
                {
                    Vector3 direction = Vector3.zero;
                    float distance = 0f;

                    // finds the nearest point on the outside of the collider and how far away it is, if those are found the colliders are seperated
                    Physics.ComputePenetration(_collider, teleportPosition, transform.rotation, objectCollider,
                        objectCollider.transform.position, objectCollider.transform.rotation, out direction, out distance);
                    
                    teleportPosition += direction * distance;
                    
                    //// find the closest point on the outside of the collider
                    //Vector3 newPos = objectCollider.ClosestPoint(teleportPosition);
                    //teleportPosition = newPos;
                }
            }
        }
        #endregion

        transform.position = teleportPosition;



        #region Velocity

        // set the velocity to be in the direction of the camera, reduce the velocity by a factor if the player is moving downwards
        float dot = Vector3.Dot(_rigidbody.velocity.normalized, Vector3.down);
        if (dot > _downTeleportReductionThreshold)
            _rigidbody.velocity = _cameraTransform.forward * _rigidbody.velocity.magnitude * DownwardTeleportVelocityReduction;
        else
            _rigidbody.velocity = _cameraTransform.forward * _rigidbody.velocity.magnitude;
        #endregion

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
                    _isSlamming = false;
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
                    _isSlamming = false;
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


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + _cameraTransform.forward * 5f);
        Gizmos.color = Color.blue;
        Vector3 dir = _cameraTransform.forward * _rigidbody.velocity.magnitude;
        Gizmos.DrawLine(transform.position, transform.position + dir.normalized * 7.5f);
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + _rigidbody.velocity.normalized * 10f);
        Gizmos.color = Color.yellow;
        Vector3 teleportPosition = Vector3.zero;
        teleportPosition = transform.position + _cameraTransform.forward * TeleportDistance;
        Gizmos.DrawSphere(teleportPosition, 0.1f);
    }

}
