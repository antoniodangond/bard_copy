using TMPro;
using UnityEngine;


public class TextAccessibilityScript : MonoBehaviour
{
    public TextMeshProUGUI text;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log(text.fontSize);
        text.fontSize = 96;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
