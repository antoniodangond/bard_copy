using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;

public class MainMenu : MonoBehaviour
{
    [Header("Buttons")]
    public Button ContinueButton;
    public Button NewGameButton;
    public Button QuitButton;

    [Header("New Game Warning UI")]
    public GameObject newGameWarningPanel;
    public Button confirmNewGameYesButton;
    public Button confirmNewGameNoButton;

    [Header("Config")]
    public string newGameSceneName = "Game_Start";

    void OnEnable()
    {
        RefreshButtons();
    }

    void Start()
    {
        RefreshButtons();
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.gameCompleted == true)
        {
            StartCoroutine(GameManager.Instance.backgroundAudio.ChangeBackgroundMusic("MainMenu"));
            GameManager.Instance.gameCompleted = false;
        }
    }

    void RefreshButtons()
    {
        bool hasSave = HasValidSave();

        // Hide continue if no save
        if (ContinueButton != null)
            ContinueButton.gameObject.SetActive(hasSave);

        if (newGameWarningPanel != null)
            newGameWarningPanel.SetActive(false);

        // Clear listeners
        if (ContinueButton != null) ContinueButton.onClick.RemoveAllListeners();
        if (NewGameButton != null)  NewGameButton.onClick.RemoveAllListeners();
        if (QuitButton != null)     QuitButton.onClick.RemoveAllListeners();
        if (confirmNewGameYesButton != null) confirmNewGameYesButton.onClick.RemoveAllListeners();
        if (confirmNewGameNoButton != null)  confirmNewGameNoButton.onClick.RemoveAllListeners();

        // Hook main buttons
        if (ContinueButton != null && hasSave)
            ContinueButton.onClick.AddListener(OnContinue);

        if (NewGameButton != null)
            NewGameButton.onClick.AddListener(() => OnNewGame(hasSave));

        if (QuitButton != null)
            QuitButton.onClick.AddListener(OnQuit);

        // Hook warning buttons
        if (confirmNewGameYesButton != null)
            confirmNewGameYesButton.onClick.AddListener(OnConfirmNewGameYes);

        if (confirmNewGameNoButton != null)
            confirmNewGameNoButton.onClick.AddListener(OnConfirmNewGameNo);

        // Initial focus
        if (EventSystem.current != null)
        {
            GameObject first = null;
            if (hasSave && ContinueButton != null && ContinueButton.gameObject.activeSelf)
                first = ContinueButton.gameObject;
            else if (NewGameButton != null)
                first = NewGameButton.gameObject;

            if (first != null)
                EventSystem.current.SetSelectedGameObject(first);
        }
    }

    bool HasValidSave()
    {
        string path = Path.Combine(Application.persistentDataPath, "savefile.json");
        if (!File.Exists(path)) return false;

        try
        {
            var json = File.ReadAllText(path);
            if (string.IsNullOrWhiteSpace(json)) return false;

            var data = JsonUtility.FromJson<SaveData>(json);
            if (data == null) return false;

            bool hasScene = !string.IsNullOrEmpty(data.currentScene);
            bool hasAnyProgress =
                (data.upgrades?.Count ?? 0) > 0 ||
                (data.savedSongs?.Count ?? 0) > 0 ||
                (data.defeatedEnemies?.Count ?? 0) > 0; //||
                // data.numTabletsCollected > 0;

            return hasScene || hasAnyProgress;
        }
        catch
        {
            return false;
        }
    }

    // ---------- NEW HELPERS (no recursion!) ----------

    void SetMainButtonsInteractable(bool state)
    {
        if (ContinueButton != null) ContinueButton.interactable = state;
        if (NewGameButton != null)  NewGameButton.interactable = state;
        if (QuitButton != null)     QuitButton.interactable = state;
    }

    void ShowNewGameWarning()
    {
        if (newGameWarningPanel != null)
            newGameWarningPanel.SetActive(true);

        SetMainButtonsInteractable(false);

        if (EventSystem.current != null && confirmNewGameYesButton != null)
            EventSystem.current.SetSelectedGameObject(confirmNewGameYesButton.gameObject);
    }

    void HideNewGameWarning()
    {
        if (newGameWarningPanel != null)
            newGameWarningPanel.SetActive(false);

        SetMainButtonsInteractable(true);

        if (EventSystem.current != null && NewGameButton != null)
            EventSystem.current.SetSelectedGameObject(NewGameButton.gameObject);
    }

    // ---------- BUTTON HANDLERS ----------

    void OnContinue()
    {
        var pp = PlayerProgress.Instance;
        string scene = (pp != null) ? pp.GetSavedSceneName() : null;
        if (string.IsNullOrEmpty(scene)) scene = newGameSceneName;
        SceneManager.LoadScene(scene);
    }

    void OnNewGame(bool hasSave)
    {
        if (hasSave && newGameWarningPanel != null)
        {
            ShowNewGameWarning();
        }
        else
        {
            StartNewGameImmediately();
        }
    }

    void StartNewGameImmediately()
    {
        // 1. Reset global stuff that can lock movement
        Time.timeScale = 1f;

        // If you have a global game state manager that can lock controls,
        // reset it here too, e.g.:
        // GameState.Instance?.SetPaused(false);
        // GameState.Instance?.SetInputLocked(false);

        // 2. Clear save data
        if (PlayerProgress.Instance != null)
        {
            PlayerProgress.Instance.ClearAll();
            GameManager.Instance.ResetGameProgress();
        }

        // 3. Load fresh overworld scene
        SceneManager.LoadScene(newGameSceneName);
    }



    void OnConfirmNewGameYes()
    {
        // optional: HideNewGameWarning(); scene is changing anyway
        PlayerProgress.Instance?.ClearAll();
        GameManager.Instance.ResetGameProgress();
        SceneManager.LoadScene(newGameSceneName);
    }

    void OnConfirmNewGameNo()
    {
        HideNewGameWarning();
    }

    void OnQuit()
    {
    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
    #else
        Application.Quit();
    #endif
    }
}
