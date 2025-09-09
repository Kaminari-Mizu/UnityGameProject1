#if (ENABLE_INPUT_SYSTEM && INPUT_SYSTEM_INSTALLED)
#define USE_INPUT_SYSTEM
#endif

#if USE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController controller;
    public Transform modelTransform;
    public Transform foamGeneratorParent;
    public float rotationSpeed = 5f;
    public float speed = 12f;
    public float gravity = -9.81f;
    public float jumpHeight = 2f;
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    // Reference to the camera
    private Camera mainCamera;

    //Reference to animator
    private Animator animator;

    //Reference to underwater movement script
    private UnderwaterMovement underwaterMovement;

    //Reference to physical attack controller
    private PhysicalAttackController physicalAttackController;

    Vector3 velocity;
    bool isGrounded;

#if USE_INPUT_SYSTEM
    InputAction movement;
    InputAction jump;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        mainCamera = Camera.main; // Get the main camera

        //Get Animator Component
        animator = modelTransform.GetComponent<Animator>();
        if(animator == null)
        {
            Debug.LogError("Animator Component not found on modelTransform");
        }

        underwaterMovement = GetComponent<UnderwaterMovement>();
        if(underwaterMovement == null)
        {
            Debug.LogError("Underwater Movement script not found on Player");
        }
         //Get physical attack controller component that's on player
        physicalAttackController = GetComponent<PhysicalAttackController>();
        if(physicalAttackController == null)
        {
            Debug.LogError("Physical Attack Controller Script not found on Player");
        }

        movement = new InputAction("PlayerMovement", binding: "<Gamepad>/leftStick");
        movement.AddCompositeBinding("Dpad")
            .With("Up", "<Keyboard>/w")
            .With("Up", "<Keyboard>/upArrow")
            .With("Down", "<Keyboard>/s")
            .With("Down", "<Keyboard>/downArrow")
            .With("Left", "<Keyboard>/a")
            .With("Left", "<Keyboard>/leftArrow")
            .With("Right", "<Keyboard>/d")
            .With("Right", "<Keyboard>/rightArrow");

        jump = new InputAction("PlayerJump", binding: "<Keyboard>/space");

        movement.Enable();
        jump.Enable();
    }
#else
    void Start()
    {
        controller = GetComponent<CharacterController>();
        mainCamera = Camera.main;
        //Get Animator Component
        animator = modelTransform.GetComponent<Animator>();
        if(animator == null)
        {
            Debug.LogError("Animator Component not found on modelTransform");
        }
        //Get underwater movement script component that's on player
        underwaterMovement = GetComponent<UnderwaterMovement>();
        if(underwaterMovement == null)
        {
            Debug.LogError("Underwater Movement script not found on Player");
        }
        //Get physical attack controller component that's on player
        physicalAttackController = GetComponent<PhysicalAttackController>();
        if(physicalAttackController == null)
        {
            Debug.LogError("Physical Attack Controller Script not found on Player");
        }
    }
#endif

    void Update()
    {
        //Only handle land movement when not underwater/deep in water
        if (underwaterMovement != null && !underwaterMovement.isSwimming)
        {
            float x;
            float z;
            bool jumpPressed = false;

#if USE_INPUT_SYSTEM
        var delta = movement.ReadValue<Vector2>();
        x = delta.x; // Remove negation
        z = delta.y; // Remove negation
        jumpPressed = jump.WasPressedThisFrame();
        if (jumpPressed)
            {
                Debug.Log("Jump input detected (Input System)!");
            }
#else
            x = Input.GetAxis("Horizontal");
            z = Input.GetAxis("Vertical");
            jumpPressed = Input.GetButtonDown("Jump");
            if (jumpPressed)
            {
                Debug.Log("Jump input detected (Legacy Input)!");
            }
#endif

            isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
            //Debug.Log($"IsGrounded: {isGrounded}, GroundCheck Position: {groundCheck.position}");

            if (isGrounded && velocity.y < 0)
            {
                velocity.y = -5f;
            }

            // Calculate movement relative to camera's orientation
            Vector3 cameraForward = mainCamera.transform.forward;
            Vector3 cameraRight = mainCamera.transform.right;

            // Project onto the ground plane (ignore y component)
            cameraForward.y = 0;
            cameraRight.y = 0;
            cameraForward = cameraForward.normalized;
            cameraRight = cameraRight.normalized;

            // Calculate move direction based on camera
            Vector3 move = cameraRight * x + cameraForward * z;
            move = Vector3.Normalize(move);

            bool isAttacking = physicalAttackController != null && physicalAttackController.isAttacking;
            if(isAttacking)
            {
                move = Vector3.zero;
            }

            controller.Move(move * speed * Time.deltaTime);

            if (jumpPressed && isGrounded && !isAttacking)
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                Debug.Log($"Jump triggered! Velocity Y: {velocity.y}");
            }

            velocity.y += gravity * Time.deltaTime;

            Vector3 totalMove = move + velocity;
            controller.Move(totalMove * Time.deltaTime);

            if (move.magnitude > 0.1f && mainCamera != null)
            {
                Quaternion targetRotation = Quaternion.LookRotation(cameraForward, Vector3.up);
                modelTransform.rotation = Quaternion.RotateTowards(modelTransform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }

            //Update animator with movement state
            if (animator != null)
            {
                bool isMoving = move.magnitude > 0.1f; //threshold to avoid jitter
                animator.SetBool("IsRunning", isMoving);
            }

            foamGeneratorParent.localScale = Vector3.one * move.magnitude;
        }
    }
}