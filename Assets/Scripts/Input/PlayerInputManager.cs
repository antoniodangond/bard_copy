using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using UnityEditor;
// using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;


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
    public const string AOEAttack = "AOEAttack";
    public const string OpenMenu = "OpenMenu";
    public const string Dialogue = "Dialogue";
    public const string Dash = "Dash";
    // Instrument actions
    public const string ToggleInstrument = "ToggleInstrument";
    public const string NoteB = "NoteB";
    public const string NoteC = "NoteC";
    public const string NoteD = "NoteD";
    public const string NoteE = "NoteE";
    // Are we missing a note here??? ^

    // UI actions
    public const string Navigate = "Navigate";
    public const string Submit = "Submit";
    // public const string CloseMenu = "CloseMenu";
}

[RequireComponent(typeof(InputActionAsset))]
public class PlayerInputManager : MonoBehaviour
{
    // make it a singleton to access easier
    public static PlayerInputManager Instance {get; private set; }
    // Giving up and adding pause functionality here
    // public static bool isPaused {get; private set;}
    public bool isPaused;
    // Assigned in editor
    public InputActionAsset InputActionAsset;

    // Public properties for reading captured input
    public static Vector2 Movement;
    public static bool WasAttackPressed;
    public static bool WasAOEAttackPressed;
    public bool MenuOpened { get; private set; }
    // public bool MenuClosed { get; private set; }
    public static bool WasToggleInstrumentPressed;
    public static string NotePressed;
    public static bool WasDialoguePressed;
    public static bool wasDashPressed;
    private bool IsPlayerMapActive =>
        currentActionMap != null && currentActionMap.name == ActionMaps.Player;

    // Input Action Map
    private InputActionMap currentActionMap;

    // Player actions
    private InputAction moveAction;
    private InputAction attackAction;
    private InputAction AOEattackAction;
    private InputAction OpenMenuAction;
    private InputAction dialogueAction;
    private InputAction dashAction;
    // Instrument actions
    private InputAction toggleInstrumentAction;
    private InputAction noteBAction;
    private InputAction noteCAction;
    private InputAction noteDAction;
    private InputAction noteEAction;
    // UI actions
    private InputAction Navigate;
    private InputAction Submit;
    // private InputAction CloseMenuAction;
    [SerializeField] private Button firstPauseButton; // drag your Resume button here
    


    void Awake()
    {
        // Player actions
        InputActionMap playerActionMap = InputActionAsset.FindActionMap(ActionMaps.Player);
        moveAction = playerActionMap.FindAction(Actions.Move);
        attackAction = playerActionMap.FindAction(Actions.Attack);
        AOEattackAction = playerActionMap.FindAction(Actions.AOEAttack);
        OpenMenuAction = playerActionMap.FindAction(Actions.OpenMenu);
        dialogueAction = playerActionMap.FindAction(Actions.Dialogue);
        dashAction = playerActionMap.FindAction(Actions.Dash);
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
        // CloseMenuAction = UIActionMap.FindAction(Actions.CloseMenu);
        SwitchToActionMap("Player");

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
        
    }

    private void OnDisable()
    {
        
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
            MenuManager.Instance.PlayerUIManager.SetActive(false);
            PauseMenu.SetActive(true);
            foreach (var button in MenuManager.Instance.buttons)
            {
                Debug.Log($"Found button: {button.gameObject.name}");
                button.gameObject.SetActive(true);
            }
        MenuManager.Instance.UpdatePauseMenuSongs(); // Update songs each time we open the menu
        MenuManager.Instance.UpdateTabletsCountUI(); // Update tablet count each time we open the menu

        }
        isPaused = true;
        CustomEvents.OnPause?.Invoke(isPaused);
        Time.timeScale = 0f;
        SwitchToActionMap("UI");
        if (EventSystem.current && firstPauseButton)
        EventSystem.current.SetSelectedGameObject(firstPauseButton.gameObject);
        // OpenMainMenu();

    }

    public void Unpause()
    {
        if (MenuManager.Instance != null)
        {
            GameObject PauseMenu = MenuManager.Instance.MenuUI;
            PauseMenu.SetActive(false);
            MenuManager.Instance.PlayerUIManager.SetActive(true);
        }
        isPaused = false;
        CustomEvents.OnUnPause?.Invoke(isPaused);
        Time.timeScale = 1f;
        SwitchToActionMap("Player");
        if (EventSystem.current)
            EventSystem.current.SetSelectedGameObject(null);
        // Debug.Log(MenuManager.Instance);
        // CloseAllMenus();
    }

    void Update()
{
    // Pause toggle
    MenuOpened = OpenMenuAction.WasPressedThisFrame();
    if (MenuOpened) HandleMenuOpen();

    if (IsPlayerMapActive && !isPaused)
    {
        wasDashPressed          = dashAction.WasPressedThisFrame();
        Movement                = moveAction.ReadValue<Vector2>();
        WasAttackPressed        = attackAction.WasPressedThisFrame();
        WasAOEAttackPressed     = AOEattackAction.WasPressedThisFrame();
        WasToggleInstrumentPressed = toggleInstrumentAction.WasPressedThisFrame();
        HandleNotePress();
        WasDialoguePressed      = dialogueAction.WasPressedThisFrame();
    }
    else
    {
        // hard zero gameplay inputs when not in Player map
        wasDashPressed = false;
        Movement = Vector2.zero;
        WasAttackPressed = false;
        WasAOEAttackPressed = false;
        WasToggleInstrumentPressed = false;
        NotePressed = null;
        WasDialoguePressed = false;
    }
}



    
    public static string GetButtonForNote(string note)
{
    string controlScheme = UnityEngine.InputSystem.Gamepad.current == null ? "Keyboard" : "Gamepad";
    int bindingIndex = controlScheme == "Keyboard" ? 0 : 1;
    var actionMap = Instance.InputActionAsset.FindActionMap("Instrument");
    var action = actionMap.FindAction(note);
    if (action != null)
        return action.GetBindingDisplayString(bindingIndex);
    else
        return note;
}
}

