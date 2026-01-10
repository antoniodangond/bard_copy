using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EurydiceGrave : MonoBehaviour
{
    [Header("Settings")]
    public float fadeInTime;
    public bool isDevMode;
    [SerializeField] private LayerMask playerLayer;
    [Header("Grave Pieces")] 
    [SerializeField] private GameObject pieceOne;
    [SerializeField] private GameObject pieceTwo;
    [SerializeField] private GameObject pieceThree;
    [SerializeField] private Sprite completedGrave;
    [SerializeField] private GameObject spriteRendererObject;
    private SpriteRenderer spriteRenderer;
    [Header("Gateway")]
    [SerializeField] private GameObject underworldGateway;
    [SerializeField] private GameObject endGameTransporter;
    [Header("Effects")]
    [SerializeField] private ParticleSystem glowingParticles;
    [Header("Other GameObjects")]
    [SerializeField] private GameObject endGameTrigger;

    [Header("Quest / Song Mapping")]
    [Tooltip("UniqueId.Id of the NPC whose quest is solved with Melody1")]
    [SerializeField] private string questForMelody1Id;
    [Tooltip("UniqueId.Id of the NPC whose quest is solved with Melody2")]
    [SerializeField] private string questForMelody2Id;
    [Tooltip("UniqueId.Id of the NPC whose quest is solved with Melody3")]
    [SerializeField] private string questForMelody3Id;


    private int songsPlayed;
    private BoxCollider2D boxCollider;

    // Track which songs are still available to affect the grave.
    private HashSet<string> remainingMelodies;

    void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        spriteRenderer = spriteRendererObject.GetComponent<SpriteRenderer>();

        remainingMelodies = new HashSet<string>
        {
            MelodyData.Melody1,
            MelodyData.Melody2,
            MelodyData.Melody3
        };

        if (isDevMode)
        {
            ActivateDevMode();
        }
    }

    private void Start()
    {
        ApplySavedStateFromProgress();  // handles case where load already happened
    }

    private bool IsQuestSolvedForMelody(string melody)
    {
        if (PlayerProgress.Instance == null)
            return false;

        string npcId = null;

        if (melody == MelodyData.Melody1)
        {
            npcId = questForMelody1Id;
        }
        else if (melody == MelodyData.Melody2)
        {
            npcId = questForMelody2Id;
        }
        else if (melody == MelodyData.Melody3)
        {
            npcId = questForMelody3Id;
        }

        if (string.IsNullOrEmpty(npcId))
            return false; // nothing configured = treat as locked

        // Uses the existing NPC status system
        return PlayerProgress.Instance.GetNPCStatus(npcId) == "MelodySolved";
    }

    private void ActivateDevMode()
    {
        HandleGameCompleteProgression(1);
        HandleGameCompleteProgression(2);
        HandleGameCompleteProgression(3);
    }

    public void OnSongPlayed(string melody)
    {
        // 1) Ignore anything that isn't one of our three grave songs
        if (!remainingMelodies.Contains(melody))
        {
            // Either not a grave song, or this song has already been used here
            return;
        }

        // 2) Check whether the corresponding quest for this song is solved
        if (!IsQuestSolvedForMelody(melody))
        {
            // Quest not solved yet means this song has no effect but remains available for later once the quest IS solved.
            return;
        }

        // 3) Consume this melody so it can't trigger again
        remainingMelodies.Remove(melody);

        // 4) Persist to PlayerProgress
        PlayerProgress.Instance?.MarkGraveMelodyUsed(melody);

        // 5) Advance visual grave progression
        songsPlayed += 1;
        HandleGameCompleteProgression(songsPlayed);

        // Old approach; save in case i break it
        // switch (melody)
        // {
        //     case MelodyData.Melody1:
        //         songsPlayed += 1;
        //         HandleGameCompleteProgression(songsPlayed);
        //         break;

        //     case MelodyData.Melody2:
        //         songsPlayed += 1;
        //         HandleGameCompleteProgression(songsPlayed);
        //         break;

        //     case MelodyData.Melody3:
        //         songsPlayed += 1;
        //         HandleGameCompleteProgression(songsPlayed);
        //         break;
        //     default:
        //         break;
    }

    private void HandleGameCompleteProgression(int songsPlayed)
    {
        switch (songsPlayed)
        {
            case 1:
                StartCoroutine(FadeInGameObject(pieceOne, fadeInTime));
                break;
            case 2:
                StartCoroutine(FadeInGameObject(pieceTwo, fadeInTime));
                break;
            case 3:
                StartCoroutine(GameCompleteRoutine());
                break;
            default:
                break;
        }
    }

    private IEnumerator GameCompleteRoutine()
    {
        StartCoroutine(FadeInGameObject(pieceThree, fadeInTime));

        yield return new WaitForSeconds(fadeInTime);

        spriteRenderer.sprite = completedGrave;

        DestroyGravePieces();

        yield return new WaitForSeconds(0.5f);

        StartCoroutine(OpenDoorway());

        yield return new WaitForSeconds(fadeInTime);

        Destroy(boxCollider);
    }

    private void DestroyGravePieces()
    {
        Destroy(pieceOne);
        Destroy(pieceTwo);
        Destroy(pieceThree);
    }
    
    private IEnumerator OpenDoorway()
    {
        StartCoroutine(FadeInGameObject(underworldGateway, fadeInTime));

        yield return new WaitForSeconds(0.2f);

        StartCoroutine(MoveGrave(0.8f, fadeInTime));

        yield return new WaitForSeconds(0.8f);

        StartCoroutine(FadeOutSprite(spriteRenderer, fadeInTime));

        endGameTransporter.SetActive(true);
        
    }

    private IEnumerator FadeInGameObject(GameObject currentGameObject, float fadeInTime)
    {
        SpriteRenderer sr = currentGameObject.GetComponent<SpriteRenderer>();
        Color transparent = sr.color;
        Color opaque = transparent;
        opaque.a = 1;
        transparent.a = 0;
        currentGameObject.SetActive(true);

        float elapsedTime = 0;

        while (elapsedTime < fadeInTime)
        {
            float t = elapsedTime/fadeInTime;

            sr.color = Color.Lerp(transparent, opaque, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator FadeOutGameObject(GameObject go, float fadeOutTime)
    {
        SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
        Color transparent = sr.color;
        Color opaque = transparent;
        opaque.a = 1;
        transparent.a = 0;

        float elapsedTime = 0;

        while (elapsedTime < fadeOutTime)
        {
            float t = elapsedTime / fadeOutTime;

            sr.color = Color.Lerp(opaque, transparent, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        Destroy(go);
    }
    private IEnumerator FadeOutSprite(SpriteRenderer sr, float fadeOutTime)
    {
        Color transparent = sr.color;
        Color opaque = transparent;
        opaque.a = 1;
        transparent.a = 0;

        float elapsedTime = 0;

        while (elapsedTime < fadeOutTime)
        {
            float t = elapsedTime / fadeOutTime;

            sr.color = Color.Lerp(opaque, transparent, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator MoveGrave(float distance, float moveTime)
    {
        float elapsedTime = 0;

        Vector3 gravePos = spriteRenderer.transform.localPosition;
        Vector3 graveDest = gravePos + new Vector3(0, distance);

        while (elapsedTime < moveTime)
        {
            float t = elapsedTime / moveTime;

            spriteRenderer.transform.localPosition = Vector3.Lerp(gravePos, graveDest, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (Utils.HasTargetLayer(playerLayer, other.gameObject))
        {
            // Debug.Log("Player entered " + gameObject.name);
            if (GameManager.Instance.allStatuePiecesCollected)
            {
                SceneManager.LoadSceneAsync("Good End");
            }
            else
            {
                SceneManager.LoadSceneAsync("Bad End");
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
    }



    private void OnEnable()
    {
        if (PlayerProgress.Instance != null)
        {
            PlayerProgress.Instance.OnLoaded += ApplySavedStateFromProgress;
        }
    }

    private void OnDisable()
    {
        if (PlayerProgress.Instance != null)
        {
            PlayerProgress.Instance.OnLoaded -= ApplySavedStateFromProgress;
        }
    }

    private void ApplySavedStateFromProgress()
    {
        // Rebuild remainingMelodies and songsPlayed from PlayerProgress
        remainingMelodies = new HashSet<string>
        {
            MelodyData.Melody1,
            MelodyData.Melody2,
            MelodyData.Melody3
        };

        songsPlayed = 0;

        if (PlayerProgress.Instance != null)
        {
            foreach (var m in PlayerProgress.Instance.GetGraveUsedMelodies())
            {
                if (remainingMelodies.Remove(m))
                {
                    songsPlayed++;
                }
            }
        }

        // Reset visuals to a known baseline
        if (pieceOne != null) { pieceOne.SetActive(false); }
        if (pieceTwo != null) { pieceTwo.SetActive(false); }
        if (pieceThree != null) { pieceThree.SetActive(false); }

        if (underworldGateway != null) { underworldGateway.SetActive(false); }
        if (endGameTransporter != null) { endGameTransporter.SetActive(false); }

        // Make sure base sprite is visible by default
        if (spriteRenderer != null)
        {
            var c = spriteRenderer.color;
            c.a = 1f;
            spriteRenderer.color = c;
        }

        // Now apply state according to how many songs have been used:
        if (songsPlayed >= 1)
        {
            InstantShowPiece(pieceOne);
        }

        if (songsPlayed >= 2)
        {
            InstantShowPiece(pieceTwo);
        }

        if (songsPlayed >= 3)
        {
            // Completed grave state
            InstantShowPiece(pieceThree);

            if (spriteRenderer != null && completedGrave != null)
            {
                spriteRenderer.sprite = completedGrave;
            }

            // Match the "doorway opened" end-state (without re-running coroutines)
            if (underworldGateway != null)
            {
                underworldGateway.SetActive(true);
            }

            // Move the grave up like in MoveGrave(distance=0.8f), but instantly
            spriteRenderer.transform.localPosition += new Vector3(0f, 0.8f, 0f);

            // Fade out grave sprite instantly (end of FadeOutSprite)
            if (spriteRenderer != null)
            {
                var c = spriteRenderer.color;
                c.a = 0f;
                spriteRenderer.color = c;
            }

            if (endGameTransporter != null)
            {
                endGameTransporter.SetActive(true);
            }

            // Collider removed after completion
            if (boxCollider != null)
            {
                Destroy(boxCollider);
            }

            // Optional: pieces are destroyed in GameCompleteRoutine; you can either:
            // - destroy them here, or
            // - leave them inactive. Here we mirror the destruction:
            DestroyGravePieces();
        }
    }

    // Helper: instantly show a piece (no fade)
    private void InstantShowPiece(GameObject go)
    {
        if (go == null) return;
        var sr = go.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            var c = sr.color;
            c.a = 1f;
            sr.color = c;
        }
        go.SetActive(true);
    }


}

    