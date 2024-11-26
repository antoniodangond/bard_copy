using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;


public class QuitGame : MonoBehaviour
{
    public Button quitGame;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        quitGame = gameObject.GetComponent<Button>();
        quitGame.onClick.AddListener(ExitGame);
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
