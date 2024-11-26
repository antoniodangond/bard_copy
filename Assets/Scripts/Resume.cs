using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

public class Resume : MonoBehaviour
{
    public Button resume;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        resume = gameObject.GetComponent<Button>();
        resume.onClick.AddListener(Clicked);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Clicked()
    {
        FindAnyObjectByType<PlayerInputManager>().HandleMenuOpen();
    }
}
