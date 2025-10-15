// Assets/Scripts/Input/InputDisplayUtil.cs
using UnityEngine;
using UnityEngine.InputSystem;

public static class InputDisplayUtil
{
    public static string GetBindingForAction(string actionMap, string action, PlayerInput playerInput = null)
    {
        if (playerInput == null)
            playerInput = Object.FindFirstObjectByType<PlayerInput>();
        if (playerInput == null) return "?";

        // Pick binding slot by scheme (adjust names to match your asset)
        string scheme = playerInput.currentControlScheme ?? "";
        int bindingIndex = scheme.Contains("Gamepad") ? 1 : 0; // 0: KBM, 1: Gamepad in your project

        var map = playerInput.actions.FindActionMap(actionMap);
        var act = map?.FindAction(action);
        if (act == null) return "?";

        return act.GetBindingDisplayString(bindingIndex);
    }
}
