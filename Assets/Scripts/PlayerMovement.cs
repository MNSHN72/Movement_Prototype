using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{

    [SerializeField] private CharacterController _characterController;

    //movement related fields
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _sprintSpeed = 8f;
    [SerializeField] private float _currentSpeed = 5f;
    [SerializeField] private float _boostSpeed = 11f;
    [SerializeField] private float _jumpSpeed = 1f;
    [SerializeField] private float _gravity = .25f;
    [SerializeField] private float _boostTime = .5f;
    [SerializeField] private float _inertia = 0.01f;
    [SerializeField] private float _acceleration = 0.01f;
    [SerializeField] private float _decceleration = 0.1f;

    [SerializeField] private Vector3 _moveDirection = Vector3.zero;
    private Vector3 _inputDirection = Vector3.zero;



    private bool _playerIsMoving;
    private bool _playerJumped;
    private PlayerSpeed _playerSpeedState = PlayerSpeed.Normal;
    private enum PlayerSpeed
    {
        Normal = 0,
        Fast = 1,
    }

    //animation related fields?
    private Vector3 _viewingVector;
    private Transform _playerModel;

    private Animator _animator;

    //particle effect related fields
    [SerializeField] private Transform _particleSystemParent;
    [SerializeField] private List<ParticleSystem> _particleSystems = new List<ParticleSystem>();
    //Particle Index
    //0 Sprint particles
    //1 boost particles

    private PlayerInput _playerInput;



    //initialization
    private void Awake()
    {
        _playerInput = new PlayerInput();

        _currentSpeed = _moveSpeed;
        _playerModel = transform.GetChild(0);
        _characterController = GetComponent<CharacterController>();
        _animator = _playerModel.GetComponentInChildren<Animator>();
        CacheParticleSystems();
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
    }

    //initialization helpers
    private void CacheParticleSystems()
    {
        _particleSystemParent = transform.GetChild(1);
        foreach (Transform child in _particleSystemParent)
        {
            _particleSystems.Add(child.GetComponent<ParticleSystem>());
        }
    }

    //update methods
    private void Update()
    {
        UpdatePlayerSpeedState();
        HandleAnimation();
    }
    private void FixedUpdate()
    {
        MoveCharacter();
    }
    //inputhandlers
    private void MoveHandler(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if (_playerSpeedState == PlayerSpeed.Normal)
        {
            if (context.canceled == true)
            {
                _currentSpeed = _moveSpeed;
                _playerIsMoving = false;
            }
            if (context.started == true)
            {
                _playerIsMoving = true;
                StartCoroutine(AccelerateToSprint());
            }
            else
            {
                _inputDirection = new Vector3(context.ReadValue<Vector2>().x, 0f, context.ReadValue<Vector2>().y);
            } 
        }
        if (_playerSpeedState == PlayerSpeed.Fast)
        {
             _inputDirection = new Vector3(context.ReadValue<Vector2>().x, 0f, context.ReadValue<Vector2>().y);
            if (context.canceled == true)
            {
                _playerIsMoving = false;
            }
            else
            {
                _playerIsMoving = true;
            }
        }
    }
    private void SprintHandler(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if (_playerSpeedState == PlayerSpeed.Normal)
        {
            if (context.started == true /*&& _playerIsMoving == true*/)
            {
                _currentSpeed = _boostSpeed;
                StartCoroutine(DegradeSpeed());
            }
        }
        if (_playerSpeedState == PlayerSpeed.Fast)
        {
            if (context.performed == true && _currentSpeed < _sprintSpeed)
            {
                _currentSpeed = _sprintSpeed;
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

    //IEnumerator DegradeInput() 
    //{
    //    Debug.Log("howdy");
    //    _inputDirection = _playerModel.forward;
    //    while ( _playerIsMoving == false)
    //    {
    //        _inputDirection = Vector3.Lerp(_inputDirection, Vector3.zero, _inertia);

    //        yield return new WaitForEndOfFrame();
    //    }
    //}
    IEnumerator DegradeSpeed() 
    {
        while (_currentSpeed > _moveSpeed)
        {
            _currentSpeed -= _decceleration * Time.deltaTime;

            if (_playerSpeedState == PlayerSpeed.Normal && _playerIsMoving == false)
            {
                _currentSpeed -= _inertia * Time.deltaTime;
            }

            yield return new WaitForEndOfFrame();
        }
    }
    IEnumerator AccelerateToSprint() 
    {
        while (_currentSpeed < (_sprintSpeed - 1f) && _playerIsMoving == true)
        {
            _currentSpeed += _acceleration * Time.deltaTime;

            yield return new WaitForEndOfFrame();
        }
    }


    //movement/animation handlers
    private void HandleAnimation()
    {
        _animator.SetFloat("playerSpeed", ((_currentSpeed - 15f) / _boostSpeed));

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
        ProccessMoveDirection();
        ProccessJump();
        ProccessCharacterModelRotation();

        _characterController.Move(_moveDirection);
    }
    private void ProccessMoveDirection()
    {
        //Vector3 transformDirection = transform.TransformDirection(_inputDirection);
        //Vector3 groundMovement = _currentSpeed * Time.deltaTime * transformDirection;

        //movehandler gets input direction and sprint  handler manipulates speed

        Vector3 groundMovement = Vector3.zero;
        if (_currentSpeed > _moveSpeed)
        {
            groundMovement = _currentSpeed * Time.deltaTime * Vector3.Normalize(_playerModel.forward + _inputDirection);
        }
        else
        {
            groundMovement = _currentSpeed * Time.deltaTime * _inputDirection;
        }

        _moveDirection = new Vector3(groundMovement.x, _moveDirection.y, groundMovement.z);
    }
    private void ProccessJump()
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
    private void ProccessCharacterModelRotation()
    {
        _viewingVector = new Vector3(_characterController.velocity.x, 0f, _characterController.velocity.z);
        if (_viewingVector != Vector3.zero)
        {
            _playerModel.transform.rotation = Quaternion.LookRotation(_viewingVector, Vector3.up);
        }
    }

    //misc. methods
    private void UpdatePlayerSpeedState() 
    {
        if (_currentSpeed < _sprintSpeed)
        {
            _playerSpeedState = PlayerSpeed.Normal;
        }
        else if (_currentSpeed >= _sprintSpeed)
        {
            _playerSpeedState = PlayerSpeed.Fast;
        }

    }
    private void ToggleSprintParticles(bool inBool)
    {
        ParticleSystem.EmissionModule emissionModule = _particleSystems[0].emission;
        emissionModule.enabled = inBool;
    }
    private void TriggerBoostParticles()
    {
        ParticleSystem.EmissionModule emissionModule = _particleSystems[1].emission;
        emissionModule.enabled = true;
        _particleSystems[1].Play();
    }
}
