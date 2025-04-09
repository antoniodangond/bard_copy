using UnityEngine;
using UnityEngine.UI;

public class ImageDuplicator : MonoBehaviour
{
    public Image originalImage; // Assign the original image in the Inspector

    public void DuplicateImage()
    {
        if (originalImage == null)
        {
            Debug.LogError("Original image is not assigned!");
            return;
        }

        // Create a new GameObject for the duplicated image
        GameObject newImageObject = new GameObject("DuplicatedImage");

        // Add an Image component to the new GameObject
        Image newImage = newImageObject.AddComponent<Image>();

        // Copy the properties from the original image to the new one
        newImage.sprite = originalImage.sprite;
        newImage.color = originalImage.color;
        newImage.type = originalImage.type;
        newImage.preserveAspect = originalImage.preserveAspect;

        // Set the parent of the new image to the Canvas (or another UI element)
        newImageObject.transform.SetParent(originalImage.transform.parent, false);

        // Copy the RectTransform properties
        RectTransform originalRect = originalImage.GetComponent<RectTransform>();
        RectTransform newRect = newImageObject.GetComponent<RectTransform>();

        newRect.anchorMin = originalRect.anchorMin;
        newRect.anchorMax = originalRect.anchorMax;
        newRect.pivot = originalRect.pivot;
        newRect.sizeDelta = originalRect.sizeDelta;
        newRect.anchoredPosition = originalRect.anchoredPosition;
        newRect.localScale = originalRect.localScale;
        newRect.localRotation = originalRect.localRotation;
    }
}