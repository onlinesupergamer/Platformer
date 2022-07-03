using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class Movement : MonoBehaviour
{
    Player_Input m_playerInput;
    CharacterController characterController;
    Animator animator;

    int m_isWalkingHash;
    int m_isJumpingHash;
    bool bisJumpAnimating;
    


    Vector2 m_currentMovementInput;
    Vector3 m_currentMovement;
    bool bisMovementPressed;
    float m_rotationFactorPerFrame = 15f;
    float gravity = -3f;
    float groundedGravity = -0.01f;

    bool bisJumpButtonPressed = false;

    float m_initialJumpVelocity;
    float m_maxJumpHeight = 4f;
    float m_maxJumpTime = 0.75f;
    bool bisJumping = false;
    int int_jumpCount = 0;
    int int_jumpCountHash;
    Dictionary<int, float> initialJumpVelocities = new Dictionary<int, float>();
    Dictionary<int, float> jumpGravities = new Dictionary<int, float>();
    Coroutine currentJumpResetRoutine = null;




    private void Awake()
    {
        m_playerInput = new Player_Input();
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        m_isWalkingHash = Animator.StringToHash("IsWalking");
        m_isJumpingHash = Animator.StringToHash("IsJumping");
        int_jumpCountHash = Animator.StringToHash("JumpCount");

        m_playerInput.CharacterControls.Move.started += OnMovementInput;
        m_playerInput.CharacterControls.Move.canceled += OnMovementInput;
        m_playerInput.CharacterControls.Move.performed += OnMovementInput;

        m_playerInput.CharacterControls.Jump.started += onJump;
        m_playerInput.CharacterControls.Jump.canceled += onJump;


        SetupJumpVariables();
    }

    void onJump(InputAction.CallbackContext context) 
    {
        bisJumpButtonPressed = context.ReadValueAsButton();
    }

    void OnMovementInput(InputAction.CallbackContext context) 
    {
        m_currentMovementInput = context.ReadValue<Vector2>();
        m_currentMovement.x = m_currentMovementInput.x * 3;
        m_currentMovement.z = m_currentMovementInput.y * 3;
        bisMovementPressed = m_currentMovementInput.x != 0 || m_currentMovementInput.y != 0;

    }

    void SetupJumpVariables() 
    {
        float timeToApex = m_maxJumpTime / 2;
        gravity = (-2 * m_maxJumpHeight) / Mathf.Pow(timeToApex, 2);
        m_initialJumpVelocity = (2 * m_maxJumpHeight) / timeToApex;

        float secondJumpGravity = (-2 * (m_maxJumpHeight + 2)) / Mathf.Pow((timeToApex * 1.25f), 2);
        float secondJumpInitialVelocity = (2 * (m_maxJumpHeight + 2)) / (timeToApex * 1.25f);
        float thirdJumpGravity = (-2 * (m_maxJumpHeight + 4)) / Mathf.Pow((timeToApex * 1.5f), 2);
        float thirdJumpInitialVelocity = (2 * (m_maxJumpHeight + 4)) / (timeToApex * 1.5f);

        initialJumpVelocities.Add(1, m_initialJumpVelocity);
        initialJumpVelocities.Add(2, secondJumpInitialVelocity);
        initialJumpVelocities.Add(3, thirdJumpInitialVelocity);

        jumpGravities.Add(0, gravity);
        jumpGravities.Add(1, gravity);
        jumpGravities.Add(2, secondJumpGravity);
        jumpGravities.Add(3, thirdJumpGravity);




    }

    void HandleGravity()
    {
        bool bisFalling = m_currentMovement.y <= 0.0f || !bisJumpButtonPressed;
        float fallMultiplier = 2.0f;

        if (characterController.isGrounded)
        {
            if (bisJumpAnimating) 
            {
                animator.SetBool(m_isJumpingHash, false);
                bisJumpAnimating = false;
                currentJumpResetRoutine = StartCoroutine(jumpResetRoutine());
                if (int_jumpCount == 3) 
                {
                    int_jumpCount = 0;
                    animator.SetInteger(int_jumpCountHash, int_jumpCount);
                }
            }
            
            m_currentMovement.y = groundedGravity;
        }
        else if (bisFalling) 
        {
            float previousYVelocity = m_currentMovement.y;
            float newYVelocity = m_currentMovement.y + (jumpGravities[int_jumpCount] * fallMultiplier * Time.deltaTime);
            float nextYVelocity = Mathf.Max((previousYVelocity + newYVelocity) * 0.5f, -20.0f);
            m_currentMovement.y = nextYVelocity;

        }

        else
        {
            float previousYVelocity = m_currentMovement.y;
            float newYVelocity = m_currentMovement.y + (jumpGravities[int_jumpCount] * Time.deltaTime);
            float nextYVelocity = (previousYVelocity + newYVelocity) * 0.5f;
            m_currentMovement.y = nextYVelocity;

        }
    }

    void HandleJump()
    {
        if (!bisJumping && characterController.isGrounded && bisJumpButtonPressed)
        {
            if (int_jumpCount < 3 && currentJumpResetRoutine != null) 
            {
                StopCoroutine(currentJumpResetRoutine);
            }
            animator.SetBool(m_isJumpingHash, true);
            bisJumpAnimating = true;
            bisJumping = true;
            int_jumpCount += 1;
            animator.SetInteger(int_jumpCountHash, int_jumpCount);
            m_currentMovement.y = initialJumpVelocities[int_jumpCount] * 0.5f;

        }
        else if (!bisJumpButtonPressed && bisJumping && characterController.isGrounded) 
        {
            bisJumping = false;
        }

    }

    IEnumerator jumpResetRoutine() 
    {
        yield return new WaitForSeconds(0.5f);
        int_jumpCount = 0;
    }


    void Update()
    {  
        HandleAnimation();
        HandleRotation();
        characterController.Move(m_currentMovement * Time.deltaTime);

       HandleGravity();
        HandleJump();

    }
   

    void HandleAnimation() 
    {
        bool bisWalking = animator.GetBool("IsWalking");
        bool bisRunning = animator.GetBool("IsRunning");

        if (bisMovementPressed && !bisWalking) 
        {
            animator.SetBool(m_isWalkingHash, true);
        }
        else if (!bisMovementPressed && bisWalking)
        {
            animator.SetBool("IsWalking", false);
        }

    }

    void HandleRotation() 
    {
        Vector3 positionToLookAt;

        positionToLookAt.x = m_currentMovement.x;
        positionToLookAt.y = 0.0f;
        positionToLookAt.z = m_currentMovement.z;

        Quaternion m_currentRotation = transform.rotation;

        if (bisMovementPressed)
        {
            Quaternion targetRotation = Quaternion.LookRotation(positionToLookAt);
            transform.rotation = Quaternion.Slerp(m_currentRotation, targetRotation, m_rotationFactorPerFrame * Time.deltaTime);
        }

    }

    


    

    void OnEnable()
    {
        m_playerInput.CharacterControls.Enable();
    }

    void OnDisable()
    {
        m_playerInput.CharacterControls.Disable();
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
