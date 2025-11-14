using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerSpawnOnLoad : MonoBehaviour
{
    IEnumerator Start()
    {
        // wait one frame so binders/OnLoaded have applied
        yield return null;

        var pp = PlayerProgress.Instance;
        if (pp == null) yield break;

        if (pp.GetSavedSceneName() == SceneManager.GetActiveScene().name)
        {
            Vector3 p = pp.GetPlayerPosition();
            // change this guard to your liking (e.g., a separate "hasSavedPos" flag)
            if (p != Vector3.zero) transform.position = p;
        }
    }
}
