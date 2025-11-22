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
    public bool ShouldStartActive = true;
    [HideInInspector]
    public Teleporter destinationTeleporter;
    public bool hasBeenEntered = false;
    private bool isActive;
    private GameObject player;

    void Awake()
    {
        if (DestinationTeleporter) {destinationTeleporter = DestinationTeleporter.GetComponent<Teleporter>();}
        isActive = ShouldStartActive;
    }

    // NOTE: This script's game object should have a trigger circle collider
    void OnTriggerEnter2D(Collider2D other)
    {
        if (Utils.HasTargetLayer(PlayerLayer, other.gameObject))
        {
            Debug.Log("Player entered " + gameObject.name);
            if (isActive)
            {
                hasBeenEntered = true;
                TeleportPlayer(other.gameObject);
            }
            else
            {
                player = other.gameObject;
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        player = null;
    }

    // TeleportPlayer sets the player's position to the DestinationTeleporter's LandingPoint position
    private void TeleportPlayer(GameObject player)
    {
        if (destinationTeleporter)
        {
            Debug.Log("Teleporting player from current position " + player.transform.position + " to landing point position " + destinationTeleporter.LandingPoint.transform.position);
            player.transform.position = destinationTeleporter.LandingPoint.transform.position;
        }
    }

    public void Activate()
    {
        isActive = true;
        if (player != null)
        {
            TeleportPlayer(player);
        }
    }
}
