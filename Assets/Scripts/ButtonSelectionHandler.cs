using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonSelectionHandler :
    MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    [Header("Targets (recommended)")]
    [Tooltip("Child to animate (move/scale). Use the Visual container under the layout parent.")]
    public RectTransform animTarget;           // e.g., YesVisual / NoVisual
    [Tooltip("Image to tint and/or swap sprite on select. E.g., YesButton / NoButton (fills Visual).")]
    public Image tintImage;                    // the gray box image

    [Header("Audio (optional)")]
    public bool playSoundOnSelect = true;
    public AudioSource audioSource;

    [Header("Tint (optional)")]
    public bool useTint = true;
    public Color selectedTint = new Color32(0x4E, 0x7E, 0xA8, 0xFF);
    public bool restoreOriginalTintOnDeselect = true;

    [Header("Sprite Swap (optional)")]
    public bool useSpriteSwap = false;
    [Tooltip("Sprite used when selected (e.g., artist-provided selected fill).")]
    public Sprite selectedSprite;
    public bool restoreOriginalSpriteOnDeselect = true;

    [Header("Frame Overlay (optional)")]
    public bool useFrame = false;
    [Tooltip("Direct reference to a frame image to show/hide on select (placed under animTarget).")]
    public Image frameImage;                   // optional, if already in the prefab
    [Tooltip("If provided and frameImage is empty, we'll instantiate this under animTarget at runtime.")]
    public Image framePrefab;                  // optional, for auto-instantiation
    [Tooltip("Extra size around the target (in px).")]
    public Vector2 framePadding = new Vector2(6, 6);
    [Tooltip("Render frame above the button visuals (true) or below (false).")]
    public bool frameAbove = true;
    [Tooltip("Set Image.type = Sliced on the frame if using a 9-sliced sprite.")]
    public bool frameIsSliced = true;
    public Color frameColorSelected = Color.white;
    public Color frameColorDeselected = new Color(1, 1, 1, 0f);   // transparent when deselected

    [Header("Bump Animation (MenuManager)")]
    public bool allowBump = true;

    // --- internals ---
    private bool isSelectedVisual;
    private Color originalTint;
    private Sprite originalSprite;
    private RectTransform targetRect;     // the rect we'll size from (tintImage or animTarget)
    private GameObject bumpTargetGO;      // which object the MenuManager will animate

    void Awake()
    {
        // Auto-wire basics
        if (!audioSource) audioSource = GetComponent<AudioSource>();
        if (!animTarget) animTarget = transform as RectTransform;

        // Prefer the tintImage’s RectTransform if available, else animTarget
        if (!tintImage)
        {
            tintImage = animTarget ? animTarget.GetComponent<Image>() : GetComponentInChildren<Image>(true);
            if (!tintImage)
                tintImage = GetComponentInChildren<Image>(true);
        }
        targetRect = tintImage ? tintImage.rectTransform : animTarget;
        bumpTargetGO = animTarget ? animTarget.gameObject : gameObject;

        if (tintImage)
        {
            originalTint = tintImage.color;
            originalSprite = tintImage.sprite;
        }

        // Setup the frame if requested
        if (useFrame)
        {
            EnsureFrameExists();
            if (frameImage)
            {
                var img = frameImage;
                if (frameIsSliced) img.type = Image.Type.Sliced;
                // start hidden/transparent
                img.color = frameColorDeselected;
                img.gameObject.SetActive(false);
            }
        }
    }

    // Create the frame dynamically if needed
    private void EnsureFrameExists()
    {
        if (frameImage) return;

        if (framePrefab)
        {
            // Instantiate under animTarget, stretch, and keep disabled initially
            var parent = animTarget ? animTarget : (transform as RectTransform);
            var spawned = Instantiate(framePrefab, parent);
            frameImage = spawned;
            var rt = frameImage.rectTransform;
            StretchToFill(rt);
            if (!frameAbove) rt.SetAsFirstSibling(); else rt.SetAsLastSibling();
            frameImage.gameObject.SetActive(false);
        }
        else
        {
            // No prefab given; create a minimal Image frame if you still want a simple outline sprite later.
            // (You can assign the sprite via inspector on runtime object if desired.)
            var parent = animTarget ? animTarget : (transform as RectTransform);
            var go = new GameObject("SelectionFrame", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            frameImage = go.GetComponent<Image>();
            var rt = frameImage.rectTransform;
            StretchToFill(rt);
            if (!frameAbove) rt.SetAsFirstSibling(); else rt.SetAsLastSibling();
            frameImage.raycastTarget = false;
            frameImage.gameObject.SetActive(false);
        }
    }

    // Stretch a RectTransform to fill its parent with zero offsets
    private static void StretchToFill(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = Vector2.zero;
        rt.localScale = Vector3.one;
    }

    public void SetSelected(bool selected, bool playSound = false)
    {
        if (selected == isSelectedVisual) return;
        isSelectedVisual = selected;

        // TINT
        if (useTint && tintImage)
        {
            tintImage.color = selected ? selectedTint : (restoreOriginalTintOnDeselect ? originalTint : tintImage.color);
        }

        // SPRITE SWAP
        if (useSpriteSwap && tintImage)
        {
            if (selected && selectedSprite)
                tintImage.sprite = selectedSprite;
            else if (!selected && restoreOriginalSpriteOnDeselect)
                tintImage.sprite = originalSprite;
        }

        // FRAME
        if (useFrame && frameImage)
        {
            if (selected)
            {
                frameImage.gameObject.SetActive(true);
                frameImage.color = frameColorSelected;

                // Size the frame to the target with padding
                var target = targetRect ? targetRect : (transform as RectTransform);
                // We’ll use sizeDelta adjustment since anchors are stretched.
                // Get current target size in parent space:
                var size = target.rect.size;
                frameImage.rectTransform.sizeDelta = new Vector2(size.x + framePadding.x * 2f, size.y + framePadding.y * 2f);
                frameImage.rectTransform.anchoredPosition = Vector2.zero; // centered over target
            }
            else
            {
                frameImage.color = frameColorDeselected;
                frameImage.gameObject.SetActive(false);
            }
        }

        // SOUND
        if (playSound && selected && audioSource != null && playSoundOnSelect)
        {
            audioSource.volume = 0.3f;
            audioSource.Play();
        }

        // BUMP (animate the child, not the layout parent)
        if (allowBump && MenuManager.Instance != null && bumpTargetGO != null)
        {
            MenuManager.Instance.TriggerButtonAnimation(bumpTargetGO, startingAnimation: selected);
        }
    }

    // Pointer / EventSystem
    public void OnPointerEnter(PointerEventData e)
    {
        SetSelected(true, playSound: true);
        EventSystem.current?.SetSelectedGameObject(gameObject);
    }
    public void OnPointerExit(PointerEventData e)
    {
        if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject == gameObject) return;
        SetSelected(false, playSound: false);
    }
    public void OnSelect(BaseEventData e) => SetSelected(true, playSound: false);
    public void OnDeselect(BaseEventData e) => SetSelected(false, playSound: false);
}
