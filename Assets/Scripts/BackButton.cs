using UnityEngine;
using UnityEngine.UI;

public class BackButton : MonoBehaviour
{
    public Button back;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        back = gameObject.GetComponent<Button>();
        back.onClick.AddListener(Clicked);
    }

    // Update is called once per frame
    void Update()
    {

    }
    
    void Clicked()
    {
        ReturnToPauseScreen();
    }
    
    void ReturnToPauseScreen()
    {
        MenuManager.Instance.MenuUI.SetActive(true);
        MenuManager.Instance.OptionsMenuUI.SetActive(false);
        MenuManager.Instance.ControlsMenuUI.SetActive(false);
    }
}
