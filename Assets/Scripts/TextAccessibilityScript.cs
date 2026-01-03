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

    private void HandleProgressLoaded()
    {
        // Progress is now guaranteed loaded -> apply the real saved size
        float size = GetSavedOrFallbackSize();
        if (textSizeSlider != null)
            textSizeSlider.SetValueWithoutNotify(size);
        ApplySize(size);
    }

    public void RegisterChoiceText(TextMeshProUGUI newChoiceText)
    {
        choiceText = newChoiceText;

        // Apply immediately using the best known value
        float size = GetSavedOrFallbackSize();
        ApplySize(size);
    }

    private float GetSavedOrFallbackSize()
    {
        if (PlayerProgress.Instance != null)
            return PlayerProgress.Instance.GetDialogueFontSize();

        if (textSizeSlider != null)
            return textSizeSlider.value;

        if (text != null)
            return text.fontSize;

        return 36f;
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
