using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
[DefaultExecutionOrder(-900)]

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance;
    public GameObject MenuUI;
    public ButtonSelectionHandler[] buttons { get; private set; }
    private Dictionary<GameObject, (Vector3 _startPos, Vector3 _startScale)> buttonStates;
    // public bool isPaused {get; private set;}
     void Awake()
    {
        // Ensure only one instance of MenuManager exists
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject);  // Optional: persists across scenes
        }
        else if (Instance != this)
        {
            Destroy(gameObject);  // Destroy duplicate instances
            return;
        }
    }
    private void Start()
    {
        buttonStates = new Dictionary<GameObject, (Vector3, Vector3)>();
        buttons = FindObjectsByType<ButtonSelectionHandler>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (var h in buttons)
        {
            var tgt = h.animTarget ? h.animTarget.gameObject : h.gameObject;
            buttonStates[tgt] = (tgt.transform.position, tgt.transform.localScale);
        }
    }

    public void TriggerButtonAnimation(GameObject button, bool startingAnimation)
    {
        if (buttonStates.ContainsKey(button))
        {
            StartCoroutine(AnimateButton(button, startingAnimation));
        }
        else
        {
            Debug.LogError($"Button {button.name} not found in buttonStates dictionary!");
        }
        // StartCoroutine(AnimateButton(button, startingAnimation));
    }

    private IEnumerator AnimateButton(GameObject button, bool startingAnimation)
    {
        if (button == null) yield break;

        float moveTime = 0.1f;
        float elapsedTime = 0f;
        float _verticalMoveAmount = 10f;
        float _scaleAmount = 1.1f;
        
        var (initialPos, initalScale) = buttonStates[button];

        // Define animation targets
        Vector3 _endPos; 
        Vector3 _endscale;


        while (elapsedTime < moveTime)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float t = elapsedTime / moveTime;
             if(startingAnimation)
            {
                _endPos = initialPos + new Vector3(0f, _verticalMoveAmount, 0f);
                _endscale = initalScale * _scaleAmount;
            }
            else
            {
                _endPos = initialPos;
                _endscale = initalScale;
            }

            // Interpolate position and scale
            // button.transform.position = Vector3.Lerp(initialPos, _endPos, t);
            // button.transform.localScale = Vector3.Lerp(initalScale, _endscale, t);
            Vector3 LerpedPos = Vector3.Lerp(button.transform.position, _endPos, t);
            Vector3 LerpedScale = Vector3.Lerp(button.transform.localScale, _endscale, t);

            button.transform.position = LerpedPos;
            button.transform.localScale = LerpedScale;

            yield return null;
        }

    }

    public void RegisterAnimTarget(GameObject target)
    {
        if (target == null) return;
        if (!buttonStates.ContainsKey(target))
        {
            buttonStates[target] = (target.transform.position, target.transform.localScale);
        }
    }

    public void EnsureRegistered(GameObject target)
    {
        if (target == null) return;
        if (!buttonStates.ContainsKey(target))
        {
            buttonStates[target] = (target.transform.position, target.transform.localScale);
        }
    }

}
