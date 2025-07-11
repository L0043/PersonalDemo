using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.HID;
using UnityEngine.UI;

// TO FIX:
/*
 * infinte corner jump - Fixed
 * fix bump launching
 * limit dashes?
 * sliding into jump is inconsistent
 * velocity gets lost constantly, improve the retention
*/
// If the player jumps within 0.5 seconds of touching the ground, they retain their velocity
[RequireComponent(typeof(Rigidbody)), RequireComponent(typeof(PlayerInput))]
public class Controls : MonoBehaviour
{
    [SerializeField] Rigidbody _rigidbody;
    [SerializeField] CapsuleCollider _collider;
    public float MoveSpeed = 5f;
    public float AirMoveSpeed = 5f;
    public float Sensitivity = 0.1f;

    [Space]
    [Header("Jump")]
    public float JumpForce = 5000f;
    [SerializeField] float _wallAngleLimit = 30f;
    float _groundAngleLimit = 0f;
    GameObject _lastTouchedWall = null;
    bool _onWall = false;
    [Tooltip("The maximum angle the object can be at on the x or z axis to be considered a wall by the player")]
    bool _wallJumpAvailable = false;

    [Space]
    [Header("Dash")]
    public float DashForce = 10f;
    //DashCooldown is the time the player has to wait before being allowed to dash again
    public float DashCooldown { get; private set; } = 1f;
    float _dashCooldownTimer = 0.0f;
    // DashTime is the time in the players inputs will be ignored for to allow for dashing on the ground
    // and in the air, this is also the time the player will be dashing for, this can be used for invulnerability frames if enemies are added
    // The reason for its existance is due to the on ground movement constantly setting the players velocity to allow for snappy movement on the ground.
    // this however, prevented the dash from working on the ground as the velocity was constantly being set to 0.
    public float DashTime = 2f;
    float _dashTimer = 0.0f;
    bool _isDashing = false;

    [Space]
    [Header("Slam")]
    public float SlamForce = 5000f;
    bool _isSlamming = false;

    [Space]
    [Header("Slide")]
    public float SlideSpeedMultiplier = 1.25f;
    Vector3 _slideDirection = new Vector3();
    bool _isSliding = false;

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
    Vector3 _wallNormal = new Vector3();
    Vector3 _groundNormal = new Vector3();
    Vector3 _retainedVelocity = new Vector3();
    Vector3 _storedPosition = new Vector3();

    InputAction _aimAction;
    InputAction _moveAction;
    InputAction _jumpAction;
    InputAction _wallRunAction;
    InputAction _dashAction;
    InputAction _slamAction;
    InputAction _teleportAction;

    float _viewPitch = 0.0f;
    float _viewYaw = 0.0f;
    float _positionStoreTimer = 0.0f;
    [SerializeField] float _positionStoreTime = 0.1f;
    bool _onGround = false;
    bool _isWallRunning = false;
    [SerializeField] LayerMask _groundLayerMask = new LayerMask();


    // Start is called before the first frame update
    void Start()
    {
        //lock mouse to game window
        Cursor.lockState = CursorLockMode.Locked;
        _viewYaw = transform.rotation.eulerAngles.y;

        if (!_rigidbody)
            _rigidbody = GetComponent<Rigidbody>();
        if(!_collider)
            _collider = GetComponent<CapsuleCollider>();

        var inputMap = GetComponent<PlayerInput>().currentActionMap;
        _moveAction = inputMap.FindAction("Move");
        _aimAction = inputMap.FindAction("Look");
        _jumpAction = inputMap.FindAction("Jump");
        _wallRunAction = inputMap.FindAction("Wall Run");
        _dashAction = inputMap.FindAction("Dash");
        _slamAction = inputMap.FindAction("Slam");
        _teleportAction = inputMap.FindAction("Teleport");

        _moveAction.started += OnMoveInputRecieved;
        _moveAction.performed += OnMoveInputRecieved;
        _moveAction.canceled += OnMoveInputRecieved;
        _aimAction.performed += OnAimInputRecieved;
        _aimAction.canceled += OnAimInputRecieved;
        _jumpAction.performed += OnJumpInputRecieved;
        _wallRunAction.performed += OnWallRunInputRecieved;
        _wallRunAction.canceled += OnWallRunInputRecieved;
        _dashAction.performed += OnDashInputRecieved;
        _slamAction.started += OnSlamInputRecieved;
        _slamAction.performed += OnSlamInputRecieved;
        _slamAction.canceled += OnSlamInputRecieved;
        _teleportAction.performed += OnTeleportInputRecieved;

        _groundAngleLimit = 90 - _wallAngleLimit;
        GetComponent<PlayerInput>().controlsChangedEvent.AddListener(NotifyGameManager);
    }

    void NotifyGameManager(PlayerInput playerInput) 
    {
        int aj = 5;
        aj += 12;
    }
   
    void FixedUpdate()
    {
        // MOVEMENT
        _wantedDir = transform.forward * _movementDirection.y + transform.right * _movementDirection.x;

        if (_onGround)
        {
            if (!_isDashing && !_isSliding)
            {
                Vector3 vel = _wantedDir * MoveSpeed * Time.fixedDeltaTime;
                vel.y = _rigidbody.velocity.y;
                _rigidbody.velocity = vel;
            }
            if (_isSliding) 
            {
                _slideDirection = Vector3.ProjectOnPlane(_slideDirection, _groundNormal);
                Vector3 vel = _slideDirection * MoveSpeed * SlideSpeedMultiplier * Time.fixedDeltaTime;
                vel.y = _rigidbody.velocity.y;
                _rigidbody.velocity = vel;
            }

        }
        else if(!_onGround && !_isWallRunning && !_isSlamming && !_isSliding)
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
        else if (_isWallRunning) 
        {
            // suggestion from ripleys aquarium: add modifiers to wall running velocity retention based on the angle the player touches the wall at
            // the closer to parralel they are, the more velocity they retain
            // my idea: make the wall running only happen if the player is holding a button (perhaps space?)
            // Add a pendulum grappling hook please
            // 
            if (!_isDashing && !_isSliding) 
            {
                Vector3 newDirection = Vector3.ProjectOnPlane(_wantedDir, _wallNormal);
                newDirection *= MoveSpeed * Time.fixedDeltaTime;
                newDirection += _retainedVelocity;
                newDirection.y = _rigidbody.velocity.y;
                // on the wall the player should not fall and the y velocity should be 0, keep the current x and z velocity
                _rigidbody.velocity = newDirection;
            }
        }
    }

    private void Update()
    {
        if(transform.position.y <= -500f)
            GameManager.Instance.ResetPlayerPosition();

        // CAMERA
        _viewPitch = Mathf.Clamp(_viewPitch - _lookDirection.y, -89.0f, 89.0f);
        _viewYaw += _lookDirection.x;

        if(_viewYaw >= 360f)
            _viewYaw -= 360f;
        else if (_viewYaw < 0f)
            _viewYaw += 360f;

        //rotate around
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, _viewYaw, transform.rotation.eulerAngles.z);
        // aim up and down
        _cameraTransform.localRotation = Quaternion.Euler(_viewPitch, 0f, 0f);

        if(_teleportTimer >= 0.0f)
            _teleportTimer -= Time.deltaTime;
        if(_dashCooldownTimer >= 0f)
            _dashCooldownTimer -= Time.deltaTime;


        // allow immediate movement after slamming
        if (_moveAction.IsInProgress() && !_isSlamming) 
        {
            // Call the moveinput received function
            _movementDirection = _moveAction.ReadValue<Vector2>();
        }

        if(_slamAction.IsInProgress() && _onGround && !_isSlamming && !_isSliding)
        {
            Slide();
        }

        // increase the players drag if they are not moving on a wall
        //float dot = Vector3.Dot(transform.up, _rigidbody.velocity.normalized);
        //// if velocity is downwards, set drag to 5
        //
        //if (_onWall && _movementDirection == Vector2.zero && dot < -0.8f)
        //{
        //    _rigidbody.drag = 5f;
        //}
        //else
        //{
        //    _rigidbody.drag = 0f;
        //}

        if (_isDashing) 
        {
            _dashTimer -= Time.deltaTime;
            if (_dashTimer <= 0f) 
            { 
                _isDashing = false;
            }
        }
        if (_onGround)
        {
            _positionStoreTimer += Time.deltaTime;
            if (_positionStoreTimer >= _positionStoreTime)
            {
                _storedPosition = transform.position;
                _positionStoreTimer = 0f;
            }
        }
        else
            _positionStoreTimer = 0.0f;

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
        if (!_onGround && !_onWall)
            return;
        if (_onWall && !_wallJumpAvailable && !_onGround)
            return;

        // Handle jump input
        if (context.performed)
        {
            Jump();
        }
    }
    // TODO : Make Wall Running a hold interaction seperate from jumping it will toggle when the player is on a wall and holding space
    // if the player is not moving their drag is increased and they start to fall down they will be able to move forward only,
    // when the player releases space their drag is reset and they will no longer have 0 y velocity.
    // when they tap space they will jump, if they tap space after releasing space they will jump off the wall and not begin to wall run
    void Jump() 
    {
        
        if (_onWall && !_onGround)
        {
            _wallJumpAvailable = false;
            _isWallRunning = false;
            float dot = Vector3.Dot(_cameraTransform.forward, transform.up);
            //force the drag to be 0 while the player is jumping so their jump is not affected
            _rigidbody.drag = 0f;

            if(_rigidbody.velocity.y < 0f) 
            {
                Vector3 zeroedVel = _rigidbody.velocity;
                zeroedVel.y = 0f;
                _rigidbody.velocity = zeroedVel;
            }
            _rigidbody.AddForce(Vector3.up * JumpForce * 1.25f);
            // if the camera is looking 45 degrees >= above the horizontal plane, jump in the direction of the camera
            //if (dot >= 0.7f)
            //{
            //    _rigidbody.AddForce(_cameraTransform.forward * JumpForce * 1.25f);
            //}
            //else
            //{
            //    _rigidbody.AddForce(Vector3.up * JumpForce * 1.25f);
            //}
        }
        else
        {
            _rigidbody.velocity = new Vector3(_rigidbody.velocity.x, 0f, _rigidbody.velocity.z);
            _rigidbody.AddForce(Vector3.up * JumpForce);
        }
    }

    void OnWallRunInputRecieved(InputAction.CallbackContext context) 
    {
        if (context.performed) 
        {
            // start checking for wall run
            
        }
        else if (context.canceled) 
        {
            // end checking for wall run
            _rigidbody.drag = 0f;
            _isWallRunning = false;
        }
    }

    void OnDashInputRecieved(InputAction.CallbackContext context)
    {
        if (_dashCooldownTimer >= 0f)
        { 
            return;
        }

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

        float dashForce = DashForce;

        if (_wantedDir != Vector3.zero)
            _rigidbody.AddForce(_wantedDir * dashForce, ForceMode.Impulse);
        else
            _rigidbody.AddForce(transform.forward * dashForce, ForceMode.Impulse);
        _dashCooldownTimer = DashCooldown;
        _isDashing = true;
        _dashTimer = DashTime;

        EventManager.OnDash.Invoke();
    }

    void OnSlamInputRecieved(InputAction.CallbackContext context)
    {
        // Handle slam and slide input
        // if the player is not on the ground, allow them to slam, if on ground the player slides
        // if the slide ends in the air, do not slam


        if (context.performed || context.started)
        {
            if (!_onGround && !_isSliding && !_isSlamming)
                Slam();
            else if(!_isSliding && _onGround)
                Slide();
        }
        else if (context.canceled)
        {
            if(_isSliding)
                SlideEnd();
        }
    }

    void Slam() 
    {
        _rigidbody.velocity = Vector3.zero;
        _rigidbody.AddForce(Vector3.down * SlamForce);
        _isSlamming = true;
    }

    void SlideEnd() 
    {
        _isSliding = false;
        transform.localRotation = Quaternion.Euler(0f, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
        _viewPitch -= 85.75f;
    }

    void Slide() 
    {
        _slideDirection = Vector3.zero;
        if (_movementDirection == Vector2.zero)
            _slideDirection = Vector3.Project(_cameraTransform.forward, transform.forward);
        else
            _slideDirection = _wantedDir.normalized;


        _slideDirection.Normalize();
        _slideDirection = Vector3.ProjectOnPlane(_slideDirection, _groundNormal);
        // bring the player closer to the ground and bump move speed
        _rigidbody.velocity = _slideDirection * MoveSpeed * SlideSpeedMultiplier * Time.fixedDeltaTime;
        _isSliding = true;

        //rotate the player to be closer to the ground based on the forward direction the body is facing
        transform.localRotation = Quaternion.Euler(-85.75f, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
        _viewPitch += 85.75f;
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
    // ADDITIONAL NOTE: If the player teleports through a wall and the point they are meant to go to is on the other side of a non teleportable wall, they will reach that point
    // this will cause the player to be able to clip out of bounds extremely easily, this will need to be fixed.
    void Teleport()
    {
        EventManager.OnTeleport.Invoke();
        _teleportTimer = TeleportCooldown;
        _isSlamming = false;

        Vector3 teleportPosition = Vector3.zero;
        teleportPosition = transform.position + _cameraTransform.forward * TeleportDistance;

        #region Teleport Positioning
        //do not teleport if object cannot be teleported through
        
        // if it hits something, check if it can be teleported through

        // ray cast needs to collect all objects that it will hit,
        // if the first object is teleportable, go through it, if the second object is not teleportable, teleport to the point of impact
        var mask = LayerMask.GetMask("Environment", "Default");
        var hits = Physics.RaycastAll(transform.position, _cameraTransform.forward, TeleportDistance, mask, QueryTriggerInteraction.Ignore);

        // I have a hunch that the optimal way to do this would be having the teleportable objects on a seperate layer
        // this would allow for me to check the layer bits rather than a string comparison. However, this would cause issues elsewhere
        // as the teleportable objects would not be registered as environment objects. damn that sucks
        if (hits.Length <= 0) 
        {
            // no hits, teleport to the point of impact
            transform.position = teleportPosition;
        }
        else 
        {


            // this will loop through all the objects collected by the raycast
            // if it finds an object that is not teleportable the loop will break and the player will be teleported to the point of impact + an offset based on their size
            // if the object is teleportable and the position is not inside the object we continue through the list
            // if the object is teleportable and the position is inside the object the loop will
            // break and the player will be teleported to the point of impact + an offset based on their size
            // granted this object should be the last object in the list, the loop break is their as a safety net

            // Note: I had originally intended for the loop to continue even if the object contains the player position
            // but thought that this could be used to make level creation easier, as I could have a solid wall with a silver of teleportable object in it
            foreach (RaycastHit hit in hits) 
            {
                // if the first object contains the player pos, it ends the loop and the player can phase through things


                bool isTeleportable = hit.collider.CompareTag("Teleportable");
                bool contains = hit.collider.bounds.Contains(teleportPosition);
                // if the object is teleportable and the position is not inside the object we continue through the list
                if (isTeleportable && contains == false)
                    continue;
                if(!isTeleportable) 
                {
                    // add radius if collision is from the side, add height if above or below
                    float dotproduct = Vector3.Dot(hit.normal, Vector3.up);
                    // if the object is above or below the player, teleport to the point of impact adding the player height
                    if (dotproduct > 0.5f || dotproduct < -0.5f)
                        teleportPosition = hit.point + hit.normal * _collider.height / 2f;
                    // if the object is to the side of the player, teleport to the point of impact adding the player "width"
                    else
                        // add the radius to the teleport position so the player is not inside the object
                        teleportPosition = hit.point + hit.normal * _collider.radius;
                    break;
                }
                if(isTeleportable && contains)
                {
                    Vector3 direction = Vector3.zero;
                    float distance = 0f;

                    // finds the nearest point on the outside of the collider and how far away it is, if those are found the colliders are seperated
                    Physics.ComputePenetration(_collider, teleportPosition, transform.rotation, hit.collider,
                        hit.collider.transform.position, hit.collider.transform.rotation, out direction, out distance);

                    teleportPosition += direction * distance;

                    break;
                }

            }

            transform.position = teleportPosition;

        }

        //if (Physics.Raycast(transform.position, _cameraTransform.forward, out hit, TeleportDistance, mask, QueryTriggerInteraction.Ignore))
        //{
        //    objectHit = hit.collider.gameObject;
        //    objectCollider = hit.collider;
        //}
        //
        //if (objectHit) 
        //{
        //    // check if the player can teleport through it or not
        //    // if the tag is not teleportable, teleport in front of it
        //    if (objectHit.CompareTag("Teleportable") == false)
        //    {
        //        // add radius if collision is from the side, add height if above or below
        //        float dotproduct = Vector3.Dot(hit.normal, Vector3.up);
        //        // if the object is above or below the player, teleport to the point of impact adding the player height
        //        if (dotproduct > 0.5f || dotproduct < -0.5f)
        //            teleportPosition = hit.point + hit.normal * _collider.height / 2f;
        //        // if the object is to the side of the player, teleport to the point of impact adding the player "width"
        //        else
        //            // add the radius to the teleport position so the player is not inside the object
        //            teleportPosition = hit.point + hit.normal * _collider.radius;
        //
        //    }
        //    // is tagged teleportable
        //    else 
        //    {
        //        // teleport to the outside of the object including the player collider radius so the player is not inside the object
        //        if (objectCollider.bounds.Contains(teleportPosition)) 
        //        {
        //            Vector3 direction = Vector3.zero;
        //            float distance = 0f;
        //
        //            // finds the nearest point on the outside of the collider and how far away it is, if those are found the colliders are seperated
        //            Physics.ComputePenetration(_collider, teleportPosition, transform.rotation, objectCollider,
        //                objectCollider.transform.position, objectCollider.transform.rotation, out direction, out distance);
        //            
        //            teleportPosition += direction * distance;
        //            
        //            //// find the closest point on the outside of the collider
        //            //Vector3 newPos = objectCollider.ClosestPoint(teleportPosition);
        //            //teleportPosition = newPos;
        //        }
        //    }
        //}
        #endregion

        transform.position = teleportPosition;



        #region Velocity

        // set the velocity to be in the direction of the camera, reduce the velocity by a factor if the player is moving downwards
        float dot = Vector3.Dot(_rigidbody.velocity.normalized, Vector3.down);
        float radians = Mathf.Cos(_downTeleportReductionThreshold * Mathf.Deg2Rad);
        // convert radians to degrees
        float degrees = Mathf.Acos(radians) * Mathf.Rad2Deg;

        if (dot > radians)
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
                float radians = Mathf.Cos(_groundAngleLimit * Mathf.Deg2Rad);
                if (Vector3.Dot(contact.normal, Vector3.up) > radians)
                {
                    _onGround = true;
                    _isSlamming = false;
                    _groundNormal = contact.normal;
                    _retainedVelocity = _rigidbody.velocity;
                }
                // check if the object is perpendicular to the player
                if (WallCheck(_wallAngleLimit, contact.normal))
                {
                    BoxCollider box = collision.collider as BoxCollider;
                    if (!box)
                        return;
                    _onWall = true;
                    _retainedVelocity = _rigidbody.velocity;
                    if (_wallRunAction.phase == InputActionPhase.Performed && _onGround == false)
                    {
                        _isWallRunning = true;
                        _wallNormal = contact.normal;
                        _rigidbody.drag = 5f;
                    }
                    // refresh jump here unless last touched wall has changed
                    if (_lastTouchedWall != collision.gameObject)
                    // do vfx stuff here
                    {
                        _lastTouchedWall = collision.gameObject;
                        _wallJumpAvailable = true;
                        // toggle wall running here
                    }
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
                float radians = Mathf.Cos(_groundAngleLimit * Mathf.Deg2Rad);
                if (Vector3.Dot(contact.normal, Vector3.up) > radians)
                {
                    _onGround = true;
                    _groundNormal = contact.normal;
                    _isSlamming = false;
                }
                // check if the object is perpendicular to the player
                if (WallCheck(_wallAngleLimit, contact.normal))
                {
                BoxCollider box = collision.collider as BoxCollider;
                    if (!box)
                        return;

                    _onWall = true;
                    if (_isWallRunning == false)
                    {
                        if (_wallRunAction.phase == InputActionPhase.Performed && _onGround == false)
                        {
                            _isWallRunning = true;
                            _wallNormal = contact.normal;
                            _rigidbody.drag = 5f;
                        }
                    }
                    //// refresh jump here unless last touched wall has changed
                    //if (_lastTouchedWall == collision.gameObject)
                    //    // do vfx stuff here
                    //    return;
                    //_lastTouchedWall = collision.gameObject;
                    //_wallJumpAvailable = true;
                }
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.layer != LayerMask.NameToLayer("Environment"))
            return;
        
        if (_onGround)
        {
            _onGround = false;
            _isSlamming = false;
            _lastTouchedWall = null;
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        Physics.ComputePenetration(other, other.transform.position, other.transform.rotation, 
            _collider, transform.position, transform.rotation, out Vector3 direction, out float distance);
        // check if the object is perpendicular to the player
        if (WallCheck(_wallAngleLimit, direction.normalized))
        {
            BoxCollider box = other as BoxCollider;
            if (!box)
                return;

            // refresh jump here unless last touched wall has changed
            if (_lastTouchedWall == other.gameObject)
                // do vfx stuff here
                return;
            _lastTouchedWall = other.gameObject;
            _wallJumpAvailable = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (_onWall)
        {
            _onWall = false;
            _isWallRunning = false;
            _wallNormal = Vector3.zero;
        }
    }

    bool WallCheck(float degreesLimit, Vector3 normal) 
    {
        // take the degreesLimit and convert it to a value between -1 and 1
        float radians = Mathf.Cos(degreesLimit * Mathf.Deg2Rad);
         
        float dot = Vector3.Dot(normal, Vector3.up);
        if (dot <= radians && dot >= -radians)
            return true;

        return false;
    }

    public void StopSlam() 
    {
        _isSlamming = false;
    }

    public void ResetPosition() 
    {
        _rigidbody.velocity = Vector3.zero;
        transform.position = _storedPosition;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Vector3 teleportPosition = transform.position + _cameraTransform.forward * TeleportDistance;
        Gizmos.DrawSphere(teleportPosition, 0.1f);

    }

}
