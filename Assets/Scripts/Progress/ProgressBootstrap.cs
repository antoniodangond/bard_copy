using UnityEngine;

public class ProgressBootstrap : MonoBehaviour
{
    public GameObject gameManager;
    void Awake()
    {
        if (PlayerProgress.Instance == null)
        {
            var go = new GameObject();
            // go.AddComponent<GameManager>(); // becomes DontDestroyOnLoad automatically
            go = Instantiate(gameManager, new Vector3(0,0,0), Quaternion.identity);
        }
    }
}
