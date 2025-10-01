using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonSelectionHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    [Header("Colors")]
    [SerializeField] private Color selectedColor = new Color32(0x4E, 0x7E, 0xA8, 0xFF);

    private Color originalColor;
    private Image buttonImage;
    private AudioSource audioSource;
    private bool isSelectedVisual; // internal state to avoid double work

    void Awake()
    {
        buttonImage = GetComponent<Image>();
        audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        if (buttonImage != null)
            originalColor = buttonImage.color;
    }

    /// <summary>
    /// Programmatically set the selected visual state (color+sfx+menu animation).
    /// </summary>
    public void SetSelected(bool selected, bool playSound = false)
    {
        if (buttonImage == null) return;

        if (selected == isSelectedVisual) return; // no-op if already in that state
        isSelectedVisual = selected;

        // Color
        buttonImage.color = selected ? selectedColor : originalColor;

        // Sound (optional)
        if (playSound && selected && audioSource != null)
        {
            audioSource.volume = 0.3f;
            audioSource.Play();
        }

        // Bump/scale via MenuManager (if present)
        if (MenuManager.Instance != null)
        {
            MenuManager.Instance.TriggerButtonAnimation(gameObject, startingAnimation: selected);
        }
    }

    // ----- Pointer / EventSystem hooks (mouse & gamepad UI) -----

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Visually select on hover + sfx
        SetSelected(true, playSound: true);

        // Make this the currently selected UI object for keyboard/controller nav
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(gameObject);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Only clear the visual if this object is no longer the selected one
        if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject == gameObject)
            return;

        SetSelected(false, playSound: false);
    }

    public void OnSelect(BaseEventData eventData)
    {
        // Selected via keyboard/controller navigation
        SetSelected(true, playSound: false);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        SetSelected(false, playSound: false);
    }
}
