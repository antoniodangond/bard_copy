using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CerberusStatue : MonoBehaviour
{
    public static CerberusStatue Instance; // Make singleton because we are shipping soon!!
    public Sprite completedStatue;
    private SpriteRenderer spriteRenderer;
    private Dictionary<string, GameObject> statuePiecesDict = new Dictionary<string, GameObject> { };
    private int foundPieces;
    void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }

        spriteRenderer = GetComponent<SpriteRenderer>();

        foundPieces = 0;

        foreach (Transform pieceTransform in gameObject.transform)
        {
            GameObject piece = pieceTransform.gameObject;
            piece.SetActive(false);
            statuePiecesDict[piece.name] = piece;
        }
    }

    public void ActivateStatuePiece(string statuePieceName)
    {
        foundPieces++;

        if (foundPieces == 9) { CompleteStatue(); }
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

}
