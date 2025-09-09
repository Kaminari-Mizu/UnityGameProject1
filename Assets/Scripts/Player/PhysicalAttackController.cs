using System.Collections;
using UnityEngine;

public class PhysicalAttackController : MonoBehaviour
{
    public GameObject scythe;
    public bool isAttacking = false;
    private Animator animator;
    private int comboIndex = 0;
    private float comboTimer = 0f;
    private float comboResetTime = 1f;
    private int physicalAttackLayerIndex;
    private bool isAnimationLocked = false;
    private float attackAnimationLength = 0.5f;

    // Block until user releases mouse after scene load
    private bool blockInputUntilRelease = true;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        if (animator == null)
        {
            Debug.LogError("Animator not found on modelTransform!");
        }
        else
        {
            physicalAttackLayerIndex = animator.GetLayerIndex("Physical Layer");
        }

        // Ensure attack visuals/params are clean immediately
        ResetAttackState();
    }

    private void Start()
    {
        if (scythe != null) scythe.SetActive(false);

        if (physicalAttackLayerIndex == -1)
            Debug.LogError("PhysicalAttack layer not found in Animator!");

        // Wait until any mouse buttons are released so clicks from UI don't carry over
        StartCoroutine(BlockUntilMouseReleased());
    }

    private IEnumerator BlockUntilMouseReleased()
    {
        // wait a frame to let UI click events process
        yield return null;

        // wait until left and right mouse are not held
        while (Input.GetMouseButton(0) || Input.GetMouseButton(1))
        {
            yield return null;
        }

        blockInputUntilRelease = false;
        Debug.Log("PhysicalAttackController: input unblocked (mouse released).");
    }

    void Update()
    {
        // If we are blocking input because of scene transition, don't run attack logic
        if (blockInputUntilRelease) return;

        // Update layer weight based on attack state
        if (animator != null)
            animator.SetLayerWeight(physicalAttackLayerIndex, isAttacking ? 1f : 0f);

        if (comboTimer > 0)
        {
            comboTimer -= Time.deltaTime;
            if (comboTimer <= 0)
            {
                comboIndex = 0;
                isAttacking = false;
                isAnimationLocked = false;
                if (scythe != null) scythe.SetActive(false);
                animator?.SetInteger("ComboIndex", comboIndex);
            }
        }

        if (Input.GetMouseButtonDown(0) && !isAnimationLocked)
        {
            if (comboIndex == 0)
            {
                isAttacking = true;
            }
            if (comboIndex < 3)
            {
                comboIndex++;
                animator?.SetInteger("ComboIndex", comboIndex);
                animator?.SetTrigger("PhysicalAttack");
                if (scythe != null) scythe.SetActive(true);
                comboTimer = comboResetTime;
                isAnimationLocked = true;
                Invoke(nameof(UnlockAnimation), attackAnimationLength);
            }
            else
            {
                comboIndex = 0;
                isAttacking = false;
                isAnimationLocked = false;
                if (scythe != null) scythe.SetActive(false);
            }
        }
    }

    private void UnlockAnimation()
    {
        isAnimationLocked = false;
        Debug.Log("Animation unlocked, ready for next attack");
    }

    // Public reset so other systems (e.g. save loader) can force a clean state
    public void ResetAttackState()
    {
        comboIndex = 0;
        comboTimer = 0f;
        isAttacking = false;
        isAnimationLocked = false;
        if (scythe != null) scythe.SetActive(false);

        if (animator != null)
        {
            animator.ResetTrigger("PhysicalAttack");
            animator.SetInteger("ComboIndex", 0);
            // optionally force a safe state on layer
            if (physicalAttackLayerIndex >= 0)
                animator.SetLayerWeight(physicalAttackLayerIndex, 0f);
        }
    }
}
