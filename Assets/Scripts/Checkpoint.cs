using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public LayerMask PlayerLayer;

    // NOTE: This script's game object should have a trigger collider
    void OnTriggerEnter2D(Collider2D other)
    {
        if (Utils.HasTargetLayer(PlayerLayer, other.gameObject))
        {
            Debug.Log("Setting player checkpoint: " + gameObject.name);
            SetPlayerCheckpoint(other.gameObject);
        }
    }

    private void SetPlayerCheckpoint(GameObject player)
    {
        PlayerController playerController = player.GetComponent<PlayerController>();
        playerController.LastCheckpoint = this;
    }

    // TeleportPlayer sets the player's position to this checkpoint's position
    public void TeleportPlayer(GameObject player)
    {
        Debug.Log("Teleporting player from current position " + player.transform.position + " to checkpoint position " + transform.position);
        player.transform.position = transform.position;
    }
}
