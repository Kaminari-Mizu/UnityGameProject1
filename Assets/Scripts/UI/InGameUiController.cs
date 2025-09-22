using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;

public class InGameUiController : MonoBehaviour
{
    public ProgressBar healthbar;
    public ProgressBar manabar;
    private PlayerStats playerStats;

    void Start()
    {
        StartCoroutine(InitializeUI());
        playerStats = FindFirstObjectByType<PlayerStats>(); // Find PlayerStats dynamically
        if (playerStats == null)
        {
            Debug.LogError("InGameUiController: PlayerStats not found");
        }
    }

    private IEnumerator InitializeUI()
    {
        const int maxRetries = 5;
        const float retryDelay = 0.1f;
        for (int i = 0; i < maxRetries; i++)
        {
            var uiDocument = GetComponent<UIDocument>();
            if (uiDocument == null)
            {
                Debug.LogError("InGameUiController: UIDocument component not found on GameObject");
                yield break;
            }

            if (uiDocument.visualTreeAsset == null)
            {
                Debug.LogError("InGameUiController: UIDocument has no VisualTreeAsset assigned");
                yield break;
            }

            var root = uiDocument.rootVisualElement;
            if (root == null)
            {
                Debug.LogWarning($"InGameUiController: Root VisualElement not found, retrying (attempt {i + 1}/{maxRetries})");
                yield return new WaitForSecondsRealtime(retryDelay);
                continue;
            }

            healthbar = root.Q<ProgressBar>("HealthBar");
            manabar = root.Q<ProgressBar>("ManaBar");

            if (healthbar == null)
            {
                Debug.LogWarning($"InGameUiController: HealthBar ProgressBar not found, retrying (attempt {i + 1}/{maxRetries})");
            }
            if (manabar == null)
            {
                Debug.LogWarning($"InGameUiController: ManaBar ProgressBar not found, retrying (attempt {i + 1}/{maxRetries})");
            }

            if (healthbar != null && manabar != null)
            {
                Debug.Log("InGameUiController: Initialized UI elements successfully");
                yield break;
            }

            yield return new WaitForSecondsRealtime(retryDelay);
        }

        if (healthbar == null)
        {
            Debug.LogError("InGameUiController: Failed to find HealthBar ProgressBar after retries");
        }
        if (manabar == null)
        {
            Debug.LogError("InGameUiController: Failed to find ManaBar ProgressBar after retries");
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.H) && playerStats != null)
        {
            playerStats.currentHealth = 250f; // Set to half
            UpdateHealth(playerStats.currentHealth, playerStats.maxHealth);
        }
        if (Input.GetKeyDown(KeyCode.J) && playerStats != null)
        {
            playerStats.currentMana = 125f; // Set to half
            UpdateMana(playerStats.currentMana, playerStats.maxMana);
        }
    }

    public void UpdateHealth(float currentHealth, float maxHealth)
    {
        if (healthbar != null)
        {
            healthbar.value = currentHealth;
            healthbar.highValue = maxHealth;
            Debug.Log($"InGameUiController: Updated HealthBar to {currentHealth}/{maxHealth}");
        }
        else
        {
            Debug.LogError("InGameUiController: Cannot update health, HealthBar is null");
        }
    }

    public void UpdateMana(float currentMana, float maxMana)
    {
        if (manabar != null)
        {
            manabar.value = currentMana;
            manabar.highValue = maxMana;
            Debug.Log($"InGameUiController: Updated ManaBar to {currentMana}/{maxMana}");
        }
        else
        {
            Debug.LogError("InGameUiController: Cannot update mana, ManaBar is null");
        }
    }
}