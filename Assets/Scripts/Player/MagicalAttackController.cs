using UnityEngine;

public class MagicalAttackController : MonoBehaviour
{
    public bool isAttacking = false; // Public flag for movement check
    private Animator animator;
    private int comboIndex = 0;
    private float comboTimer = 0f;
    private float comboResetTime = 1f;
    private int magicalAttackLayerIndex; // Index of the PhysicalAttack layer
    private bool isAnimationLocked = false; // Prevent new attacks during animation
    private float attackAnimationLength = 0.5f; // Adjust based on your animation length

    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        if (animator == null)
        {
            Debug.LogError("Animator not found on modelTransform!");
            return;
        }
        magicalAttackLayerIndex = animator.GetLayerIndex("Magic Layer"); // Get layer index
        if (magicalAttackLayerIndex == -1)
        {
            Debug.LogError("Magic Attack layer not found in Animator!");
        }
    }

    void Update()
    {
        // Update layer weight based on attack state
        animator.SetLayerWeight(magicalAttackLayerIndex, isAttacking ? 1f : 0f);

        if (comboTimer > 0)
        {
            comboTimer -= Time.deltaTime;
            if (comboTimer <= 0)
            {
                comboIndex = 0;
                isAttacking = false;
                isAnimationLocked = false;
                animator.SetInteger("ComboIndex", comboIndex); // Force update
            }
        }

        if (Input.GetMouseButtonDown(1) && !isAnimationLocked) // Left mouse button
        {
            if (comboIndex == 0)
            {
                isAttacking = true;
            }
            if (comboIndex < 3)
            {
                comboIndex++;
                animator.SetInteger("ComboIndex", comboIndex);
                animator.SetTrigger("MagicAttack");
                comboTimer = comboResetTime;
                isAnimationLocked = true; // Lock until animation finishes
                // Wait for animation length before unlocking
                Invoke(nameof(UnlockAnimation), attackAnimationLength);
            }
            else
            {
                comboIndex = 0;
                isAttacking = false;
                isAnimationLocked = false;
            }
        }
    }
    private void UnlockAnimation()
    {
        isAnimationLocked = false;
        Debug.Log("Animation unlocked, ready for next attack");
    }
}
