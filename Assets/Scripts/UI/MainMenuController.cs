using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private UiManager uiManager;

    void Start()
    {
        // Get UIDocument and root
        var uiDocument = GetComponent<UIDocument>();
        if (uiDocument == null)
        {
            Debug.LogError("MainMenuController: UIDocument component is missing!");
            return;
        }

        var root = uiDocument.rootVisualElement;
        if (root == null)
        {
            Debug.LogError("MainMenuController: Root VisualElement is null!");
            return;
        }

        // Initialize UiManager
        if (uiManager == null)
        {
            Debug.LogError("MainMenuController: UiManager is not assigned in Inspector!");
            return;
        }
        try
        {
            uiManager.Initialize(root);
            Debug.Log("MainMenuController: UiManager initialized successfully");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"MainMenuController: UiManager.Initialize failed: {ex}");
            return;
        }

        // Query buttons
        var startButton = root.Q<Button>("StartButton");
        var loadButton = root.Q<Button>("LoadButton");
        var loginButton = root.Q<Button>("LoginButton");
        var quitButton = root.Q<Button>("QuitButton");

        // Log button query results
        if (startButton == null) Debug.LogError("MainMenuController: StartButton not found in UXML!");
        else Debug.Log("MainMenuController: StartButton found");
        if (loadButton == null) Debug.LogWarning("MainMenuController: LoadButton not found in UXML!");
        else Debug.Log("MainMenuController: LoadButton found");
        if (loginButton == null) Debug.LogError("MainMenuController: LoginButton not found in UXML!");
        else Debug.Log("MainMenuController: LoginButton found");
        if (quitButton == null) Debug.LogError("MainMenuController: QuitButton not found in UXML!");
        else Debug.Log("MainMenuController: QuitButton found");

        // Start New Game
        if (startButton != null)
        {
            startButton.clicked += () =>
            {
                Debug.Log("MainMenuController: StartButton clicked - Starting new game");
                SceneManager.LoadScene("Island");
            };
        }

        // Load Game (quickfix: loads latest save)
        if (loadButton != null)
        {
            loadButton.clicked += () =>
            {
                Debug.Log("MainMenuController: LoadButton clicked");
                if (GameManager.Instance == null || string.IsNullOrEmpty(GameManager.Instance.currentUserId))
                {
                    Debug.LogWarning("MainMenuController: No user logged in - Showing login dialog");
                    uiManager.ShowLoginDialog();
                    return;
                }

                if (!GameManager.Instance.HasSavesForCurrentUser())
                {
                    Debug.LogWarning("MainMenuController: No save files available for current user.");
                    // TODO: Show UI message (e.g., "No saves found")
                    return;
                }

                var saveMetas = GameManager.Instance.GetSaveListForCurentUser();
                if (saveMetas.Count > 0)
                {
                    var latestSaveMeta = saveMetas[0]; // Latest due to timestamp sorting
                    Debug.Log($"MainMenuController: Loading save: {latestSaveMeta.fileName}");
                    GameManager.Instance.LoadSaveAndStart(latestSaveMeta.fileName);
                }
            };
        }

        // Login
        if (loginButton != null)
        {
            loginButton.clicked += () =>
            {
                Debug.Log("MainMenuController: LoginButton clicked");
                uiManager.ShowLoginDialog();
            };
        }

        // Quit
        if (quitButton != null)
        {
            quitButton.clicked += () =>
            {
                Debug.Log("MainMenuController: QuitButton clicked");
                Application.Quit();
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false; // For testing in Editor
#endif
            };
        }
    }
}