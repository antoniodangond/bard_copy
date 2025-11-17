using UnityEngine;
using UnityEngine.InputSystem;

public static class InputDisplayUtil
{
    private static ControlsMapper mapper = new ControlsMapper();

    private static PlayerInput GetPlayerInput(PlayerInput playerInput = null)
    {
        if (playerInput != null) return playerInput;
        return Object.FindFirstObjectByType<PlayerInput>();
    }

    public static bool IsGamepad(PlayerInput playerInput = null)
    {
        playerInput = GetPlayerInput(playerInput);
        if (playerInput == null) return false;

        string scheme = playerInput.currentControlScheme ?? "";
        return scheme.Contains("Gamepad");
    }

    private static string GetSchemeString(PlayerInput playerInput = null)
    {
        return IsGamepad(playerInput) ? "Gamepad" : "Keyboard";
    }

    /// <summary>
    /// Uses ControlsMapper.getCorrectButton with the current scheme.
    /// </summary>
    public static string GetActionDisplay(
        string actionMap,
        string action,
        bool isAttackAction = false,
        PlayerInput playerInput = null)
    {
        playerInput = GetPlayerInput(playerInput);
        if (playerInput == null) return "?";

        string scheme = GetSchemeString(playerInput);
        return mapper.getCorrectButton(actionMap, action, scheme, isAttackAction, playerInput);
    }

    /// <summary>
    /// Attack buttons: (primary, secondary). Secondary is empty for gamepad.
    /// </summary>
    public static (string primary, string secondary) GetAttackButtons(PlayerInput playerInput = null)
    {
        playerInput = GetPlayerInput(playerInput);
        if (playerInput == null) return ("?", "");

        string scheme = GetSchemeString(playerInput);
        string combined = mapper.getCorrectButton("Player", "Attack", scheme, true, playerInput);

        if (scheme == "Keyboard")
        {
            var parts = combined.Split(',');
            string p1 = parts.Length > 0 ? parts[0] : "";
            string p2 = parts.Length > 1 ? parts[1] : "";
            return (p1, p2);
        }
        else
        {
            // On gamepad, getCorrectButton already returns icon string
            return (combined, "");
        }
    }

    /// <summary>
    /// AOE buttons: (primary, secondary). Secondary is likely empty on gamepad.
    /// </summary>
    public static (string primary, string secondary) GetAOEButtons(PlayerInput playerInput = null)
    {
        playerInput = GetPlayerInput(playerInput);
        if (playerInput == null) return ("?", "");

        string scheme = GetSchemeString(playerInput);
        string combined = mapper.getCorrectButton("Player", "AOEAttack", scheme, true, playerInput);

        if (scheme == "Keyboard")
        {
            var parts = combined.Split(',');
            string p1 = parts.Length > 0 ? parts[0] : "";
            string p2 = parts.Length > 1 ? parts[1] : "";
            return (p1, p2);
        }
        else
        {
            return (combined, "");
        }
    }

    /// <summary>
    /// Lyre note string (comma-separated). Uses ControlsMapper.setLyreButtons.
    /// </summary>
    public static string GetLyreButtons(string[] notes, PlayerInput playerInput = null)
    {
        playerInput = GetPlayerInput(playerInput);
        if (playerInput == null) return "?";

        bool gamepadActive = IsGamepad(playerInput);
        return mapper.setLyreButtons(gamepadActive, notes, playerInput);
    }

    /// <summary>
    /// Generic binding fetch, then mapped to icons on gamepad.
    /// Good for UI / menu actions not covered by ControlsMapper.
    /// </summary>
    public static string GetUIBindingWithIcons(
        string actionMap,
        string action,
        PlayerInput playerInput = null)
    {
        playerInput = GetPlayerInput(playerInput);
        if (playerInput == null) return "?";

        string scheme = playerInput.currentControlScheme ?? "";
        int bindingIndex = scheme.Contains("Gamepad") ? 1 : 0;

        var map = playerInput.actions.FindActionMap(actionMap);
        var act = map?.FindAction(action);
        if (act == null) return "?";

        string raw = act.GetBindingDisplayString(bindingIndex);
        if (scheme.Contains("Gamepad"))
        {
            return mapper.MapGamepadIcons(raw);
        }
        else
        {
            return raw;
        }
    }

    // Backwards-compatible wrapper used by DialogueManager.ReplaceControlPlaceholders
    public static string GetBindingForAction(string actionMap, string action, PlayerInput playerInput = null)
    {
        playerInput = GetPlayerInput(playerInput);
        if (playerInput == null) return "?";

        // For Player / Instrument actions, use ControlsMapper logic
        if (actionMap == "Player" || actionMap == "Instrument")
        {
            bool isAttack = action == "Attack" || action == "AOEAttack";
            return GetActionDisplay(actionMap, action, isAttack, playerInput);
        }

        // For UI / other maps, fall back to raw binding + icon mapping
        return GetUIBindingWithIcons(actionMap, action, playerInput);
    }
}
