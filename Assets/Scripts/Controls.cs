using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody)), RequireComponent(typeof(PlayerInput))]
public class Controls : MonoBehaviour
{
    [SerializeField] Rigidbody _rigidbody;
    public float MoveSpeed = 5f;
    Vector2 _movementDirection = new Vector2();
    InputAction _aimAction;
    InputAction _forwardAction;
    InputAction _backwardAction;
    InputAction _leftAction;
    InputAction _rightAction;
    InputAction _jumpAction;
    InputAction _dashAction;
    // Start is called before the first frame update
    void Start()
    {
        if(!_rigidbody)
            _rigidbody = GetComponent<Rigidbody>();
        
        var actionMap = GetComponent<PlayerInput>().currentActionMap;

        _aimAction = actionMap["Aim"];
        _forwardAction = actionMap["Forward"];
        _backwardAction = actionMap["Backward"];
        _leftAction = actionMap["Left"];
        _rightAction = actionMap["Right"];
        _jumpAction = actionMap["Jump"];
        _dashAction = actionMap["Dash"];

        _aimAction.started += ctx => AimActionRecieved();
        _forwardAction.started += ctx => ForwardActionRecieved();
        _backwardAction.started += ctx => BackwardActionRecieved();
        _leftAction.started += ctx => LeftActionRecieved();
        _rightAction.started += ctx => RightActionRecieved();
        _jumpAction.started += ctx => JumpActionRecieved();
        _dashAction.started += ctx => DashActionRecieved();

        _aimAction.canceled += ctx => AimActionRecieved();
        _forwardAction.canceled += ctx => ForwardActionEnded();
        _backwardAction.canceled += ctx => BackwardActionEnded();
        _leftAction.canceled += ctx => LeftActionEnded();
        _rightAction.canceled += ctx => RightActionEnded();
    }


    private void OnEnable()
    {
        
    }

    void AimActionRecieved() 
    {
        
    }

    void ForwardActionRecieved() 
    {
        _movementDirection.x = 1;

    }

    void BackwardActionRecieved() 
    {
        _movementDirection.x = -1;
    }

    void LeftActionRecieved() 
    {
        _movementDirection.y = 1;
        
    }
    void RightActionRecieved() 
    {
        _movementDirection.y = -1;   
    }

    void ForwardActionEnded()
    {
        _movementDirection.x = 0;

    }

    void BackwardActionEnded()
    {
        _movementDirection.x = 0;
    }

    void LeftActionEnded()
    {
        _movementDirection.y = 0;

    }
    void RightActionEnded()
    {
        _movementDirection.y = 0;
    }

    void JumpActionRecieved() 
    {

        
    }
    void DashActionRecieved() 
    {
        
    }
    private void OnDisable()
    {
        _aimAction.started -= ctx => AimActionRecieved();
        _forwardAction.started -= ctx => ForwardActionRecieved();
        _backwardAction.started -= ctx => BackwardActionRecieved();
        _leftAction.started -= ctx => LeftActionRecieved();
        _rightAction.started -= ctx => RightActionRecieved();
        _jumpAction.started -= ctx => JumpActionRecieved();
        _dashAction.started -= ctx => DashActionRecieved();
    }

    // Update is called once per frame
    void Update()
    {
        if(_movementDirection != Vector2.zero)
        {
            Vector3 movement = new Vector3(_movementDirection.x, 0, _movementDirection.y);
            movement.Normalize();
            _rigidbody.AddForce(movement * MoveSpeed, ForceMode.Impulse);
        }
        
    }
}
