using UnityEngine;
using UnityEngine.InputSystem;

public class ControlsMapper
{
    public string MapGamepadIcons(string bindingDisplayString)
    {
        switch (bindingDisplayString)
        {
            case "Cross":
                return "<sprite name=\"Playstation_Cross\">";
            case "Circle":
                return "<sprite name=\"Playstation_Circle\">";
            case "Triangle":
                return "<sprite name=\"Playstation_Triangle\">";
            case "Square":
                return "<sprite name=\"Playstation_Square\">";
            case "R1":
                return "<sprite name=\"Playstation_R1\">";
            case "Options":
                return "<sprite name=\"Playstation_Options\">";
            case "A":
                return "<sprite name=\"XBox_A\">";
            case "B":
                return "<sprite name=\"XBox_B\">";
            case "X":
                return "<sprite name=\"XBox_X\">";
            case "Y":
                return "<sprite name=\"XBox_Y\">";
            case "RB":
                return "<sprite name=\"XBox_RB\">";
            case "Menu":
                return "<sprite name=\"XBox_Menu\">";
            default:
                return "";
        }
    }

    public string getCorrectButton(string actionMap, string action, string controlScheme, bool isAttackAction, PlayerInput playerInput)
    {
        if (controlScheme == "Keyboard")
        {
            switch(actionMap)
            {
                case "Player":
                    if (isAttackAction == false) {return playerInput.actions.FindActionMap(actionMap).FindAction(action).GetBindingDisplayString(0);}
                    else 
                    {
                        // since there are 2 mappings for attacks, we concatenate them and handle the split elsewhere
                        // (because this function returns a single string)
                        return 
                            playerInput.actions.FindActionMap("Player").FindAction("Attack").GetBindingDisplayString(0) 
                            + "," 
                            + playerInput.actions.FindActionMap("Player").FindAction("Attack").GetBindingDisplayString(1);
                    }
                case "Instrument":
                    return playerInput.actions.FindActionMap(actionMap).FindAction(action).GetBindingDisplayString(0);
                default:
                    return "";
            }
        }
        else
        {
            switch (actionMap)
            {
                case "Player":
                    if (isAttackAction == false) {return MapGamepadIcons(playerInput.actions.FindActionMap(actionMap).FindAction(action).GetBindingDisplayString(1));}
                    else {return MapGamepadIcons(playerInput.actions.FindActionMap("Player").FindAction("Attack").GetBindingDisplayString(2));}
                case "Instrument":
                    return playerInput.actions.FindActionMap(actionMap).FindAction(action).GetBindingDisplayString(1);
                default:
                    return "";
            }
        }
    }

    public string setLyreButtons(bool gamepadActive, string[] notes, PlayerInput playerInput)
    {
        string lyreButtons = "";

        for (int i = 0; i < notes.Length; i++)
        {
            if (!gamepadActive)
            {
                if (i == 0)
                {
                    lyreButtons = getCorrectButton("Instrument", notes[i], "Keyboard", false, playerInput);
                }
                else
                {
                    lyreButtons = lyreButtons + ", " + getCorrectButton("Instrument", notes[i], "Keyboard", false, playerInput);
                }
            }
            else
            {
                if (i == 0)
                {
                    lyreButtons = lyreButtons + MapGamepadIcons(getCorrectButton("Instrument", notes[i], "Gamepad", false, playerInput));

                }
                else
                {
                    lyreButtons = lyreButtons + ", " + MapGamepadIcons(getCorrectButton("Instrument", notes[i], "Gamepad", false, playerInput));
                }
            }
        }
        return lyreButtons;
    }
    
}