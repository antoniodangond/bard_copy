using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TextAccessibilityScript : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private TextMeshProUGUI choiceText;
    [SerializeField] private Slider textSizeSlider;

    private void Awake()
    {
        if (textSizeSlider == null) textSizeSlider = GetComponent<Slider>();
    }

    private void Start()
    {
        float savedSize = (PlayerProgress.Instance != null)
            ? PlayerProgress.Instance.GetDialogueFontSize()
            : (text != null ? text.fontSize : 36f);

        textSizeSlider.SetValueWithoutNotify(savedSize);
        ApplySize(savedSize);

        textSizeSlider.onValueChanged.AddListener(OnSliderChanged);
    }

    private void OnDestroy()
    {
        if (textSizeSlider != null)
            textSizeSlider.onValueChanged.RemoveListener(OnSliderChanged);
    }

    private void OnSliderChanged(float size)
    {
        ApplySize(size);

        if (PlayerProgress.Instance != null)
            PlayerProgress.Instance.SetDialogueFontSize(size);
    }

    private void ApplySize(float size)
    {
        if (text != null) text.fontSize = size;
        if (choiceText != null) choiceText.fontSize = size;
    }
}
