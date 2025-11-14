using UnityEngine;

public class ProgressBootstrap : MonoBehaviour
{
    void Awake()
    {
        if (PlayerProgress.Instance == null)
        {
            var go = new GameObject("PlayerProgress");
            go.AddComponent<PlayerProgress>(); // becomes DontDestroyOnLoad automatically
        }
    }
}
