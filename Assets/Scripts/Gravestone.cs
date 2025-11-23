using UnityEngine;

public class Gravestone : MonoBehaviour
{
    public float TotalDistance;
    public float MoveSpeed;
    public GameObject TeleporterObj;
    public EnvironmentAudioData audioData;
    public GraveAudio graveAudio;

    private Rigidbody2D rb;
    private float originalTransformY;
    private float distanceMoved = 0f;
    private Teleporter teleporter;
    private AudioMixerScript audioMixerScript;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        audioMixerScript = GetComponent<AudioMixerScript>();
        originalTransformY = transform.position.y;
        teleporter = TeleporterObj.GetComponent<Teleporter>();
        graveAudio = gameObject.AddComponent<GraveAudio>();
        graveAudio.audioData = audioData;
        graveAudio.graveAudioSource = gameObject.AddComponent<AudioSource>();
        audioMixerScript.assignSFXGroup(graveAudio.graveAudioSource);
    }

    public void Move()
    {
        if (distanceMoved < TotalDistance)
        {
            rb.linearVelocityY = MoveSpeed;
            distanceMoved += transform.position.y - originalTransformY;
            if (!graveAudio.graveAudioSource.isPlaying)
            {
                graveAudio.PlayGravePushSound();
            }
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
