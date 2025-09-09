using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
    private UIDocument uiDocument;
    private VisualElement pauseMenu;
    private bool isPaused = false;

    [SerializeField] private GameObject player; // Assign in Inspector

    void Start()
    {
        uiDocument = GetComponent<UIDocument>();
        pauseMenu = uiDocument.rootVisualElement.Q<VisualElement>("pauseMenu");
        if (pauseMenu == null)
        {
            Debug.LogError("PauseMenuController: Failed to find 'pauseMenu' element in PauseMenu.uxml");
        }
        else
        {
            pauseMenu.style.display = DisplayStyle.None; // Hide initially
        }

        if (player == null)
        {
            Debug.LogError("PauseMenuController: Player GameObject is not assigned");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Toggle_pause();
        }
    }

    void Toggle_pause()
    {
        isPaused = !isPaused;
        if (isPaused)
        {
            ShowPauseMenu();
        }
        else
        {
            HidePauseMenu();
        }
    }

    void ShowPauseMenu()
    {
        if (pauseMenu != null)
        {
            pauseMenu.style.display = DisplayStyle.Flex;
            Time.timeScale = 0f;
            SetupButtonHandlers();
        }
    }

    void HidePauseMenu()
    {
        if (pauseMenu != null)
        {
            pauseMenu.style.display = DisplayStyle.None;
            Time.timeScale = 1f;
        }
    }

    void SetupButtonHandlers()
    {
        var resumeButton = uiDocument.rootVisualElement.Q<Button>("resumeButton");
        if (resumeButton != null)
        {
            resumeButton.clicked += HidePauseMenu;
        }
        else
        {
            Debug.LogError("PauseMenuController: Failed to find 'resumeButton' in PauseMenu.uxml");
        }

        var saveButton = uiDocument.rootVisualElement.Q<Button>("saveButton");
        if (saveButton != null)
        {
            saveButton.clicked += () =>
            {
                if (GameManager.Instance == null)
                {
                    Debug.LogError("PauseMenuController: GameManager.Instance is null");
                    return;
                }
                if (player == null)
                {
                    Debug.LogError("PauseMenuController: Player GameObject is null");
                    return;
                }
                string saveName = $"Save_{System.DateTime.Now:yyyyMMdd_HHmmss}"; // Unique save name
                if (GameManager.Instance.SaveGame(player, out string fileName, saveName))
                {
                    Debug.Log($"PauseMenuController: Saved game successfully as {fileName}");
                    // TODO: Show UI feedback (e.g., "Game Saved!")
                }
                else
                {
                    Debug.LogError("PauseMenuController: Failed to save game");
                    // TODO: Show UI error (e.g., "Save failed - please log in")
                }
            };
        }
        else
        {
            Debug.LogError("PauseMenuController: Failed to find 'saveButton' in PauseMenu.uxml");
        }

        var quitButton = uiDocument.rootVisualElement.Q<Button>("quitButton");
        if (quitButton != null)
        {
            quitButton.clicked += () =>
            {
                Time.timeScale = 1f;
                SceneManager.LoadScene("MainMenu");
            };
        }
        else
        {
            Debug.LogError("PauseMenuController: Failed to find 'quitButton' in PauseMenu.uxml");
        }
    }
}