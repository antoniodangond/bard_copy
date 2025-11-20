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
        // Same philosophy as your SignController / GetLyreButtons:
        // if any gamepad is connected, show gamepad icons.
        return Gamepad.current != null ? "Gamepad" : "Keyboard";
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

        // Match the logic we already use in SignController.handleTutorialDialog
        bool gamepadActive = Gamepad.current != null;

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

    public static string GetOpenMenuBinding(PlayerInput playerInput = null)
    {
        playerInput = GetPlayerInput(playerInput);
        if (playerInput == null)
        {
            Debug.LogWarning("[InputDisplayUtil] GetOpenMenuBinding: no PlayerInput found.");
            return "?";
        }

        var map = playerInput.actions.FindActionMap("Player");
        if (map == null)
        {
            Debug.LogWarning("[InputDisplayUtil] GetOpenMenuBinding: action map 'Player' not found.");
            return "?";
        }

        var act = map.FindAction("OpenMenu");
        if (act == null)
        {
            Debug.LogWarning("[InputDisplayUtil] GetOpenMenuBinding: action 'OpenMenu' not found in 'Player' map.");
            return "?";
        }

        bool gamepadActive = Gamepad.current != null;
        int bindingIndex = -1;

        if (gamepadActive)
        {
            // look for Gamepad group
            for (int i = 0; i < act.bindings.Count; i++)
            {
                var groups = act.bindings[i].groups;
                if (!string.IsNullOrEmpty(groups) && groups.Contains("Gamepad"))
                {
                    bindingIndex = i;
                    break;
                }
            }
        }
        else
        {
            // look for keyboard group
            for (int i = 0; i < act.bindings.Count; i++)
            {
                var groups = act.bindings[i].groups;
                if (!string.IsNullOrEmpty(groups) &&
                    (groups.Contains("Keyboard&Mouse") || groups.Contains("Keyboard")))
                {
                    bindingIndex = i;
                    break;
                }
            }
        }

        if (bindingIndex < 0)
            bindingIndex = 0;

        string raw = act.GetBindingDisplayString(bindingIndex);
        Debug.Log($"[InputDisplayUtil] GetOpenMenuBinding gamepadActive={gamepadActive} index={bindingIndex} raw='{raw}'");

        return gamepadActive ? mapper.MapGamepadIcons(raw) : raw;
    }


}
