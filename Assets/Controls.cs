using UnityEngine;
using UnityEngine.UI;

public class Controls : MonoBehaviour
{
    public Button controls;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        controls = gameObject.GetComponent<Button>();
        controls.onClick.AddListener(Clicked);
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void Clicked()
    {
        OpenControlsScreen();
    }
    
    void OpenControlsScreen()
    {
        MenuManager.Instance.MenuUI.SetActive(false);
        MenuManager.Instance.ControlsMenuUI.SetActive(true);
    }
}
