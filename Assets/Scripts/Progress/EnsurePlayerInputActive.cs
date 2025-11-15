using UnityEngine;
using UnityEngine.InputSystem;

public class EnsurePlayerInputActive : MonoBehaviour
{
    void Start()
    {
        var pi = GetComponent<PlayerInput>();
        if (pi != null)
        {
            pi.enabled = true;

            // Replace "Player" with your gameplay action map name if different
            if (pi.currentActionMap == null || pi.currentActionMap.name != "Player")
            {
                pi.SwitchCurrentActionMap("Player");
            }
        }

        // Safety: make sure time is running on scene load
        Time.timeScale = 1f;
    }
}
