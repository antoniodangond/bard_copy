using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonSelectionHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    private Color originalColor;
    private Color selectedColor = new Color32(0x4E, 0x7E, 0xA8, 0xFF);

    private Image buttonImage;
    private AudioSource audioSource;
    private void Start()
    {
        buttonImage = GetComponent<Image>();
        audioSource = GetComponent<AudioSource>();

        // Store the original color
        if (buttonImage != null)
        {
            originalColor = buttonImage.color;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // select the button
        eventData.selectedObject = gameObject;
        audioSource.volume = 0.3f;
        Debug.Log(audioSource.volume)
;        audioSource.Play();
        buttonImage.color = selectedColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // deselect the button
        if (eventData.selectedObject == gameObject)
        {
            buttonImage.color = originalColor;
            eventData.selectedObject = null;
        }
    }

    public void OnSelect(BaseEventData eventData)
    {
         if (MenuManager.Instance != null)
        {
            if (gameObject.activeSelf)
            {
                // MenuManager.Instance.TriggerButtonAnimation(gameObject, true); // Starting animation
            }
        }
    }

    public void OnDeselect(BaseEventData eventData)
    {
         if (MenuManager.Instance != null)
        {
            if (gameObject.activeSelf)
            {
                // MenuManager.Instance.TriggerButtonAnimation(gameObject, false); // Ending animation
            }
        }
    }
}
