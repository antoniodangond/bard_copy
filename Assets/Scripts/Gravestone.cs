using UnityEngine;

public class Gravestone : MonoBehaviour
{
    public float TotalDistance;
    public float MoveSpeed;
    public GameObject TeleporterObj;

    private Rigidbody2D rb;
    private float originalTransformY;
    private float distanceMoved = 0f;
    private Teleporter teleporter;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        originalTransformY = transform.position.y;
        teleporter = TeleporterObj.GetComponent<Teleporter>();
    }

    public void Move()
    {
        if (distanceMoved < TotalDistance)
        {
            rb.linearVelocityY = MoveSpeed;
            distanceMoved += transform.position.y - originalTransformY;
        }
        else {
            Stop();
            // Activate the teleporter after the gravestone has been pushed it's TotalDistance
            teleporter.Activate();
        }
    }

    public void Stop()
    {
        rb.linearVelocityY = 0f;
    }
}
