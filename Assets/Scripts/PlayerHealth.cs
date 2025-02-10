using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    public Image heartPrefab; // Assign the heart prefab in the Inspector
    public Transform heartContainer; // Assign a UI panel to hold the hearts
    public int maxHealth = 5; // Maximum health (number of hearts)

    public ImagePool heartPool;

    private void Awake()
    {
        // Initialize the object pool
        heartPool = new ImagePool(heartPrefab, heartContainer, maxHealth);
        
    }

    // Update the UI to reflect the current health
    public void UpdateHealthUI(float currentHealth)
    {
        // Return all hearts to the pool
        foreach (Transform child in heartContainer)
        {
            heartPool.ReturnImage(child.GetComponent<Image>());
        }

        // Add hearts based on current health
        for (int i = 0; i < (int)currentHealth; i++)
        {
            Image heart = heartPool.GetImage();
            heart.transform.SetParent(heartContainer, false);

            RectTransform heartRect = heart.GetComponent<RectTransform>();
            heartRect.anchoredPosition = new Vector2(-i * 30,0);
        }
    }
}