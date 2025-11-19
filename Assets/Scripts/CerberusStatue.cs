using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CerberusStatue : MonoBehaviour
{
    public static CerberusStatue Instance; // Make singleton because we are shipping soon!!

    // private GameObject leftHead;
    // private GameObject leftArm;
    // private GameObject leftLeg;
    // private GameObject middleHead;
    // private GameObject torso;
    // private GameObject rightHead;
    // private GameObject rightArm;
    // private GameObject rightLeg;
    // private GameObject tail;
    // private GameObject[] statuePieces = new GameObject[] { };
    private Dictionary<string, GameObject> statuePiecesDict = new Dictionary<string, GameObject> { };
    private int foundPieces;
    void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }
        
        foundPieces = 0;
        // statuePieces = GetComponentsInChildren<GameObject>();
        foreach (Transform pieceTransform in gameObject.transform)
        {
            GameObject piece = pieceTransform.gameObject;
            piece.SetActive(false);
            // piece.SetActive(false);
            statuePiecesDict[piece.name] = piece;
            // Debug.Log(piece.name);
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
        // optional: add a particle effect
    }

}
