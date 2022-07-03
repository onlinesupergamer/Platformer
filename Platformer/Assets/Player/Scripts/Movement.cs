using UnityEngine;
using System.Collections;
public class Movement : MonoBehaviour
{

    [SerializeField] private float jumpHeight;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private float jumpButtonGracePeriod;
    [SerializeField] private float jumpHorizontalSpeed;
    [SerializeField] private float gravityMultiplier;

    private Animator animator;

    private CharacterController characterController;
    private float ySpeed;
    private float originalStepOffset;
    private float? lastGroundedTime;
    private float? jumpButtonPressedTime;
    private bool isJumping;
    private bool isGrounded;
    private bool canAttack = true;
    private bool canJump = true;

    [SerializeField]
    private Transform cameraTransform;

    void Start()
    {
        
        animator = GetComponent<Animator>();


        characterController = GetComponent<CharacterController>();
        originalStepOffset = characterController.stepOffset;
    }


    void Update()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");


        Vector3 movementDirection = new Vector3(horizontalInput, 0, verticalInput);
        float inputMagnitude = Mathf.Clamp01(movementDirection.magnitude);


        animator.SetFloat("Input Magnitude", inputMagnitude, 0.05f, Time.deltaTime);


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

        if (Input.GetButtonDown("Jump") && canJump) 
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
            
        if (isGrounded == false) 
        {
            Vector3 velocity = movementDirection * inputMagnitude * jumpHorizontalSpeed;
            velocity = AdjustVelocityToSlope(velocity);
            velocity.y += ySpeed;

            

            characterController.Move(velocity * Time.deltaTime);
        }

        if (Input.GetButtonDown("Attack") && canAttack) 
        {
            Attack();
        }

        
     
    }

    IEnumerator AttackReset() 
    {
        yield return new WaitForSeconds(0.3f);
        canJump = true;
        yield return new WaitForSeconds(0.6f);
        canAttack = true;
        
    }
    
    private void OnAnimatorMove()
    {
        if (isGrounded)
        {
            Vector3 velocity = animator.deltaPosition;
            velocity.y = ySpeed * Time.deltaTime;

            characterController.Move(velocity);
        }
    }
    private Vector3 AdjustVelocityToSlope(Vector3 velocity) 
    {
        var ray = new Ray(transform.position, Vector3.down);

        if (Physics.Raycast(ray, out RaycastHit hitInfo, 0.2f)) 
        {
            var slopeRotation = Quaternion.FromToRotation(Vector3.up, hitInfo.normal);
            var adjustedVelocity = slopeRotation * velocity;

            if (adjustedVelocity.y < 0) 
            {
                return adjustedVelocity;
            }
        }
        return velocity;
    }

    private void Attack() 
    {
        if (isGrounded) {
            canAttack = false;
            canJump = false;
            animator.SetTrigger("Attack");

            StartCoroutine(AttackReset());
        }
        

    }


    private void OnApplicationFocus(bool focus)
    {
        if (focus)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
        else 
        {
            Cursor.lockState = CursorLockMode.None;
        }
    }
    
}
