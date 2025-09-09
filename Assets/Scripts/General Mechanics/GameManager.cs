using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    //Current logged in user id
    public string currentUserId { get; private set; }


    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void InitializeGameManager()
    {
        if (Instance == null)
        {
            GameObject gmObject = new GameObject("GameManager");
            Instance = gmObject.AddComponent<GameManager>();
            DontDestroyOnLoad(gmObject);
            Debug.Log("GameManager: Created instance before scene load");
        }
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("GameManager: Initialized singleton instance");

            // Restore login state from PlayerPrefs
            if (PlayerPrefs.HasKey("userId"))
            {
                string userId = PlayerPrefs.GetString("userId");
                if (!string.IsNullOrEmpty(userId))
                {
                    SetCurrentUser(userId);
                    Debug.Log($"GameManager: Restored userId {userId} from PlayerPrefs");
                }
            }
        }
        else
        {
            Destroy(gameObject);
            Debug.Log("GameManager: Destroyed duplicate instance");
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private SaveData pendingSaveData = null;

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if(pendingSaveData != null)
        {
            StartCoroutine(TryLoadPlayerState(pendingSaveData));
            pendingSaveData = null;
        }
    }

    // ---Public API for login--
    public void SetCurrentUser(string userId)
    {
        currentUserId = userId;
        Debug.Log($"GameManager: CurrentUserId set to {userId}");
    }

    public void ClearCurrentUser()
    {
        currentUserId=null;
        Debug.Log("GameManager: CurrentUserId cleared");
    }

    // --Save/Load using SaveSystem--

    //Save the game for the current user, optional saveName
    public bool SaveGame(GameObject player, out string outFileName, string saveName = null)
    {
        
        outFileName = null;

        if(string.IsNullOrEmpty(currentUserId))
        {
            Debug.LogWarning("GameManager: Cannot save — no user logged in.");
            return false;
        }

        if(player == null)
        {
            Debug.LogError("GameManager: Player GameObject is null");
            return false;
        }

        var playerMovement = player.GetComponent<PlayerMovement>();
        var underwaterMovement = player.GetComponent<UnderwaterMovement>();
        var physicalAttack = player.GetComponent<PhysicalAttackController>();
        var magicalAttack = player.GetComponent<MagicalAttackController>();
        var playerStats = player.GetComponent<PlayerStats>() ?? FindFirstObjectByType<PlayerStats>();

        if (!playerMovement || !underwaterMovement || !physicalAttack || !magicalAttack || !playerStats)
        {
            Debug.LogError($"GameManager: Missing required player components - " +
                $"PlayerMovement: {(playerMovement ? "found" : "null")}, " +
                $"UnderwaterMovement: {(underwaterMovement ? "found" : "null")}, " +
                $"PhysicalAttackController: {(physicalAttack ? "found" : "null")}, " +
                $"MagicalAttackController: {(magicalAttack ? "found" : "null")}, " +
                $"PlayerStats: {(playerStats ? $"found on {playerStats.gameObject.name}" : "null")}");
            return false;
        }

        SaveData data = new SaveData
        {
            saveName = saveName,
            sceneName = SceneManager.GetActiveScene().name,
            playerPosition = player.transform.position,
            isRunning = playerMovement.GetComponentInChildren<Animator>().GetBool("IsRunning"),
            isSwimming = underwaterMovement.isSwimming,
            isFloating = underwaterMovement.GetComponentInChildren<Animator>().GetBool("IsFloating"),
            isPhysicalAttacking = physicalAttack.isAttacking,
            physicalComboIndex = physicalAttack.GetComponentInChildren<Animator>().GetInteger("ComboIndex"),
            isMagicalAttacking = magicalAttack.isAttacking,
            magicalComboIndex = magicalAttack.GetComponentInChildren<Animator>().GetInteger("ComboIndex"),
            health = playerStats.currentHealth,
            mana = playerStats.currentMana,
        };

        var ok = SaveSystem.SaveGame(currentUserId, data, out outFileName);
        if (ok) Debug.Log($"GameManager: Game saved as {outFileName}");
        return ok;
    }

    //overload wihtout out param
    public bool SaveGame(GameObject player, string saveName = null)
    {
        return SaveGame(player, out _, saveName);
    }

    //Return whether currentUser has any saves
    public bool HasSavesForCurrentUser()
    {
        if(string.IsNullOrEmpty(currentUserId)) return false;
        var list = SaveSystem.GetSaveMetas(currentUserId);
        return list.Count > 0;
    }

    //Return a list of saves for UI
    public List<SaveMeta> GetSaveListForCurentUser()
    {
        if (string.IsNullOrEmpty(currentUserId)) return new List<SaveMeta>();
        return SaveSystem.GetSaveMetas(currentUserId);
    }

    //Load a save file (not yet applied to the player). Use LoadAndSaveStart to actually load the scene and apply it.
    public SaveData LoadSaveFileByName(string fileName)
    {
        if(String.IsNullOrEmpty(currentUserId)) return null;
        return SaveSystem.LoadSave(currentUserId, fileName);
    }

    //Start loading the save's scene and apply the save (applied after scene loads)
    public void LoadSaveAndStart(string fileName)
    {
        if(string.IsNullOrEmpty(currentUserId))
        {
            Debug.LogWarning("GameManager: cannot load — no user logged in.");
            return;
        }

        var save = SaveSystem.LoadSave(currentUserId, fileName);
        if(save == null)
        {
            Debug.LogWarning($"GameManager: save '{fileName}' not found or invalid.");
            return;
        }

        pendingSaveData = save;
        SceneManager.LoadScene(save.sceneName);
    }

    public bool DeleteSave(string fileName)
    {
        if (string.IsNullOrEmpty(currentUserId)) return false;
        return SaveSystem.DeleteSave(currentUserId, fileName);
    }

    //Try loadPlayerState - same as original routine, but also reset attack controllers after loading
    public void LoadPlayerStateOnSceneLoad(SaveData saveData)
    {
        pendingSaveData = saveData;
    }

    private IEnumerator TryLoadPlayerState(SaveData saveData)
    {
        const int maxRetries = 10;
        const float retryDelay = 0.1f;
        for (int i = 0; i < maxRetries; i++)
        {
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                var playerStats = playerObj.GetComponent<PlayerStats>();
                if (playerStats != null)
                {
                    playerStats.LoadState(saveData);
                    // reset attack controllers to avoid immediate attack on load
                    var physical = playerObj.GetComponent<PhysicalAttackController>();
                    physical?.ResetAttackState();

                    var magical = playerObj.GetComponent<MagicalAttackController>();
                    // optional: ensure magical has ResetAttackState method too
                    magical?.SendMessage("ResetAttackState", SendMessageOptions.DontRequireReceiver);

                    Debug.Log("GameManager: Player state loaded successfully");
                    yield break;
                }
                else
                {
                    Debug.LogWarning($"GameManager: Player found but PlayerStats component missing (attempt {i + 1}/{maxRetries})");
                }
            }
            else
            {
                Debug.LogWarning($"GameManager: Player not found in loaded scene (attempt {i + 1}/{maxRetries})");
            }
            yield return new WaitForSecondsRealtime(retryDelay);
        }
        Debug.LogError("GameManager: Failed to find Player with PlayerStats after retries");
    }
}