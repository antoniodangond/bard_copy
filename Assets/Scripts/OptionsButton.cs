using UnityEngine;
using UnityEngine.UI;

public class OptionsMenuScript : MonoBehaviour
{
    public Button options;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        options = gameObject.GetComponent<Button>();
        options.onClick.AddListener(Clicked);
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OpenOptionsMenu()
    {
        MenuManager.Instance.MenuUI.SetActive(false);
        MenuManager.Instance.OptionsMenuUI.SetActive(true);
    }
    
    void Clicked()
    {
        OpenOptionsMenu();
    }
}
