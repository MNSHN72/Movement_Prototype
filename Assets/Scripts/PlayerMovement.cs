using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _sprintSpeed = 8f;
    [SerializeField] private float _currentSpeed = 5f;
    [SerializeField] private float _jumpSpeed = 1f;
    [SerializeField] private float _gravity = .25f;

    [SerializeField] private Vector3 _moveDirection = Vector3.zero;
    private Vector3 _viewingVector;
    private Transform _playerModel;
    private CharacterController _characterController;
    private void Awake()
    {
        _playerModel = transform.GetChild(0);
        _characterController = GetComponent<CharacterController>();
    }
    private void Update()
    {
        //Debug.Log(_characterController.isGrounded);
        Debug.Log(_characterController.velocity);
        if (_characterController.isGrounded && Input.GetButtonDown("Jump"))
        {
            PlayerJumped = true;
        }
        if (Input.GetButton("Sprint"))
        {
            _currentSpeed = _sprintSpeed;
        }
        else
        {
            _currentSpeed = _moveSpeed;
        }
    }
    private void FixedUpdate()
    {

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 inputDirection = new Vector3(horizontal, 0, vertical);
        Vector3 transformDirection = transform.TransformDirection(inputDirection);

        Vector3 groundMovement = _currentSpeed * Time.deltaTime * transformDirection;

        _moveDirection = new Vector3(groundMovement.x, _moveDirection.y, groundMovement.z);

        if (PlayerJumped)
        {
            _moveDirection.y = _jumpSpeed;
            PlayerJumped = false;
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
    private bool PlayerJumped;


}
