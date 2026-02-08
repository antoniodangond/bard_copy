using UnityEngine;

public class SongInputRowSwap : MonoBehaviour
{
    [SerializeField] private GameObject keyboardRow;   // KeyboardInputs1
    [SerializeField] private GameObject controllerRow; // ControllerInputs1

    public void ShowKeyboard()
    {
        if (keyboardRow) keyboardRow.SetActive(true);
        if (controllerRow) controllerRow.SetActive(false);
    }

    public void ShowController()
    {
        if (keyboardRow) keyboardRow.SetActive(false);
        if (controllerRow) controllerRow.SetActive(true);
    }
}
