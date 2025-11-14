using UnityEngine;

public class SaveGameButton : MonoBehaviour
{

    [SerializeField] private GameObject saveToast; // small Game Saved popup


    // Called by the button's OnClick event in the Inspector
    public void SaveGame()
    {
        // If you track position/health dynamically, update before saving
        var player = FindObjectOfType<PlayerController>();
        if (player)
            PlayerProgress.Instance.SetPlayerPosition(player.transform.position);

        PlayerProgress.Instance.SaveNow();
        if (saveToast) saveToast.SetActive(true);
        Debug.Log("[SaveGameButton] Game saved!");
    }
}
