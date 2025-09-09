#if (ENABLE_INPUT_SYSTEM && INPUT_SYSTEM_INSTALLED)
#define USE_INPUT_SYSTEM
#endif

#if USE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class UnderwaterMovement : MonoBehaviour
{
    public CharacterController controller;
    public Transform modelTransform;
    public WaterSurface waterSurface;
    public float chestHeight = 1.5f;
    public float swimSpeed = 6f;
    public float rotationSpeed = 5f;

    private Camera mainCamera;
    private Animator animator;
    public bool isSwimming = false;
    private bool wasSwimmingLastFrame = false;
    private float swimMovementSmoothing = 0f; // For smoothing IsSwimming transitions
    //Reference to physical attack controller
    private PhysicalAttackController physicalAttackController;

#if USE_INPUT_SYSTEM
    private InputAction movement;
    private InputAction jump;
    private InputAction crouch;
#endif

    void Start()
    {
        if (controller == null) controller = GetComponent<CharacterController>();
        mainCamera = Camera.main;
        animator = modelTransform.GetComponent<Animator>();

        if (waterSurface == null)
        {
            Debug.LogError("WaterSurface reference not set in PlayerWaterInteraction.");
        }
        else if (!waterSurface.scriptInteractions)
        {
            Debug.LogWarning("Script interactions are disabled on WaterSurface. Enabling it now.");
            waterSurface.scriptInteractions = true;
        }
        //Get physical attack controller component that's on player
        physicalAttackController = GetComponent<PhysicalAttackController>();
        if (physicalAttackController == null)
        {
            Debug.LogError("Physical Attack Controller Script not found on Player");
        }

#if USE_INPUT_SYSTEM
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

        jump = new InputAction("PlayerJump", binding: "<Gamepad>/a");
        jump.AddBinding("<Keyboard>/space");

        crouch = new InputAction("PlayerCrouch", binding: "<Gamepad>/b");
        crouch.AddBinding("<Keyboard>/leftCtrl");

        movement.Enable();
        jump.Enable();
        crouch.Enable();
#endif
    }

    void Update()
    {
        float x;
        float z;
        bool jumpPressed = false;
        bool crouchPressed = false;

#if USE_INPUT_SYSTEM
        var delta = movement.ReadValue<Vector2>();
        x = delta.x;
        z = delta.y;
        jumpPressed = Mathf.Approximately(jump.ReadValue<float>(), 1);
        crouchPressed = Mathf.Approximately(crouch.ReadValue<float>(), 1);
#else
        x = Input.GetAxis("Horizontal");
        z = Input.GetAxis("Vertical");
        jumpPressed = Input.GetButton("Jump");
        crouchPressed = Input.GetButton("Crouch");
#endif

        CheckWaterLevel();

        if (isSwimming)
        {
            SwimMovement(x, z, jumpPressed, crouchPressed);
        }
        else
        {
            // Reset swim parameters when not in water
            if (animator != null)
            {
                animator.SetBool("IsFloating", false);
                animator.SetBool("IsSwimming", false);
            }
        }
    }

    void CheckWaterLevel()
    {
        Vector3 playerPos = transform.position;
        WaterSearchParameters wsp = new WaterSearchParameters
        {
            startPositionWS = playerPos,
            maxIterations = 15
        };
        WaterSearchResult wsr;

        if (waterSurface.ProjectPointOnWaterSurface(wsp, out wsr))
        {
            float waterHeight = wsr.projectedPositionWS.y;
            float chestPosY = playerPos.y + chestHeight;

            if (waterHeight >= chestPosY)
            {
                isSwimming = true;
                if (!wasSwimmingLastFrame)
                {
                    transform.position = new Vector3(playerPos.x, waterHeight, playerPos.z);
                    if (animator != null)
                    {
                        animator.SetBool("IsFloating", true);
                    }
                }
            }
            else
            {
                isSwimming = false;
            }
        }
        else
        {
            isSwimming = false;
        }

        wasSwimmingLastFrame = isSwimming;
    }

    void SwimMovement(float x, float z, bool jumpPressed, bool crouchPressed)
    {
        Vector3 cameraForward = mainCamera.transform.forward;
        Vector3 cameraRight = mainCamera.transform.right;
        cameraForward.y = 0;
        cameraRight.y = 0;
        cameraForward = cameraForward.normalized;
        cameraRight = cameraRight.normalized;

        Vector3 horizontalMove = (cameraRight * x + cameraForward * z).normalized * swimSpeed;

        float verticalInput = 0f;
        if (jumpPressed) verticalInput = 1f;
        if (crouchPressed) verticalInput = -1f;
        Vector3 verticalMove = Vector3.up * verticalInput * swimSpeed;

        Vector3 moveDir = horizontalMove + verticalMove;

        bool isAttacking = physicalAttackController != null && physicalAttackController.isAttacking;
        if (isAttacking)
        {
            moveDir = Vector3.zero;
        }

        controller.Move(moveDir * Time.deltaTime);

        if (horizontalMove != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(horizontalMove, Vector3.up);
            modelTransform.rotation = Quaternion.RotateTowards(modelTransform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        if (animator != null)
        {
            // Smooth the movement detection to prevent jitter
            float moveMagnitude = moveDir.magnitude;
            swimMovementSmoothing = Mathf.Lerp(swimMovementSmoothing, moveMagnitude, Time.deltaTime * 10f);
            bool isMoving = swimMovementSmoothing > 0.1f;
            animator.SetBool("IsSwimming", isMoving);
        }
    }
}