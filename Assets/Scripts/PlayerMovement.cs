using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cinemachine;

public class PlayerMovement : MonoBehaviour
{

    [SerializeField] private CharacterController _characterController;

    //movement related fields
    [SerializeField] private float _moveSpeed = 15f;
    [SerializeField] private float _sprintSpeed = 30f;
    [SerializeField] private float _currentSpeed = 5f;
    [SerializeField] private float _boostSpeed = 90f;
    [SerializeField] private float _airDashSpeed = 40f;
    [SerializeField] private float _jumpSpeedClamp = 60f;
    [SerializeField] private float _jumpForce = 1f;
    [SerializeField] private float _gravity = 2.2f;
    [SerializeField] private float _normalDecceleration = 40f;
    [SerializeField] private float _acceleration = 5f;
    [SerializeField] private float _boostDecceleration = 70f;
    [SerializeField] private float _directionalInfluence = 0.3f;
    [SerializeField] private float _airDashLength = 0.01f;

    [SerializeField] private Vector3 _forward;

    [SerializeField] private Vector3 _moveDirection = Vector3.zero;
    private Vector3 _inputDirection = Vector3.zero;
    private Vector3 _transformDirection = Vector3.zero;
    private Vector3 _airDashDirection = Vector3.zero;

    private bool _playerIsMoving;
    private bool _playerJumped;

    private ControlState _playerControlState = ControlState.Standard;

    private bool _doubleJumpAvailable = true;
    private bool _airDashAvailable = true;

    //animation related fields?
    private Vector3 _viewingVector;
    private Transform _playerModel;

    private Animator _animator;
    private PlayerInput _playerInput;

    private TrailRenderer _trail;
    private ParticleSystem _ring;

    private float _dashTime = 0f;

    private enum ControlState
    {
        Standard,
        SP01_AirDash
    }


    private void Awake()
    {
        _playerInput = new PlayerInput();

        _currentSpeed = _moveSpeed;
        _playerModel = transform.GetChild(0);
        _characterController = GetComponent<CharacterController>();
        _animator = _playerModel.GetComponentInChildren<Animator>();

        _forward = _playerModel.forward;
        _playerControlState = ControlState.Standard;

        //placeholder
        _trail = transform.GetChild(1).GetChild(0).GetComponent<TrailRenderer>();
        _ring = transform.GetChild(1).GetChild(1).GetComponent<ParticleSystem>();
    }


    private void OnEnable()
    {
        //enable player controls
        _playerInput.CharacterControls.Move.started += MoveHandler;
        _playerInput.CharacterControls.Move.performed += MoveHandler;
        _playerInput.CharacterControls.Move.canceled += MoveHandler;


        _playerInput.CharacterControls.Jump.started += JumpHandler;

        _playerInput.CharacterControls.Sprint.started += SprintHandler;

        _playerInput.CharacterControls.Sprint.started += AirDashHandler;

        _playerInput.CharacterControls.ReloadCurrentScene.canceled += ReloadHandler;
        _playerInput.CharacterControls.Enable();

    }
    private void OnDisable()
    {
        //disable player controls
        _playerInput.CharacterControls.Move.started -= MoveHandler;
        _playerInput.CharacterControls.Move.performed -= MoveHandler;
        _playerInput.CharacterControls.Move.canceled -= MoveHandler;


        _playerInput.CharacterControls.Jump.started -= JumpHandler;

        _playerInput.CharacterControls.Sprint.started -= SprintHandler;

        _playerInput.CharacterControls.ReloadCurrentScene.canceled -= ReloadHandler;
        _playerInput.CharacterControls.Disable();

        StopAllCoroutines();
    }

    //update methods
    private void Update()
    {

        _transformDirection = Camera.main.transform.TransformDirection(_inputDirection);
        HandleAnimation();

        //debug
        //Debug.Log(ProcessInputs());

        //placeholder
        _trail.time = ((_currentSpeed) * (0.2f / _boostSpeed));

    }
    private void FixedUpdate()
    {
        ProcessAcceleration();
        ProcessForwardDirection();
        if (_playerControlState == ControlState.Standard)
        {
            _dashTime = 0f;
            MoveCharacter();
        }
        else if (_playerControlState == ControlState.SP01_AirDash)
        {
            AirDash();
        }
    }

    //inputhandlers
    private void MoveHandler(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        //setting the bool values for _playerIsmoving which is meant to return true while the player is inputing movement
        if (context.started || context.performed)
        {
            _playerIsMoving = true;
        }
        if (context.canceled)
        {
            _playerIsMoving = false;
        }

        //sets player input to _inputdirection vector3 to use later
        _inputDirection = new Vector3(context.ReadValue<Vector2>().x, 0f, context.ReadValue<Vector2>().y);
    }
    private void SprintHandler(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {

        //sets current speed to boost speed when sprint button is pressed while below certain speed threshold and grounded
        if (context.started && _characterController.isGrounded)
        {
            if (_currentSpeed < _sprintSpeed + 5f)
            {
                _ring.Play();
                _currentSpeed = _boostSpeed;
            }
        }
    }
    private void AirDashHandler(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if (_characterController.isGrounded == false && _airDashAvailable)
        {
            Debug.Log("howdy");
            if (context.started)
            {
                if (_playerIsMoving == true)
                {
                    _airDashDirection = _transformDirection; 
                }
                else
                {
                    _airDashDirection = _playerModel.forward;
                }
            }
            _ring.Play();
            _playerControlState = ControlState.SP01_AirDash;
            _airDashAvailable = false;
        }
    }
    private void JumpHandler(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        //if player presses jump button while grounded
        if (_characterController.isGrounded && context.started == true)
        {
            _playerJumped = true;
        }

        else if (_doubleJumpAvailable && _characterController.isGrounded == false && context.started == true)
        {
            _ring.Play();
            _playerJumped = true;
            _doubleJumpAvailable = false;
        }
    }
    private void ReloadHandler(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }


    //movement/animation handlers
    private void HandleAnimation()
    {
        //placeholder as af

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

    //composite method
    private void MoveCharacter()
    {
        ProcessMoveDirection();
        ProcessJump();
        ProcessCharacterModelRotation();

        _characterController.Move(_moveDirection);
    }

    //methods responsible for directional movement
    //sets the x and z values for _movementDirection
    private void ProcessMoveDirection()
    {
        //set to zero just in case
        Vector3 groundMovement = Vector3.zero;

        //if you have speed it does this
        if (_currentSpeed > _moveSpeed)
        {
            groundMovement = _currentSpeed * Time.deltaTime * ProcessInputs();
        }
        else if (_characterController.isGrounded == false)
        {
            groundMovement = _currentSpeed * Time.deltaTime * ProcessInputs();
        }
        //if you dont it stops basically the only time you'll call this is if you dont move
        else
        {
            groundMovement = _currentSpeed * Time.deltaTime * _transformDirection;
        }

        _moveDirection = new Vector3(groundMovement.x, _moveDirection.y, groundMovement.z);
    }
    private Vector3 ProcessInputs()
    {
        if (_inputDirection == Vector3.zero)
        {
            return Vector3.Slerp(_forward, _forward, _directionalInfluence);
        }
        return Vector3.Slerp(_forward, _transformDirection, _directionalInfluence);
    }
    private void ProcessForwardDirection()
    {
        if (_characterController.velocity != Vector3.zero)
        {
            _forward = Vector3.Normalize(_characterController.velocity);
        }
        else if (_characterController.velocity == Vector3.zero)
        {
            _forward = _playerModel.forward;
        }

        _forward.y = 0f;
    }
    private void ProcessCharacterModelRotation()
    {
        _viewingVector = new Vector3(_characterController.velocity.x, 0f, _characterController.velocity.z);

        if (_viewingVector != Vector3.zero)
        {
            _playerModel.transform.rotation = Quaternion.LookRotation(_viewingVector, Vector3.up);

            //placeholder
            _ring.transform.rotation = Quaternion.LookRotation(_characterController.velocity);
        }
    }

    private void ProcessJump()
    {
        if (_playerJumped)
        {
            // if speed is above jumpspeedclamp value clamp it down
            // made it to make boost jump a bit less crazy
            if (_currentSpeed > _jumpSpeedClamp && _inputDirection != Vector3.zero)
            {
                _currentSpeed = _jumpSpeedClamp;
            }
            _moveDirection.y = _jumpForce;
            _playerJumped = false;
        }

        //applies gravity while airborne
        else if (_characterController.isGrounded == false)
        {
            _moveDirection.y -= _gravity * Time.deltaTime;
        }

        else if (_characterController.isGrounded)
        {
            _doubleJumpAvailable = true;
            _airDashAvailable = true;
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
        //decceleration when _playerisMoving == false // no movement input
        if (_playerIsMoving == false && _currentSpeed >= _sprintSpeed)
        {
            _currentSpeed -= _boostDecceleration * Time.deltaTime;
        }
        if (_playerIsMoving == false && _moveSpeed < _currentSpeed && _currentSpeed < _sprintSpeed)
        {
            //adjust with some kind of formula idk
            _currentSpeed -= _normalDecceleration * Time.deltaTime;
        }
        if (_currentSpeed < _moveSpeed)
        {
            _currentSpeed = _moveSpeed;
        }
    }

    //special actions

    private void AirDash()
    {
        _airDashDirection.y = 0f;
        if (_dashTime >= _airDashLength)
        {
            _playerControlState = ControlState.Standard;
        }
        _moveDirection = _airDashDirection * _airDashSpeed * Time.deltaTime;
        _dashTime += Time.deltaTime;

        _characterController.Move(_moveDirection);
    }

}
