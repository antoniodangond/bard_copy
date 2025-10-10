using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.ShaderGraph.Internal;
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
    // Health ("Hearts") info
    private ImagePool fullHearts;
    public GameObject fullHeartObj; // Assign in inspector
    private Transform fullHeartStartLocation;
    private int maxHearts = 5;
    private List<Image> currentHearts = new List<Image>();
    private ImagePool emptyHearts;
    public GameObject emptyHeartObj; // Assign in inspector
    private Transform emptyHeartStartLocation;
    // Tablets Info
    private int totalTablets = 5;
    private ImagePool collectedTabletsImages;
    public GameObject collectedTabletObj; // Assign in inspector
    // private int numCollectedTablets = 0;
    private Dictionary<String, Vector2> tabletImageLocations = new Dictionary<String, Vector2>();
    [Header("Health UI Settings")]
    // Below are for orienting the Health UI elements in a fan like shape
    [SerializeField] public float xspacing = 75f;
    [SerializeField] public float yspacing = 75f;
    [SerializeField] public float baseRotation = 15f; // Start at 45 degrees right
    [SerializeField] public float rotationStep = -45f;    // End at 45 degrees left
    [SerializeField] public float radius = 150f;     // Distance from center
    [SerializeField] public float baseScale = 0.8f; // Start at 80% of full size
    [SerializeField] public float scaleStep = 0.1f; // End 120% of full size
    [SerializeField] private float overlapAmount = 0.7f; // 0=no overlap, 1=full overlap
    [Header("Tablet Settings")]
    [SerializeField] public Vector2 tablet1DrawLocation;
    [SerializeField] public Vector2 tablet2DrawLocation;
    [SerializeField] public Vector2 tablet3DrawLocation;
    [SerializeField] public Vector2 tablet4DrawLocation;
    [SerializeField] public Vector2 tablet5DrawLocation;


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
        UpdateTabletDrawLocations();
    }

    private void InitializeUI()
    {
        fullHeartStartLocation = gameObject.transform.GetChild(0).GetChild(1);
        fullHearts = new ImagePool(fullHeartObj.GetComponent<Image>(), fullHeartStartLocation, 5);
        UpdateHealthUI(maxHearts);
    }

    private void UpdateTabletDrawLocations()
    {
        tabletImageLocations.Add("Tablet1", tablet1DrawLocation);
        tabletImageLocations.Add("Tablet2", tablet2DrawLocation);
        tabletImageLocations.Add("Tablet3", tablet3DrawLocation);
        tabletImageLocations.Add("Tablet4", tablet4DrawLocation);
        tabletImageLocations.Add("Tablet5", tablet5DrawLocation);
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
            float xPos, yPos;
            float scale = baseScale + (i * scaleStep);

            // Position (stacked leftward with overlap)
            if (i % 2 == 1)
            {
                xPos = -i * overlapAmount;
                yPos = -i * yspacing;
                rt.anchoredPosition = new Vector2(xPos, yPos);
            }
            else
            {
                xPos = -i * xspacing * overlapAmount;
                yPos = -i * yspacing;
                rt.anchoredPosition = new Vector2(xPos, yPos);
            }
            // xPos = -i * xspacing * overlapAmount;

            // Apply transformations
            rt.localEulerAngles = new Vector3(0, 0, rotation);
            rt.localScale = Vector3.one * scale;
            rt.SetAsLastSibling(); // Bring to front

            // Ensure pivot point is centered
            // rt.pivot = new Vector2(0.5f, 0.5f);
            // rt.anchorMin = new Vector2(0.5f, 0.5f);
            // rt.anchorMax = new Vector2(0.5f, 0.5f);

            // Set heart visibility based on current health
            heart.gameObject.SetActive(i < currentHealth);
            heart.color = i < currentHealth ? Color.white : new Color(1, 1, 1, 0.2f);

            currentHearts.Add(heart);
        }
    }

    public void UpdateCollectedTabletsUI(int numTabletsCollected, HashSet<String> collectedTablets)
    {
        
        for (int i = 0; i < numTabletsCollected; i++)
        {
            
        }
        
    }
}