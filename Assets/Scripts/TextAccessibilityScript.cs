using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class TextAccessibilityScript : MonoBehaviour
{
    public TextMeshProUGUI text;
    private Slider textSizeSlider;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        textSizeSlider = gameObject.GetComponent<Slider>();
        textSizeSlider.onValueChanged.AddListener(delegate { SliderChanged(); });
    }

    // Update is called once per frame
    void Update()
    {

    }

    void SliderChanged()
    {
        ChangeDialogueTextSize();
    }

    void ChangeDialogueTextSize()
    {
        text.fontSize = textSizeSlider.value;
        Debug.Log(textSizeSlider.value);
    }
}
