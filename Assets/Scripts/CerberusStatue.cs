using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CerberusStatue : MonoBehaviour
{
    public static CerberusStatue Instance; // Make singleton because we are shipping soon!!
    public Sprite completedStatue;
    public Dialogue StatueHints;
    public float fadeOutTime;
    private SpriteRenderer spriteRenderer;
    private Color opaqueStatue;
    private Color transparentStatue;
    private Dictionary<string, GameObject> statuePiecesDict = new Dictionary<string, GameObject> { };
    public Dictionary<string, string> statuePieceHintsDict = new Dictionary<string, string> { };
    private int foundPieces;
    void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }

        spriteRenderer = GetComponent<SpriteRenderer>();
        opaqueStatue = spriteRenderer.color;
        transparentStatue = opaqueStatue;
        opaqueStatue.a = 1;
        transparentStatue.a = 0;
        
        foundPieces = 0;

        foreach (Transform pieceTransform in gameObject.transform)
        {
            GameObject piece = pieceTransform.gameObject;
            piece.SetActive(false);
            statuePiecesDict[piece.name] = piece;
        }

        InitializeHintDictionary();
    }

    void Start()
    {
        ApplySavedStateFromProgress();
    }

    public void ActivateStatuePiece(string statuePieceName)
    {
        foundPieces++;

        // Once we've found a specific piece, update the list of pieces in the hint list
        statuePieceHintsDict.Remove(statuePieceName);
        Debug.Log("Deactivating " + statuePieceName);

        if (foundPieces == 9) { CompleteQuest(); }
        else { statuePiecesDict[statuePieceName].SetActive(true); }
        
        return;
    }

    private void CompleteStatue()
    {
        // destroy the "piece" sprites or game objects
        foreach (GameObject piece in statuePiecesDict.Values)
        {
            Destroy(piece);
        }
        // replace the main sprite with the completed statue sprite
        spriteRenderer.sprite = completedStatue;
        // optional: add a particle effect
    }

    private void ApplySavedStateFromProgress()
    {
        if (PlayerProgress.Instance == null)
            return;

        foreach (KeyValuePair<string, GameObject> statuePiece in statuePiecesDict)
        {
            if (PlayerProgress.Instance.HasCollected(statuePiece.Key))
            {
                // statuePiece.Value.SetActive(true);
                ActivateStatuePiece(statuePiece.Key);
            }
        }
    }
    
    private void CompleteQuest()
    {
        CompleteStatue();
        // commenting this out because right now it will just fade out as soon as player has collected the last tablet
        // StartCoroutine(QuestCompleteRoutine());
    }
    private IEnumerator QuestCompleteRoutine()
    {
        yield return new WaitForSeconds(1f);
        float elapsedTime = 0;

        while (elapsedTime < fadeOutTime)
        {
            float t = elapsedTime/fadeOutTime;

            spriteRenderer.color = Color.Lerp(opaqueStatue, transparentStatue, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    private void InitializeHintDictionary()
    {
        List <string> hintKeys = StatueHints.rightLines;
        List <string> hintValues = StatueHints.leftLines;

        for (int i = 0; i < hintKeys.Count; i++)
        {
            statuePieceHintsDict[hintKeys[i]] = hintValues[i];
        }
    }
}
