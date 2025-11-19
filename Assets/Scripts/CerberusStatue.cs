using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CerberusStatue : MonoBehaviour
{
    public static CerberusStatue Instance; // Make singleton because we are shipping soon!!

    private SpriteRenderer leftHead;
    private SpriteRenderer leftArm;
    private SpriteRenderer leftLeg;
    private SpriteRenderer middleHead;
    private SpriteRenderer torso;
    private SpriteRenderer rightHead;
    private SpriteRenderer rightArm;
    private SpriteRenderer rightLeg;
    private SpriteRenderer tail;
    private SpriteRenderer sr;
    private SpriteRenderer[] statuePieces = new SpriteRenderer[] { };
    // private string[] statuePieceNames = new string[] { };
    private Dictionary<string, SpriteRenderer> statuePiecesDict = new Dictionary<string, SpriteRenderer> { };
    private int foundPieces;
    void Awake()
    {
        
        foundPieces = 0;
        statuePieces = GetComponentsInChildren<SpriteRenderer>();
        // statuePieceNames = 
        foreach (SpriteRenderer piece in statuePieces)
        {
            piece.enabled = false;
            statuePiecesDict[piece.name] = piece;
            Debug.Log(piece.name);
        }
    }

    public void ActivateStatuePiece(string statuePieceName)
    {
        foundPieces++;

        if (foundPieces == 9) { CompleteStatue(); }
        else { statuePiecesDict[statuePieceName].enabled = true; }
        
        return;
    }

    private void CompleteStatue()
    {
        // destroy the "piece" sprites or game objects
        foreach (SpriteRenderer piece in statuePieces)
        {
            Destroy(piece);
        }
        // replace the main sprite with the completed statue sprite
        // optional: add a particle effect
    }

}
