using UnityEngine;

public class QuestObjectIndicator : MonoBehaviour
{

    public SignController sign;
    public ParticleSystem particleSystem;
    public bool endedParticles;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (gameObject.GetComponent<SignController>() != null)
        {
            sign = gameObject.GetComponent<SignController>();
        }
        particleSystem.Play();
        endedParticles = false;
    }

    // Update is called once per frame
    void Update()
    {

    }
    
    public void EndParticles()
    {
        if (!endedParticles)
        {
            particleSystem.Stop();
            endedParticles = true;
        }
        else { return; }
        // Debug.Log("stopping particles");
    }
}
