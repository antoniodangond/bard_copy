using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUIManager : MonoBehaviour
{
    // Singleton instance
    private static PlayerUIManager _instance;
    
    public static PlayerUIManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindAnyObjectByType<PlayerUIManager>();
                
                if (_instance == null)
                {
                    GameObject obj = new GameObject("PlayerUIManager");
                    _instance = obj.AddComponent<PlayerUIManager>();
                    // Optionally make persistent across scenes:
                    // DontDestroyOnLoad(obj);
                }
            }
            return _instance;
        }
    }

    private ImagePool fullHearts;
    public GameObject fullHeartObj; // Assign in inspector
    private Transform fullHeartStartLocation;
    private int maxHearts = 5;
    private List<Image> currentHearts = new List<Image>();
    private ImagePool emptyHearts;
    public GameObject emptyHeartObj; // Assign in inspector
    private Transform emptyHeartStartLocation;
    [Header ("Health UI Settings")]
    // Below are for orienting the Health UI elements in a fan like shape
    [SerializeField]public float xspacing = 75f;
    [SerializeField]public float baseRotation = -45f; // Start at 45 degrees left
    [SerializeField]public float rotationStep = 15f;    // End at 45 degrees right
    [SerializeField]public float radius = 150f;     // Distance from center
    [SerializeField]public float baseScale = 0.8f; // Start at 80% of full size
    [SerializeField]public float scaleStep = 0.1f; // End 120% of full size
    [SerializeField] private float overlapAmount = 0.7f; // 0=no overlap, 1=full overlap

    void Awake()
    {
        // Singleton enforcement
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            _instance = this;
            // Optionally make persistent across scenes:
            // DontDestroyOnLoad(gameObject);
        }

        InitializeUI();
    }

    private void InitializeUI()
    {
        fullHeartStartLocation = gameObject.transform.GetChild(0).GetChild(0);
        fullHearts = new ImagePool(fullHeartObj.GetComponent<Image>(), fullHeartStartLocation, 5);
        UpdateHealthUI(maxHearts);
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
        for (int i = 0; i < maxHearts; i++)
        {
            Image heart = fullHearts.GetImage();
            heart.transform.SetParent(fullHeartStartLocation, false);
            
            RectTransform rt = heart.GetComponent<RectTransform>();
            // Calculate rotation angle (evenly distributed between start/end angles)
            float rotation = baseRotation + (i * rotationStep);
            float xPos = i * xspacing;
            float scale = baseScale + (i * scaleStep);
            
             // Position (stacked leftward with overlap)
            xPos = -i * xspacing * overlapAmount;
            
            // Apply transformations
            rt.anchoredPosition = new Vector2(xPos, 0);
            rt.localEulerAngles = new Vector3(0, 0, rotation);
            rt.localScale = Vector3.one * scale;
            rt.SetAsLastSibling(); // Bring to front

             // Ensure pivot point is centered
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);

            // Set heart visibility based on current health
            heart.gameObject.SetActive(i < currentHealth);
            heart.color = i < currentHealth ? Color.white : new Color(1,1,1,0.2f);
            
            currentHearts.Add(heart);
        }
    }
}