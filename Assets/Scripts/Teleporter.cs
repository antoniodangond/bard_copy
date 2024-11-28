using UnityEngine;

public class Teleporter : MonoBehaviour
{
    // DestinationTeleporter should be a GameObject with a Teleporter script of its own.
    public GameObject DestinationTeleporter;
    // LandingPoint's transform is where the player is teleported to.
    // NOTE: It should be placed outside of the game object's trigger circle collider, so that
    // the player doesn't get stuck in a teleportation loop between game objects.
    public GameObject LandingPoint;
    public LayerMask PlayerLayer;

    private Teleporter destinationTeleporter;

    void Awake()
    {
        destinationTeleporter = DestinationTeleporter.GetComponent<Teleporter>();
    }

    // NOTE: This script's game object should have a trigger circle collider
    void OnTriggerEnter2D(Collider2D other)
    {
        if (Utils.HasTargetLayer(PlayerLayer, other.gameObject))
        {
            TeleportPlayer(other.gameObject);
        }
    }

    // TeleportPlayer sets the player's position to the DestinationTeleporter's LandingPoint position
    private void TeleportPlayer(GameObject player)
    {
        player.transform.position = destinationTeleporter.LandingPoint.transform.position;
    }
}
