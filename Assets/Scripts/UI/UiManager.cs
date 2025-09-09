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
}