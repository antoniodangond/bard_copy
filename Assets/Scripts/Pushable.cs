using UnityEngine;

public class Pushable : MonoBehaviour
{
    public float TotalDistance;
    public float MoveSpeed;

    private Rigidbody2D rb;
    private float originalTransformY;
    private float distanceMoved = 0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        originalTransformY = transform.position.y;
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
        }
    }

    public void Stop()
    {
        rb.linearVelocityY = 0f;
    }
}
