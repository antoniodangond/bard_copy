using Unity.VisualScripting;
using UnityEngine;

public class AreaCollider : MonoBehaviour
{
    private BoxCollider2D boxCollider;
    public LayerMask playerLayer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();        
    }

    // Update is called once per frame
    void OnTriggerEnter2D(Collider2D other)
    {   
        if (Utils.HasTargetLayer(playerLayer, other.gameObject))
        {
            // TODO: Change Ambience and Music
            //play the correct music and ambience
            Debug.Log("enter trigger");
        }
        
    }
    void OnTriggerExit2D(Collider2D other)
    {
        if (Utils.HasTargetLayer(playerLayer, other.gameObject))
        {
            Debug.Log("exit trigger");
            // TODO: Change Ambience and Music
            //play the correct music and ambience
        }
    }
}
