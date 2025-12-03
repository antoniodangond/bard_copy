using Unity.VisualScripting;
using UnityEngine;

public class EurydiceGrave : MonoBehaviour
{
    public Sprite winStateSpriteOne;
    public Sprite winStateSpriteTwo;
    public Sprite winStateSpriteThree;
    private int songsPlayed;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private ParticleSystem glowingParticles;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        HandleGameCompleteProgression();
    }

    public void OnSongPlayed(string melody)
    {
        switch (melody)
        {
            case MelodyData.Melody1:
                songsPlayed += 1;
                Debug.Log(songsPlayed + " songs played");
                break;

            case MelodyData.Melody2:
                songsPlayed += 1;
                Debug.Log(songsPlayed + " songs played");
                break;

            case MelodyData.Melody3:
                songsPlayed += 1;
                Debug.Log(songsPlayed + " songs played");
                break;
            default:
                break;
        }
    }

    private void HandleGameCompleteProgression()
    {
        switch (songsPlayed)
        {
            case 1:
                spriteRenderer.sprite = winStateSpriteOne;
                break;
            case 2:
                spriteRenderer.sprite = winStateSpriteTwo;
                break;
            case 3:
                spriteRenderer.sprite = winStateSpriteThree;
                break;
            default:
                break;
        }
    }
}
