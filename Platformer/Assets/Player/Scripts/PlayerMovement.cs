using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float maximumSpeed;
    public float rotationSpeed;
    public float jumpHeight;
    public float gravityMultiplier;
    public float jumpButtonGracePeriod;
    

    Animator animator;
    float originalStepOffset;
    float ySpeed;
    float? lastGroundedTime;
    float? jumpButtonPressedTime;
    bool isJumping;
    bool isGrounded;

    CharacterController characterController;

    [SerializeField]
    Transform cameraTransform;
    
    void Start()
    {
        cameraTransform = Camera.main.transform; //Check to find a more optimal way to automatically assign the main camera transform;
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
        originalStepOffset = characterController.stepOffset;
    }

    
    void Update()
    {
        float m_horizontalInput = Input.GetAxis("Horizontal");
        float m_verticalInput = Input.GetAxis("Vertical");



        Vector3 movementDirection = new Vector3(m_horizontalInput, 0, m_verticalInput);
        float inputMagnitude = Mathf.Clamp01(movementDirection.magnitude);
        animator.SetFloat("InputMagnitude", inputMagnitude, 0.05f, Time.deltaTime);

        float speed = inputMagnitude * maximumSpeed;
        movementDirection = Quaternion.AngleAxis(cameraTransform.rotation.eulerAngles.y, Vector3.up) * movementDirection;

        movementDirection.Normalize();

        float gravity = Physics.gravity.y * gravityMultiplier;

        if (isJumping && ySpeed > 0 && Input.GetButton("Jump") == false) 
        {
            gravity *= 2;
        }
        ySpeed += gravity * Time.deltaTime;

        if (characterController.isGrounded) 
        {
            lastGroundedTime = Time.time;
        }
        if (Input.GetButtonDown("Jump")) 
        {
            jumpButtonPressedTime = Time.time;
        }

        if (Time.time - lastGroundedTime <= jumpButtonGracePeriod)
        {
            characterController.stepOffset = originalStepOffset;
            ySpeed = -0.5f;
            animator.SetBool("IsGrounded", true);
            isGrounded = true;
            animator.SetBool("IsJumping", false);
            isJumping = false;
            animator.SetBool("IsFalling", false);

            if (Time.time - jumpButtonPressedTime <= jumpButtonGracePeriod)
            {
                ySpeed = Mathf.Sqrt(jumpHeight * -3 * gravity);
                animator.SetBool("IsJumping", true);
                isJumping = true;
                jumpButtonPressedTime = null;
                lastGroundedTime = null;

            }

        }
        else 
        {
            characterController.stepOffset = 0;
            animator.SetBool("IsGrounded", false);
            isGrounded = false;

            if ((isJumping && ySpeed < 0) || ySpeed < -2) 
            {
                animator.SetBool("IsFalling", true);
            }
        }


        Vector3 Velocity = movementDirection * speed;
        Velocity = AdjustVelocityToSlope(Velocity);
        Velocity.y += ySpeed;
        characterController.Move(Velocity * Time.deltaTime);

        if (movementDirection != Vector3.zero)
        {
            animator.SetBool("IsMoving", true);
            Quaternion toRotation = Quaternion.LookRotation(movementDirection, Vector3.up);

            transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);
        }
        else
        {
            animator.SetBool("IsMoving", false);
        }




        }
    private Vector3 AdjustVelocityToSlope(Vector3 velocity) 
    {
        var ray = new Ray(transform.position, Vector3.down);

        if (Physics.Raycast(ray, out RaycastHit hitInfo, 0.2f)) 
        {
            var slopeRotation = Quaternion.FromToRotation(Vector3.up, hitInfo.normal);
            var AdjustedVelocity = slopeRotation * velocity;

            if (AdjustedVelocity.y < 0) 
            {
                return AdjustedVelocity;
            }
            
        }
        return velocity;
    }

  
}

