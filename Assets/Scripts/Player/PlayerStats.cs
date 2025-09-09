using UnityEngine;
using System.Collections;

public class PlayerStats : MonoBehaviour
{
    public float maxHealth = 500f;
    public float currentHealth = 500f;
    public float maxMana = 250f;
    public float currentMana = 250f;

    [SerializeField] private InGameUiController uiController; // Assign in Inspector or find dynamically
    private CharacterController characterController;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        StartCoroutine(InitializeUIController());
    }

    private IEnumerator InitializeUIController()
    {
        const int maxRetries = 5;
        const float retryDelay = 0.1f;
        for (int i = 0; i < maxRetries; i++)
        {
            if (uiController == null)
            {
                uiController = FindFirstObjectByType<InGameUiController>();
                if (uiController != null)
                {
                    Debug.Log($"PlayerStats: InGameUiController found dynamically: {uiController.gameObject.name}");
                    yield return StartCoroutine(TryUpdateUI());
                    yield break;
                }
                else
                {
                    Debug.LogWarning($"PlayerStats: InGameUiController not found (attempt {i + 1}/{maxRetries})");
                }
            }
            else
            {
                Debug.Log($"PlayerStats: InGameUiController assigned in Inspector: {uiController.gameObject.name}");
                yield return StartCoroutine(TryUpdateUI());
                yield break;
            }
            yield return new WaitForSecondsRealtime(retryDelay);
        }
        Debug.LogError("PlayerStats: Failed to find InGameUiController after retries");
    }

    private IEnumerator TryUpdateUI()
    {
        const int maxRetries = 5;
        const float retryDelay = 0.1f;
        for (int i = 0; i < maxRetries; i++)
        {
            if (uiController != null)
            {
                UpdateUI();
                yield break; // Rely on InGameUiController's error logging
            }
            yield return new WaitForSecondsRealtime(retryDelay);
        }
        Debug.LogError("PlayerStats: Failed to update UI after retries");
    }

    public void LoadState(SaveData data)
    {
        currentHealth = data.health;
        currentMana = data.mana;

        // Update position safely
        if (characterController != null)
        {
            characterController.enabled = false; // Disable to set position
            transform.position = data.playerPosition;
            characterController.enabled = true;
            Debug.Log($"PlayerStats: Set position to {data.playerPosition}");
        }
        else
        {
            transform.position = data.playerPosition;
            Debug.LogWarning("PlayerStats: CharacterController not found, set position directly");
        }

        // Update animation states
        var playerMovement = GetComponent<PlayerMovement>();
        var underwaterMovement = GetComponent<UnderwaterMovement>();
        var physicalAttack = GetComponent<PhysicalAttackController>();
        var magicalAttack = GetComponent<MagicalAttackController>();
        var animator = playerMovement?.modelTransform.GetComponent<Animator>();

        if (!playerMovement || !underwaterMovement || !physicalAttack || !magicalAttack || !animator)
        {
            Debug.LogError($"PlayerStats: Missing required components for loading state - " +
                $"PlayerMovement: {(playerMovement ? "found" : "null")}, " +
                $"UnderwaterMovement: {(underwaterMovement ? "found" : "null")}, " +
                $"PhysicalAttackController: {(physicalAttack ? "found" : "null")}, " +
                $"MagicalAttackController: {(magicalAttack ? "found" : "null")}, " +
                $"Animator: {(animator ? "found" : "null")}");
            return;
        }

        // Set movement and swimming states
        animator.SetBool("IsRunning", data.isRunning);
        animator.SetBool("IsSwimming", data.isSwimming);
        animator.SetBool("IsFloating", data.isFloating);

        // Set attack states
        physicalAttack.isAttacking = data.isPhysicalAttacking;
        animator.SetInteger("ComboIndex", data.physicalComboIndex);
        animator.SetLayerWeight(animator.GetLayerIndex("Physical Layer"), data.isPhysicalAttacking ? 1f : 0f);
        if (data.isPhysicalAttacking)
        {
            animator.SetTrigger("PhysicalAttack");
            physicalAttack.scythe.SetActive(true);
        }
        else
        {
            physicalAttack.scythe.SetActive(false);
        }

        magicalAttack.isAttacking = data.isMagicalAttacking;
        animator.SetInteger("ComboIndex", data.magicalComboIndex);
        animator.SetLayerWeight(animator.GetLayerIndex("Magic Layer"), data.isMagicalAttacking ? 1f : 0f);
        if (data.isMagicalAttacking)
        {
            animator.SetTrigger("MagicAttack");
        }

        // Update swimming state
        underwaterMovement.isSwimming = data.isSwimming;

        StartCoroutine(TryUpdateUI());
        Debug.Log($"Player state loaded: Position={data.playerPosition}, Health={data.health}, Mana={data.mana}, IsRunning={data.isRunning}, IsSwimming={data.isSwimming}, PhysicalAttack={data.isPhysicalAttacking}, MagicalAttack={data.isMagicalAttacking}");
    }

    void UpdateUI()
    {
        if (uiController != null)
        {
            uiController.UpdateHealth(currentHealth, maxHealth);
            uiController.UpdateMana(currentMana, maxMana);
            Debug.Log($"PlayerStats: Attempting to update UI with Health={currentHealth}/{maxHealth}, Mana={currentMana}/{maxMana}");
        }
        else
        {
            Debug.LogWarning("PlayerStats: Cannot update UI, InGameUiController is null");
        }
    }
}