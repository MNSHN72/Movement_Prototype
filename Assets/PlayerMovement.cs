using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private Vector3 defaultPosition = new Vector3(0f, 0.5f, 0f);
    [SerializeField] private float moveLimit = 5f;
    private 
    private bool shouldReset => Input.GetKey(KeyCode.Space);
    private bool shouldMoveRight => Input.GetAxis("Horizontal") > 0 && this.gameObject.transform.position.x <= moveLimit;
    private bool shouldMoveLeft => Input.GetAxis("Horizontal") < 0 && this.gameObject.transform.position.x >= -moveLimit;
    private bool shouldMoveForward => Input.GetAxis("Vertical") > 0 && this.gameObject.transform.position.z <= moveLimit;
    private bool shouldMoveBack => Input.GetAxis("Vertical") < 0 && this.gameObject.transform.position.z >= -moveLimit;
    void Update()
    { 
        transform.TransformDirection
        if (shouldMoveRight)
        {
            this.gameObject.transform.position += Vector3.right * Time.deltaTime * _moveSpeed;
        }
        if (shouldMoveLeft)
        {
            this.gameObject.transform.position += Vector3.left * Time.deltaTime * _moveSpeed;
        }
        if (shouldMoveForward)
        {
            this.gameObject.transform.position += Vector3.forward * Time.deltaTime * _moveSpeed;
        }
        if (shouldMoveBack)
        {
            this.gameObject.transform.position += Vector3.back * Time.deltaTime * _moveSpeed;
        }
        if (shouldReset)
        {
            this.gameObject.transform.position = defaultPosition;
        }
    }
}
