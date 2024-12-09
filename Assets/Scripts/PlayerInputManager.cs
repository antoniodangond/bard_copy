using System.Runtime.CompilerServices;
using UnityEditor;
// using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.InputSystem;

static class ActionMaps
{
    // PlayerInput action maps
    public const string Player = "Player";
    public const string Instrument = "Instrument";
    public const string UI = "UI";
}

static class Actions
{
    // Player actions
    public const string Move = "Move";
    public const string Attack = "Attack";
    public const string OpenMenu = "OpenMenu";
    public const string Dialogue = "Dialogue";
    // Instrument actions
    public const string ToggleInstrument = "ToggleInstrument";
    public const string NoteB = "NoteB";
    public const string NoteC = "NoteC";
    public const string NoteD = "NoteD";
    public const string NoteE = "NoteE";
    // UI actions
    public const string Navigate = "Navigate";
    public const string Submit = "Submit";
    public const string CloseMenu = "CloseMenu";
}

[RequireComponent(typeof(InputActionAsset))]
public class PlayerInputManager : MonoBehaviour
{
    // make it a singleton to access easier
    public static PlayerInputManager Instance {get; private set; }
    // Giving up and adding pause functionality here
    public static bool isPaused {get; private set;}
    // Assigned in editor
    public InputActionAsset InputActionAsset;

    // Public properties for reading captured input
    public static Vector2 Movement;
    public static bool WasAttackPressed;
    public static bool MenuOpened { get; private set; }
    public static bool MenuClosed { get; private set; }
    public static bool WasToggleInstrumentPressed;
    public static string NotePressed;
    public static bool WasDialoguePressed;

    // Input Action Map
    private InputActionMap currentActionMap;

    // Player actions
    private InputAction moveAction;
    private InputAction attackAction;
    private InputAction OpenMenuAction;
    private InputAction dialogueAction;
    // Instrument actions
    private InputAction toggleInstrumentAction;
    private InputAction noteBAction;
    private InputAction noteCAction;
    private InputAction noteDAction;
    private InputAction noteEAction;
    // UI actions
    private InputAction Navigate;
    private InputAction Submit;
    private InputAction CloseMenuAction;

    void Awake()
    {
        // Player actions
        InputActionMap playerActionMap = InputActionAsset.FindActionMap(ActionMaps.Player);
        moveAction = playerActionMap.FindAction(Actions.Move);
        attackAction = playerActionMap.FindAction(Actions.Attack);
        OpenMenuAction = playerActionMap.FindAction(Actions.OpenMenu);
        dialogueAction = playerActionMap.FindAction(Actions.Dialogue);
        // Instrument actions
        InputActionMap instrumentActionMap = InputActionAsset.FindActionMap(ActionMaps.Instrument);
        toggleInstrumentAction = instrumentActionMap.FindAction(Actions.ToggleInstrument);
        noteBAction = instrumentActionMap.FindAction(Actions.NoteB);
        noteCAction = instrumentActionMap.FindAction(Actions.NoteC);
        noteDAction = instrumentActionMap.FindAction(Actions.NoteD);
        noteEAction = instrumentActionMap.FindAction(Actions.NoteE);
        // UI actions
        InputActionMap UIActionMap = InputActionAsset.FindActionMap(ActionMaps.UI);
        Navigate = UIActionMap.FindAction(Actions.Navigate);
        Submit = UIActionMap.FindAction(Actions.Submit);
        CloseMenuAction = UIActionMap.FindAction(Actions.CloseMenu);

    if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);  // Destroy duplicate instances
        }

    }

    private void OnEnable()
    {
        // Enable actions to ensure they can read values
        moveAction.Enable();
        OpenMenuAction.Enable();
        toggleInstrumentAction.Enable();
        noteBAction.Enable();
        noteCAction.Enable();
        noteDAction.Enable();
        noteEAction.Enable();
        Navigate.Enable();
        Submit.Enable();
        CloseMenuAction.Enable();
    }

    private void OnDisable()
    {
        // Disable actions to prevent unnecessary updates when the gameObject is inactive
        moveAction.Disable();
        OpenMenuAction.Disable();
        toggleInstrumentAction.Disable();
        noteBAction.Disable();
        noteCAction.Disable();
        noteDAction.Disable();
        noteEAction.Disable();
        Navigate.Disable();
        Submit.Disable();
        CloseMenuAction.Disable();
    }

    void HandleNotePress()
    {
        if (noteBAction.WasPressedThisFrame())
        {
            NotePressed = Actions.NoteB;
        }
        else if (noteCAction.WasPressedThisFrame())
        {
            NotePressed = Actions.NoteC;
        }
        else if (noteDAction.WasPressedThisFrame())
        {
            NotePressed = Actions.NoteD;
        }
        else if (noteEAction.WasPressedThisFrame())
        {
            NotePressed = Actions.NoteE;
        } else {
            NotePressed = null;
        }
    }

    public void HandleMenuOpen()
    {
        // TODO: handle opening/closing other menus
        if(isPaused)
        {
            Unpause();
        }
        else
        {
            Pause();
        }
    }
    public void SwitchToActionMap(string mapName)
    {
        // If there is a current action map, disable it
        if (currentActionMap != null)
        {
            currentActionMap.Disable();
        }

        // Find the new action map by name
        currentActionMap = InputActionAsset.FindActionMap(mapName);

        // If the action map exists, enable it
        if (currentActionMap != null)
        {
            currentActionMap.Enable();
            // Debug.Log("Switched to action map: " + mapName);
        }
        else
        {
            // Debug.LogError("Action map not found: " + mapName);
        }
    }

    public void Pause()
    {
        if (MenuManager.Instance != null)
        {
            GameObject PauseMenu = MenuManager.Instance.MenuUI;
            PauseMenu.SetActive(true);
            foreach (var button in MenuManager.Instance.buttons)
            {
                // Debug.Log($"Found button: {button.gameObject.name}");
                button.gameObject.SetActive(true);
            }
        }
        isPaused = true;
        CustomEvents.OnPause?.Invoke(isPaused);
        Time.timeScale = 0f;
        SwitchToActionMap("UI");
        // OpenMainMenu();
    }

    public void Unpause()
    {
        if (MenuManager.Instance != null)
        {
            GameObject PauseMenu = MenuManager.Instance.MenuUI;
            PauseMenu.SetActive(false);
        }
        isPaused = false;
        CustomEvents.OnUnPause?.Invoke(isPaused);
        Time.timeScale = 1f;
        SwitchToActionMap("Player");
        // Debug.Log(MenuManager.Instance);
        // CloseAllMenus();
    }

    void Update()
    {
        // Move action composite mode should be set to "digital" to prevent diagonal
        // movement magnitude from being less than 1
        Movement = moveAction.ReadValue<Vector2>();
        WasAttackPressed = attackAction.WasPressedThisFrame();
        MenuOpened = OpenMenuAction.WasPressedThisFrame();
        MenuClosed = CloseMenuAction.WasPressedThisFrame();
        if(MenuOpened || MenuClosed)
        {
            HandleMenuOpen();
        }
        WasToggleInstrumentPressed = toggleInstrumentAction.WasPressedThisFrame();
        HandleNotePress();
        WasDialoguePressed = dialogueAction.WasPressedThisFrame();
    }
}

