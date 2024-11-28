using UnityEngine;

public class RegionBoundary : MonoBehaviour
{
    // private BoxCollider2D boxCollider;
    public LayerMask playerLayer;
    public string region;
    public GameObject BackgroundAudioObj;

    private BackgroundAudio backgroundAudio;

    void Awake()
    {
        backgroundAudio = BackgroundAudioObj.GetComponent<BackgroundAudio>();
    }

    // Update is called once per frame
    void OnTriggerEnter2D(Collider2D other)
    {
        if (Utils.HasTargetLayer(playerLayer, other.gameObject))
        {
            Debug.Log("enter trigger " + gameObject.tag);
            backgroundAudio.ChangeBackgroundMusic(gameObject.tag);
        }

    }
    void OnTriggerExit2D(Collider2D other)
    {
        if (Utils.HasTargetLayer(playerLayer, other.gameObject))
        {
            Debug.Log("exit trigger " + gameObject.tag);
        }
    }
}
