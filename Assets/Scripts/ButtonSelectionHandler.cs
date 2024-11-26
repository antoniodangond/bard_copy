using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

public class ButtonSelectionHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{

    private void Start()
    {
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // select the button
        eventData.selectedObject = gameObject;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // deselect the button
        if (eventData.selectedObject == gameObject)
        {
            eventData.selectedObject = null;
        }
    }

    public void OnSelect(BaseEventData eventData)
    {
         if (MenuManager.Instance != null)
        {
            if (gameObject.activeSelf)
            {
                MenuManager.Instance.TriggerButtonAnimation(gameObject, true); // Starting animation
            }
        }
    }

    public void OnDeselect(BaseEventData eventData)
    {
         if (MenuManager.Instance != null)
        {
            if (gameObject.activeSelf)
            {
                MenuManager.Instance.TriggerButtonAnimation(gameObject, false); // Ending animation
            }
        }
    }
}
