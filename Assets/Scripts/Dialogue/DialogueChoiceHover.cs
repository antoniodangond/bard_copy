using UnityEngine;
using UnityEngine.EventSystems;

public class DialogueChoiceHover : MonoBehaviour, IPointerEnterHandler
{
    [SerializeField] private bool isYes; // set this in the Inspector

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (DialogueChoiceUI.Instance == null) return;

        if (isYes)
            DialogueChoiceUI.Instance.HoverYes();
        else
            DialogueChoiceUI.Instance.HoverNo();
    }
}
