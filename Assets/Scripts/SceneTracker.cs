using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTracker : MonoBehaviour
{
    public static SceneTracker Instance { get; private set; }
    public bool CameFromStartMenu { get; set; } = false;

    void Awake()
    {
        // Ensure there's only one instance
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist between scenes
        }
        else
        {
            Destroy(gameObject);
        }
        // Scene currentScene = SceneManager.GetActiveScene();
        if (SceneManager.GetActiveScene().buildIndex == 0)
        {
            SetCameFromStartMenu();
        }
        else
        {
            ResetTracker();
        }
    }

    public void SetCameFromStartMenu()
    {
        CameFromStartMenu = true;
    }

    public void ResetTracker()
    {
        CameFromStartMenu = false;
    }
}
