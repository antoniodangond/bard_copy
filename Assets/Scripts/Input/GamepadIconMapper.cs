using UnityEngine;

public class GamepadIconMapper
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
    
}