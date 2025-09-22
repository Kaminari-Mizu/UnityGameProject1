using UnityEngine;
using UnityEngine.UIElements;
using System;

public class UiManager : MonoBehaviour
{
    [SerializeField] private VisualTreeAsset loginDialogAsset;
    private VisualElement modalOverlay;
    private VisualElement loginDialog;
    private LoginDialogController loginController;
    public Action<VisualElement> OnDialogInitialized { get; set; } // Updated delegate
    [SerializeField] private VisualTreeAsset saveLoadDialogAsset; // Assign 'LoadSaveDialog.uxml' in Inspector
    private VisualElement saveLoadDialog;
    private SaveLoadDialogController saveLoadController;

    public void Initialize(VisualElement root)
    {
        Debug.Log("UiManager: Initializing");
        modalOverlay = root.Q<VisualElement>("modalOverlay");
        if (modalOverlay == null)
        {
            Debug.LogError("UiManager: Failed to find modalOverlay");
            return;
        }

        if (loginDialogAsset == null)
        {
            Debug.LogError("UiManager: LoginDialogAsset is not assigned");
            return;
        }

        // Instantiate LoginDialog.uxml
        loginDialog = loginDialogAsset.Instantiate().Q<VisualElement>("loginDialog");
        if (loginDialog == null)
        {
            Debug.LogError("UiManager: Failed to find loginDialog in instantiated UXML");
            return;
        }

        // Add LoginDialogController to a new GameObject
        GameObject dialogGameObject = new GameObject("LoginDialog");
        loginController = dialogGameObject.AddComponent<LoginDialogController>();
        loginController.GetType().GetField("uiManager", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(loginController, this);

        // Initialize the controller with the dialog root
        loginController.Initialize(loginDialog);

        // Add to root and hide initially
        modalOverlay.Add(loginDialog);
        modalOverlay.style.display = DisplayStyle.None;
        loginDialog.style.display = DisplayStyle.None;

        // Notify subscribers that initialization is complete
        Debug.Log("UiManager: Invoking OnDialogInitialized");
        OnDialogInitialized?.Invoke(loginDialog);

        // Initialize Save/Load Dialog
        if (saveLoadDialogAsset == null)
        {
            Debug.LogWarning("UiManager: SaveLoadDialogAsset is not assigned");
            return;
        }
        saveLoadDialog = saveLoadDialogAsset.Instantiate().Q<VisualElement>("saveLoadDialog");
        if (saveLoadDialog == null)
        {
            Debug.LogError("UiManager: Failed to find saveLoadDialog in instantiated UXML");
            return;
        }
        GameObject saveDialogGameObject = new GameObject("SaveLoadDialog");
        saveLoadController = saveDialogGameObject.AddComponent<SaveLoadDialogController>();
        // Set uiManager via reflection (matching your login setup)
        saveLoadController.GetType().GetField("uiManager", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(saveLoadController, this);
        saveLoadController.Initialize(saveLoadDialog);
        modalOverlay.Add(saveLoadDialog);
        saveLoadDialog.style.display = DisplayStyle.None;
        Debug.Log("UiManager: Save/Load dialog initialized");
    }

    public void ShowLoginDialog()
    {
        if (modalOverlay == null || loginDialog == null)
        {
            Debug.LogError("UiManager: Cannot show login dialog - modalOverlay or loginDialog is null");
            return;
        }
        Debug.Log("UiManager: Showing login dialog");
        modalOverlay.style.display = DisplayStyle.Flex;
        loginDialog.style.display = DisplayStyle.Flex;
    }

    public void HideLoginDialog()
    {
        if (modalOverlay == null || loginDialog == null)
        {
            Debug.LogError("UiManager: Cannot hide login dialog - modalOverlay or loginDialog is null");
            return;
        }
        Debug.Log("UiManager: Closing login dialog");
        modalOverlay.style.display = DisplayStyle.None;
        loginDialog.style.display = DisplayStyle.None;
    }

    public void OnLoginSuccess(string userName)
    {
        Debug.Log($"Login successful for user: {userName}");
        // Update UI, e.g., show welcome message or switch to game scene
        // Example: Show a welcome label
        //var welcomeLabel = // Find VisualElement for welcome message
        //if (welcomeLabel != null)
        //    welcomeLabel.text = $"Welcome, {userName}!";
    }

    public void ShowSaveLoadDialog()
    {
        if (modalOverlay == null || saveLoadDialog == null || saveLoadController == null)
        {
            Debug.LogError("UiManager: Cannot show save/load dialog - modalOverlay, saveLoadDialog, or saveLoadController is null");
            return;
        }
        Debug.Log("UiManager: Showing save/load dialog");
        modalOverlay.style.display = DisplayStyle.Flex;
        saveLoadDialog.style.display = DisplayStyle.Flex;
        loginDialog.style.display = DisplayStyle.None; // Ensure login is hidden
        saveLoadController.RefreshSaveList(); // Refresh list when shown
    }

    public void HideSaveLoadDialog()
    {
        if (modalOverlay == null || saveLoadDialog == null)
        {
            Debug.LogError("UiManager: Cannot hide save/load dialog - modalOverlay or saveLoadDialog is null");
            return;
        }
        Debug.Log("UiManager: Closing save/load dialog");
        modalOverlay.style.display = DisplayStyle.None;
        saveLoadDialog.style.display = DisplayStyle.None;
    }
}