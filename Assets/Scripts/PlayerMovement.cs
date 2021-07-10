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
    [SerializeField] private Vector3 _velocity;
    [SerializeField] private Vector3 _characterDirection;

    [SerializeField] private Vector3 _moveDirection = Vector3.zero;
    private Vector3 _inputDirection = Vector3.zero;

    private bool _playerIsMoving;
    private bool _playerJumped;
    private bool _playerIsSprinting;
    private bool _playerIsBoosting;

    //animation related fields?
    private Vector3 _viewingVector;
    private Transform _playerModel;

    private Animator _animator;

    //particle effect related fields
    [SerializeField] private Transform _particleSystemParent;
    [SerializeField] private List<ParticleSystem> _particleSystems= new List<ParticleSystem>();
        //Particle Index
        //0 Sprint particles
        //1 boost particles

    private PlayerInput _playerInput;

    //initialization
    private void OnEnable()
    {
        Debug.Log("enabled");
        //enable player controls
        _playerInput.CharacterControls.Move.performed += MoveHandler;
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
        _playerInput.CharacterControls.Move.canceled -= MoveHandler;
        _playerInput.CharacterControls.Jump.started -= JumpHandler;
        _playerInput.CharacterControls.Sprint.started -= SprintHandler;
        _playerInput.CharacterControls.Sprint.canceled -= SprintHandler;
        _playerInput.CharacterControls.Disable();
    }

    private void Awake()
    {
        _playerInput = new PlayerInput();

        _currentSpeed = _moveSpeed;
        _playerModel = transform.GetChild(0);
        _characterController = GetComponent<CharacterController>();
        _animator = _playerModel.GetComponentInChildren<Animator>();
        CacheParticleSystems();
    }
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
        _velocity = _characterController.velocity;

        HandleAnimation();
    }
    private void FixedUpdate()
    {
        ProccessMoveDirection();
        ProccessJump();
        ProccessCharacterModelRotation();

        _characterController.Move(_moveDirection);

    }


    //inputhandlers
    private void ReloadHandler(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void SprintHandler(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {


        if (_characterController.velocity != Vector3.zero && context.started == true)
        {
            TriggerBoostParticles();
            ToggleSprintParticles(true);
            _currentSpeed = _boostSpeed;
            _playerIsSprinting = true;
        }
        else if (_characterController.velocity == Vector3.zero && context.performed == true)
        {
            _currentSpeed = _sprintSpeed;
            _playerIsSprinting = true;
        }
        else if (_characterController.velocity != Vector3.zero && context.performed == true)
        {
            StartCoroutine(DegradeSpeed());
        }
        else
        {
            ToggleSprintParticles(false);
            _currentSpeed = _moveSpeed;
            _playerIsSprinting = false;
        }


    }
    private void JumpHandler(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if (_characterController.isGrounded && context.started == true)
        {
            _playerJumped = true;
        }
    }
    private void MoveHandler(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        _inputDirection = new Vector3(context.ReadValue<Vector2>().x, 0f, context.ReadValue<Vector2>().y);
        if (context.performed == true)
        {
            _playerIsMoving = true;
        }
        else
        {
            _playerIsMoving = false;
        }
    }
    IEnumerator DegradeSpeed() 
    {
        while (_currentSpeed>_sprintSpeed)
        {
            _currentSpeed -= (_boostSpeed - _sprintSpeed) * .2f;

            yield return new WaitForSeconds(.25f*_boostTime);
        }
    }



    //movement/animation handlers
    private void HandleAnimation()
    {
        bool animatorMoveBool = _animator.GetBool("isMoving");
        bool animatorSprintBool = _animator.GetBool("isSprinting");
        if (animatorMoveBool == false && _playerIsMoving == true)
        {
            _animator.SetBool("isMoving", true);
        }
        if (animatorMoveBool == true && _playerIsMoving == false)
        {
            _animator.SetBool("isMoving", false);
        }
        if (animatorSprintBool == false && _playerIsSprinting == true)
        {
            _animator.SetBool("isSprinting", true);
        }
        if (animatorSprintBool == true && _playerIsSprinting == false)
        {
            _animator.SetBool("isSprinting", false);
        }
    }
    private void ProccessMoveDirection()
    {
        //Vector3 transformDirection = transform.TransformDirection(_inputDirection);
        //Vector3 groundMovement = _currentSpeed * Time.deltaTime * transformDirection;
        Vector3 groundMovement = _currentSpeed * Time.deltaTime * _inputDirection;
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
        _characterDirection = _viewingVector;
        if (_viewingVector != Vector3.zero)
        {
            _playerModel.transform.rotation = Quaternion.LookRotation(_viewingVector, Vector3.up);
        }
    }

    //misc. methods
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
