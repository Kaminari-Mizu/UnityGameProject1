using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Networking;
using System.Text;
using System.Collections;

[System.Serializable]
public class LoginRequest
{
    public string username;
    public string password;
}

[System.Serializable]
public class User
{
    public string userId;
    public string userName;
}

[System.Serializable]
public class LoginResponse
{
    public string token;
    public User user;
}

public class LoginDialogController : MonoBehaviour
{
    [SerializeField] private UiManager uiManager;
    private TextField usernameField;
    private TextField passwordField;
    private Button submitButton;
    private Button cancelButton;
    private Label errorLabel;

    public string EnteredUsername { get; private set; }
    public string EnteredPassword { get; private set; }

    private const string BackendUrl = "https://localhost:7000/api/auth/login"; // Adjust as needed

    public void Initialize(VisualElement dialogRoot)
    {
        if (uiManager == null)
        {
            Debug.LogError("LoginDialogController: UIManager is not assigned");
            return;
        }

        usernameField = dialogRoot.Q<TextField>("usernameField");
        passwordField = dialogRoot.Q<TextField>("passwordField");
        submitButton = dialogRoot.Q<Button>("Submit");
        cancelButton = dialogRoot.Q<Button>("Cancel");
        errorLabel = dialogRoot.Q<Label>("errorLabel");

        if (usernameField == null || passwordField == null || submitButton == null || cancelButton == null)
        {
            Debug.LogError($"LoginDialogController: Failed to find UI elements - usernameField: {(usernameField == null ? "null" : "found")}, passwordField: {(passwordField == null ? "null" : "found")}, submitButton: {(submitButton == null ? "null" : "found")}, cancelButton: {(cancelButton == null ? "null" : "found")}");
            return;
        }

        if (errorLabel == null)
        {
            Debug.LogWarning("LoginDialogController: errorLabel not found, errors will be logged to console only");
        }

        passwordField.isPasswordField = true;
        Debug.Log("LoginDialogController: Registering button events");
        submitButton.clicked += OnSubmit;
        cancelButton.clicked += OnCancel;
    }

    private void OnSubmit()
    {
        Debug.Log("LoginDialogController: Submit button clicked");
        EnteredUsername = usernameField.value;
        EnteredPassword = passwordField.value;
        Debug.Log($"LoginDialogController: Attempting login with Username={EnteredUsername}");

        if (string.IsNullOrEmpty(EnteredUsername) || string.IsNullOrEmpty(EnteredPassword))
        {
            DisplayError("Username and password are required");
            return;
        }

        StartCoroutine(LoginCoroutine());
    }

    private IEnumerator LoginCoroutine()
    {
        var loginRequest = new LoginRequest
        {
            username = EnteredUsername,
            password = EnteredPassword
        };
        string json = JsonUtility.ToJson(loginRequest);
        byte[] body = Encoding.UTF8.GetBytes(json);

        using (var request = new UnityWebRequest(BackendUrl, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(body);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            Debug.Log("LoginDialogController: Sending login request to " + BackendUrl);
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                string errorMessage = "Login failed";
                if (request.responseCode == 400)
                {
                    var errorResponse = JsonUtility.FromJson<ErrorResponse>(request.downloadHandler.text);
                    errorMessage = errorResponse?.message ?? "Invalid credentials";
                }
                else
                {
                    errorMessage = $"Login error: {request.error} (Code: {request.responseCode})";
                }
                Debug.LogError($"LoginDialogController: {errorMessage}");
                DisplayError(errorMessage);
                yield break;
            }

            var response = JsonUtility.FromJson<LoginResponse>(request.downloadHandler.text);
            Debug.Log($"LoginDialogController: Login successful: Token={response.token}, UserId={response.user.userId}, UserName={response.user.userName}");

            // Store credentials in PlayerPrefs
            PlayerPrefs.SetString("token", response.token);
            PlayerPrefs.SetString("userId", response.user.userId);
            PlayerPrefs.SetString("userName", response.user.userName);
            PlayerPrefs.Save();

            // Set GameManager current user
            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetCurrentUser(response.user.userId);
                Debug.Log($"LoginDialogController: Set GameManager.currentUserId to {response.user.userId}");
            }
            else
            {
                Debug.LogError("LoginDialogController: GameManager.Instance is null during login");
            }

            uiManager.HideLoginDialog();
            uiManager.OnLoginSuccess(response.user.userName);
        }
    }

    private void OnCancel()
    {
        Debug.Log("LoginDialogController: Cancel button clicked");
        uiManager.HideLoginDialog();
    }

    private void DisplayError(string message)
    {
        if (errorLabel != null)
        {
            errorLabel.text = message;
            errorLabel.style.display = DisplayStyle.Flex;
        }
        else
        {
            Debug.LogError($"LoginDialogController: Error: {message}");
        }
    }

    void OnDestroy()
    {
        if (submitButton != null) submitButton.clicked -= OnSubmit;
        if (cancelButton != null) cancelButton.clicked -= OnCancel;
    }
}

[System.Serializable]
public class ErrorResponse
{
    public string message;
}