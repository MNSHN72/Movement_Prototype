using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _sprintSpeed = 8f;
    [SerializeField] private float _currentSpeed = 5f;
    [SerializeField] private float _jumpSpeed = 1f;
    [SerializeField] private float _gravity = .25f;
    [SerializeField] private ParticleSystem _sprintParticles;

    [SerializeField] private Vector3 _moveDirection = Vector3.zero;
    private Vector3 _inputDirection = Vector3.zero;
    private Vector3 _viewingVector;
    private Transform _playerModel;
    private CharacterController _characterController;

    private bool _playerIsMoving;
    private bool _playerJumped;
    private bool _playerIsSprinting;

    private PlayerInput _playerInput;
    private void OnEnable()
    {
        _playerInput.CharacterControls.Enable();
    }
    private void OnDisable()
    {
        _playerInput.CharacterControls.Disable();
    }

    private void Awake()
    {
        _currentSpeed = _moveSpeed;
        _playerModel = transform.GetChild(0);
        _characterController = GetComponent<CharacterController>();
        _sprintParticles = _playerModel.transform.GetChild(2).gameObject.GetComponent<ParticleSystem>(); ;

        _playerInput = new PlayerInput();
        _playerInput.CharacterControls.Move.performed += MoveHandler;
        _playerInput.CharacterControls.Move.canceled += MoveHandler;
        _playerInput.CharacterControls.Jump.started += JumpHandler;
        _playerInput.CharacterControls.Sprint.started += SprintHandler;
        _playerInput.CharacterControls.Sprint.canceled += SprintHandler;
        _playerInput.CharacterControls.ReloadCurrentScene.started += context =>
        { SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); };
    }

    private void SprintHandler(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if (_characterController.velocity != Vector3.zero && context.started == true)
        {
            ParticleSystem.EmissionModule emissionModule = _sprintParticles.emission;
            emissionModule.enabled = true;
            _currentSpeed = _sprintSpeed;
            _playerIsSprinting = true;
        }
        else
        {
            ParticleSystem.EmissionModule emissionModule = _sprintParticles.emission;
            emissionModule.enabled = false;
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
        if (context.performed == true)
        {
            if (_playerIsSprinting)
            {
                ParticleSystem.EmissionModule emissionModule = _sprintParticles.emission;
                emissionModule.enabled = true;
            }
            _inputDirection = new Vector3(context.ReadValue<Vector2>().x, 0f, context.ReadValue<Vector2>().y);
            _playerIsMoving = true;
        }
        else
        {
            ParticleSystem.EmissionModule emissionModule = _sprintParticles.emission;
            emissionModule.enabled = false;
            _inputDirection = new Vector3(context.ReadValue<Vector2>().x, 0f, context.ReadValue<Vector2>().y);
            _playerIsMoving = false;
        }
    }

    private void Update()
    {
        //Debug.Log(_characterController.isGrounded);
        Debug.Log(_characterController.velocity);

        //if (Input.GetKeyDown(KeyCode.P))
        //{
        //    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        //}
    }



    private void FixedUpdate()
    {

        Vector3 transformDirection = transform.TransformDirection(_inputDirection);

        Vector3 groundMovement = _currentSpeed * Time.deltaTime * transformDirection;

        _moveDirection = new Vector3(groundMovement.x, _moveDirection.y, groundMovement.z);

        if (_playerJumped)
        {
            _moveDirection.y = _jumpSpeed;
            _playerJumped = false;
        }
        else if (_characterController.isGrounded == false)
        {
            _moveDirection.y -= _gravity * Time.deltaTime;
        }

        _viewingVector = new Vector3(_characterController.velocity.x, 0f, _characterController.velocity.z);
        if (_viewingVector != Vector3.zero)
        {
            _playerModel.transform.rotation = Quaternion.LookRotation(_viewingVector, Vector3.up);
        }

        _characterController.Move(_moveDirection);

    }



}
