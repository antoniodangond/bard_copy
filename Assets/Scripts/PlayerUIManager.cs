using System.Collections.Generic;
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
    private float spacing = 75f;
    private int maxHearts = 5;
    public float radius = 100f; // Radius for arranging UI Hearts in a circle
    private List<Image> currentHearts = new List<Image>();
    private ImagePool emptyHearts;
    public GameObject emptyHeartObj; // Assign in inspector
    private Transform emptyHeartStartLocation;

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
        for (int i = 0; i < currentHealth; i++)
        {
            Image heart = fullHearts.GetImage();
            heart.transform.SetParent(fullHeartStartLocation, false);
            
            RectTransform rt = heart.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(-i * spacing, 0);
            
            currentHearts.Add(heart);
        }
    }
}