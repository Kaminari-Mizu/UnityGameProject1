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
        if (uiController == null)
        {
            uiController = FindFirstObjectByType<InGameUiController>();
            if (uiController != null)
            {
                Debug.Log($"PlayerStats: InGameUiController found dynamically: {uiController.gameObject.name} at {System.DateTime.Now:HH:mm:ss} SAST");
            }
            else
            {
                Debug.LogError("PlayerStats: Failed to find InGameUiController at {System.DateTime.Now:HH:mm:ss} SAST");
                yield break;
            }
        }
        else
        {
            Debug.Log($"PlayerStats: InGameUiController assigned in Inspector: {uiController.gameObject.name} at {System.DateTime.Now:HH:mm:ss} SAST");
        }
        InitializeUI();
    }

    private void InitializeUI()
    {
        if (uiController != null)
        {
            UpdateUI();
        }
        else
        {
            Debug.LogWarning("PlayerStats: Cannot initialize UI, InGameUiController is null at {System.DateTime.Now:HH:mm:ss} SAST");
        }
    }

    public void SaveState(SaveData data)
    {
        data.health = currentHealth;
        data.mana = currentMana;
        data.playerPosition = transform.position;
        var animator = GetComponent<PlayerMovement>()?.modelTransform.GetComponent<Animator>();
        var underwaterMovement = GetComponent<UnderwaterMovement>();
        var physicalAttack = GetComponent<PhysicalAttackController>();
        var magicalAttack = GetComponent<MagicalAttackController>();

        if (animator != null)
        {
            data.isRunning = animator.GetBool("IsRunning");
            data.isFloating = animator.GetBool("IsFloating");
        }
        else
        {
            Debug.LogWarning("PlayerStats: Animator not found for saving state");
        }
        data.isSwimming = underwaterMovement?.isSwimming ?? false;
        data.isPhysicalAttacking = physicalAttack?.isAttacking ?? false;
        data.physicalComboIndex = animator?.GetInteger("ComboIndex") ?? 0;
        data.isMagicalAttacking = magicalAttack?.isAttacking ?? false;
        data.magicalComboIndex = animator?.GetInteger("ComboIndex") ?? 0;

        Debug.Log($"PlayerStats: Saved state - Health={data.health}, Mana={data.mana}, Position={data.playerPosition}");
    }

    public void LoadState(SaveData data)
    {
        currentHealth = data.health;
        currentMana = data.mana;

        if (characterController != null)
        {
            characterController.enabled = false;
            transform.position = data.playerPosition;
            characterController.enabled = true;
            Debug.Log($"PlayerStats: Set position to {data.playerPosition}");
        }
        else
        {
            transform.position = data.playerPosition;
            Debug.LogWarning("PlayerStats: CharacterController not found, set position directly");
        }

        var playerMovement = GetComponent<PlayerMovement>();
        var underwaterMovement = GetComponent<UnderwaterMovement>();
        var physicalAttack = GetComponent<PhysicalAttackController>();
        var magicalAttack = GetComponent<MagicalAttackController>();
        var animator = playerMovement?.modelTransform.GetComponent<Animator>();

        if (!playerMovement || !underwaterMovement || !physicalAttack || !magicalAttack || !animator)
        {
            Debug.LogError("PlayerStats: Missing required components for loading state");
            return;
        }

        animator.SetBool("IsRunning", data.isRunning);
        animator.SetBool("IsSwimming", data.isSwimming);
        animator.SetBool("IsFloating", data.isFloating);

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

        underwaterMovement.isSwimming = data.isSwimming;
        StartCoroutine(UpdateUIAfterLoad(data.health, data.mana));
        Debug.Log($"Player state loaded: Position={data.playerPosition}, Health={data.health}, Mana={data.mana}");
    }

    private IEnumerator UpdateUIAfterLoad(float health, float mana)
    {
        const int maxRetries = 10;
        const float retryDelay = 0.2f;
        for (int i = 0; i < maxRetries; i++)
        {
            if (uiController != null && uiController.healthbar != null && uiController.manabar != null)
            {
                uiController.UpdateHealth(health, maxHealth);
                uiController.UpdateMana(mana, maxMana);
                Debug.Log($"PlayerStats: UI updated to Health={health}/{maxHealth}, Mana={mana}/{maxMana}");
                yield break;
            }
            Debug.LogWarning($"PlayerStats: Waiting for UI initialization (attempt {i + 1}/{maxRetries})");
            yield return new WaitForSecondsRealtime(retryDelay);
        }
        Debug.LogError("PlayerStats: Failed to update UI after retries");
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