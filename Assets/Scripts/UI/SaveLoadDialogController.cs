using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

public class SaveLoadDialogController : MonoBehaviour
{
    private UiManager uiManager;
    private VisualElement dialogRoot;
    private ListView saveListView;
    private Button loadButton;
    private Button deleteButton;
    private Button cancelButton;
    private List<SaveMeta> saveMetas = new List<SaveMeta>();
    private int selectedIndex = -1;

    public void Initialize(VisualElement root)
    {
        // Get uiManager via reflection (matching your login setup)
        uiManager = (UiManager)GetType().GetField("uiManager", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(this);
        if (uiManager == null)
        {
            Debug.LogError("SaveLoadDialogController: UiManager is null");
            return;
        }

        dialogRoot = root;
        saveListView = root.Q<ListView>("SaveList");
        loadButton = root.Q<Button>("LoadButton");
        deleteButton = root.Q<Button>("DeleteButton");
        cancelButton = root.Q<Button>("CancelButton");

        if (saveListView == null || loadButton == null || deleteButton == null || cancelButton == null)
        {
            Debug.LogError("SaveLoadDialogController: Failed to find one or more UI elements (SaveList, LoadButton, DeleteButton, CancelButton)");
            return;
        }

        // Setup ListView
        saveListView.makeItem = () =>
        {
            var label = new Label();
            label.style.unityTextAlign = TextAnchor.MiddleLeft;
            label.style.paddingLeft = 10;
            label.style.paddingRight = 10;
            return label;
        };
        saveListView.bindItem = (element, index) =>
        {
            var meta = saveMetas[index];
            var label = element as Label;
            var timestamp = System.DateTime.Parse(meta.timestamp).ToLocalTime().ToString("g"); // Format: "9/22/2025 10:32 AM"
            label.text = $"{(string.IsNullOrEmpty(meta.saveName) ? "Untitled" : meta.saveName)} - {timestamp} ({meta.sceneName}) - HP: {meta.health:F1}, MP: {meta.mana:F1}";
        };
        saveListView.itemsSource = saveMetas;
        saveListView.selectionChanged += selections =>
        {
            selectedIndex = saveListView.selectedIndex;
            UpdateButtons();
            Debug.Log($"SaveLoadDialogController: Selected save index: {selectedIndex}");
        };

        // Setup buttons
        loadButton.clicked += OnLoadClicked;
        deleteButton.clicked += OnDeleteClicked;
        cancelButton.clicked += () =>
        {
            Debug.Log("SaveLoadDialogController: Cancel clicked");
            uiManager.HideSaveLoadDialog();
        };

        UpdateButtons();
        RefreshSaveList();
    }

    public void RefreshSaveList()
    {
        if (GameManager.Instance == null || string.IsNullOrEmpty(GameManager.Instance.currentUserId))
        {
            saveMetas.Clear();
            Debug.LogWarning("SaveLoadDialogController: No user logged in - Clearing save list");
            saveListView.itemsSource = saveMetas;
            saveListView.makeItem = () => new Label { text = "No user logged in. Please log in." };
            saveListView.bindItem = (element, _) => { (element as Label).text = "No user logged in. Please log in."; };
            saveListView.Rebuild();
            selectedIndex = -1;
            UpdateButtons();
            return;
        }

        saveMetas = GameManager.Instance.GetSaveListForCurentUser();
        Debug.Log($"SaveLoadDialogController: Loaded {saveMetas.Count} saves for user {GameManager.Instance.currentUserId}");
        if (saveMetas.Count == 0)
        {
            saveListView.makeItem = () => new Label { text = "No saves found for this user." };
            saveListView.bindItem = (element, _) => { (element as Label).text = "No saves found for this user."; };
        }
        saveListView.itemsSource = saveMetas;
        saveListView.Rebuild();
        selectedIndex = -1;
        UpdateButtons();
    }

    private void UpdateButtons()
    {
        bool hasSelection = selectedIndex >= 0 && selectedIndex < saveMetas.Count;
        loadButton.SetEnabled(hasSelection);
        deleteButton.SetEnabled(hasSelection);
    }

    private void OnLoadClicked()
    {
        if (selectedIndex >= 0 && selectedIndex < saveMetas.Count)
        {
            var selectedSave = saveMetas[selectedIndex];
            Debug.Log($"SaveLoadDialogController: Loading save: {selectedSave.fileName}");
            GameManager.Instance.LoadSaveAndStart(selectedSave.fileName);
            uiManager.HideSaveLoadDialog();
        }
    }

    private void OnDeleteClicked()
    {
        if (selectedIndex >= 0 && selectedIndex < saveMetas.Count)
        {
            var selectedSave = saveMetas[selectedIndex];
            if (GameManager.Instance.DeleteSave(selectedSave.fileName))
            {
                Debug.Log($"SaveLoadDialogController: Deleted save: {selectedSave.fileName}");
                RefreshSaveList();
            }
            else
            {
                Debug.LogError($"SaveLoadDialogController: Failed to delete save: {selectedSave.fileName}");
            }
        }
    }
}