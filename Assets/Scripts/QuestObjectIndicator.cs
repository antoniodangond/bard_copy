using UnityEngine;

public class QuestObjectIndicator : MonoBehaviour
{

    public SignController sign;
    public ParticleSystem particleSystem;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (gameObject.GetComponent<SignController>() != null)
        {
            sign = gameObject.GetComponent<SignController>();
        }
        particleSystem.Play();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
