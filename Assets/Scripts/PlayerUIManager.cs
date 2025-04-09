using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUIManager : MonoBehaviour
{
    private ImagePool fullHearts;
    public GameObject fullHeartObj; //Assign in inspector
    private Transform fullHeartStartLocation;
    private float spacing = 75f;
    private int maxHearts = 5;
    public float radius = 100f;//radius for arranging UI Hearts in a circle
    private List<Image> currentHearts = new List<Image>();
    private ImagePool emptyHearts;
    public GameObject emptyHeartObj; //Assign in inspector
    private Transform emptyHeartStartLocation;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        fullHeartStartLocation = gameObject.transform.GetChild(0).GetChild(0); // Get Transform of Heart Sprite
        // fullHeartStartLocation = fullHeartObj.transform; // Get Transform of Heart Sprite
        // emptyHeartStartLocation = gameObject.transform.GetChild(0).GetChild(1); // Get Transform of Heart Sprite
        fullHearts = new ImagePool(fullHeartObj.GetComponent<Image>(), fullHeartStartLocation, 5);
        // emptyHearts  = new ImagePool(emptyHeartObj.GetComponent<Image>(), emptyHeartStartLocation, 5);

        UpdateHealthUI(maxHearts);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateHealthUI(int currentHealth)
    {
        // Return all hearts to pool
        foreach (Image heart in currentHearts)
        {
            fullHearts.ReturnImage(heart);
        }
        currentHearts.Clear();

        // Create new hearts with staggered positions
        for (int i = 0; i < currentHealth; i++)
        {
            Image heart = fullHearts.GetImage();
            heart.transform.SetParent(fullHeartStartLocation, false);
            
            float angleStep = 360f/currentHealth;
            RectTransform rt = heart.GetComponent<RectTransform>();
            // Position each heart with offset
            // float angle = i * angleStep * Mathf.Deg2Rad;
            // float x = Mathf.Cos(angle) * radius;
            // float y = Mathf.Sin(angle) * radius;
        
            rt.anchoredPosition = new Vector2(-i * spacing, 0);
            
            currentHearts.Add(heart);
        }
    }
}
