using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{

    [SerializeField] private CharacterController _characterController;

    //movement related fields
    [SerializeField] private float _moveSpeed = 15f;
    [SerializeField] private float _sprintSpeed = 30f;
    [SerializeField] private float _currentSpeed = 5f;
    [SerializeField] private float _boostSpeed = 90f;
    [SerializeField] private float _jumpSpeed = 1f;
    [SerializeField] private float _gravity = 2.2f;
    [SerializeField] private float _normalDecceleration = 40f;
    [SerializeField] private float _acceleration = 5f;
    [SerializeField] private float _boostDecceleration = 70f;
    [SerializeField] private float _inertia = 0.3f;

    [SerializeField] private Vector3 _forward;


    [SerializeField] private Vector3 _moveDirection = Vector3.zero;
    private Vector3 _inputDirection = Vector3.zero;


    private bool _playerIsMoving;
    private bool _playerJumped;

    //animation related fields?
    private Vector3 _viewingVector;
    private Transform _playerModel;

    private Animator _animator;
    private PlayerInput _playerInput;


    private TrailRenderer _trail;
    private ParticleSystem _ring;


    private void Awake()
    {
        _playerInput = new PlayerInput();

        _currentSpeed = _moveSpeed;
        _playerModel = transform.GetChild(0);
        _characterController = GetComponent<CharacterController>();
        _animator = _playerModel.GetComponentInChildren<Animator>();

        _forward = _playerModel.forward;


        //placeholder
        _trail = transform.GetChild(1).GetChild(0).GetComponent<TrailRenderer>();
        _ring = transform.GetChild(1).GetChild(1).GetComponent<ParticleSystem>();
    }
    private void OnEnable()
    {
        Debug.Log("enabled");
        //enable player controls
        _playerInput.CharacterControls.Move.performed += MoveHandler;
        _playerInput.CharacterControls.Move.started += MoveHandler;
        _playerInput.CharacterControls.Move.canceled += MoveHandler;

        _playerInput.CharacterControls.Jump.started += JumpHandler;

        _playerInput.CharacterControls.Sprint.started += SprintHandler;
        _playerInput.CharacterControls.Sprint.performed += SprintHandler;
        _playerInput.CharacterControls.Sprint.canceled += SprintHandler;

        _playerInput.CharacterControls.ReloadCurrentScene.canceled += ReloadHandler;
        _playerInput.CharacterControls.Enable();

    }
    private void OnDisable()
    {
        //disable player controls
        _playerInput.CharacterControls.Move.performed -= MoveHandler;
        _playerInput.CharacterControls.Move.started -= MoveHandler;
        _playerInput.CharacterControls.Move.canceled -= MoveHandler;

        _playerInput.CharacterControls.Jump.started -= JumpHandler;

        _playerInput.CharacterControls.Sprint.started -= SprintHandler;
        _playerInput.CharacterControls.Sprint.performed -= SprintHandler;
        _playerInput.CharacterControls.Sprint.canceled -= SprintHandler;

        _playerInput.CharacterControls.ReloadCurrentScene.canceled -= ReloadHandler;
        _playerInput.CharacterControls.Disable();

        StopAllCoroutines();
    }


    //update methods
    private void Update()
    {
        HandleAnimation();

        if (_characterController.velocity != Vector3.zero)
        {
            _forward = Vector3.Normalize(_characterController.velocity);
        }

        //placeholder
        _trail.time = ((_currentSpeed) * (0.2f / _boostSpeed));

    }

    private void FixedUpdate()
    {
        ProcessAcceleration();
        MoveCharacter();
    }
    
    
    //inputhandlers
    private void MoveHandler(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if (context.started || context.performed)
        {
            _playerIsMoving = true;
        }
        if (context.canceled)
        {
            _playerIsMoving = false;
        }
        _inputDirection = new Vector3(context.ReadValue<Vector2>().x, 0f, context.ReadValue<Vector2>().y);
    }
    private void SprintHandler(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if (_currentSpeed < _sprintSpeed + 5f)
            {
                _ring.Play();
                _currentSpeed = _boostSpeed;
            }
        }
    }
    private void JumpHandler(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if (_characterController.isGrounded && context.started == true)
        {
            _playerJumped = true;
        }
    }
    private void ReloadHandler(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }


    //movement/animation handlers
    private void HandleAnimation()
    {
        bool animatorMoveBool = _animator.GetBool("isMoving");
        //bool animatorSprintBool = _animator.GetBool("isSprinting");
        if (animatorMoveBool == false && _playerIsMoving == true)
        {
            _animator.SetBool("isMoving", true);
        }
        if (animatorMoveBool == true && _playerIsMoving == false)
        {
            _animator.SetBool("isMoving", false);
        }
        //if (animatorSprintBool == false && _playerIsSprinting == true)
        //{
        //    _animator.SetBool("isSprinting", true);
        //}
        //if (animatorSprintBool == true && _playerIsSprinting == false)
        //{
        //    _animator.SetBool("isSprinting", false);
        //}
    }

    private void MoveCharacter()
    {
        ProcessMoveDirection();
        ProcessJump();
        ProcessCharacterModelRotation();

        _characterController.Move(_moveDirection);
    }
    private void ProcessMoveDirection()
    {
        //Vector3 transformDirection = transform.TransformDirection(_inputDirection);
        //Vector3 groundMovement = _currentSpeed * Time.deltaTime * transformDirection;

        //movehandler gets input direction 
        //sprint  handler manipulates speed (RE: NO thats BAD just shove it in the update method stoopid)
        // the actual movement happens
        Vector3 groundMovement = Vector3.zero;
        if (_currentSpeed >= _moveSpeed)
        {
            groundMovement = _currentSpeed * Time.deltaTime * ProcessInputs();
        }
        else
        {
            groundMovement = _currentSpeed * Time.deltaTime * _inputDirection;
        }

        _moveDirection = new Vector3(groundMovement.x, _moveDirection.y, groundMovement.z);
    }
    private void ProcessJump()
    {
        if (_playerJumped)
        {
            _moveDirection.y = _jumpSpeed;
            _playerJumped = false;
        }
        else if (_characterController.isGrounded == false)
        {
            _moveDirection.y -= _gravity * Time.deltaTime;
        }
    }
    private void ProcessCharacterModelRotation()
    {
        _viewingVector = new Vector3(_inputDirection.x, 0f, _inputDirection.z);

        if (_viewingVector != Vector3.zero)
        {
            _playerModel.transform.rotation = Quaternion.LookRotation(_viewingVector, Vector3.up);

            //placeholder
            _ring.transform.rotation = Quaternion.LookRotation(_characterController.velocity);
        }
    }
    private void ProcessAcceleration()
    {
        if (_characterController.velocity != Vector3.zero && _currentSpeed > _sprintSpeed)
        {
            // deccelerate to sprint speed if moving over sprint speed
            _currentSpeed -= _boostDecceleration * Time.deltaTime;
        }
        if (_characterController.velocity != Vector3.zero && _currentSpeed < _sprintSpeed)
        {
            // accelerate to sprint speed if moving under sprint speed
            _currentSpeed += _acceleration * Time.deltaTime;
        }
        //decceleration when not moving
        if (_playerIsMoving == false && _currentSpeed >= _sprintSpeed)
        {
            _currentSpeed -= _boostDecceleration * Time.deltaTime;
        }
        if (_playerIsMoving == false && _moveSpeed < _currentSpeed && _currentSpeed < _sprintSpeed)
        {
            //adjust with some kind of formula idk
            _currentSpeed -= _normalDecceleration * Time.deltaTime;
        }
    }
    private Vector3 ProcessInputs() 
    {
        Debug.Log(Vector3.Slerp(_forward, _inputDirection, _inertia));
        return Vector3.Slerp(_forward, _inputDirection, _inertia);
    }

}
