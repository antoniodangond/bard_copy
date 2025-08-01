using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Dialogue))]
public class DialogueEditor : Editor
{
    private SerializedProperty upLines;
    private SerializedProperty downLines;
    private SerializedProperty leftLines;
    private SerializedProperty rightLines;
    private SerializedProperty universalLines;

    private bool showUp = true;
    private bool showDown = true;
    private bool showLeft = true;
    private bool showRight = true;
    private bool showUniversal = true;

    private enum Direction { Up, Down, Left, Right, Universal }
    private Direction copyFrom = Direction.Up;
    private Direction copyTo = Direction.Down;

    void OnEnable()
    {
        upLines = serializedObject.FindProperty("upLines");
        downLines = serializedObject.FindProperty("downLines");
        leftLines = serializedObject.FindProperty("leftLines");
        rightLines = serializedObject.FindProperty("rightLines");
        universalLines = serializedObject.FindProperty("universalLines");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.LabelField("Directional Dialogue Lines", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        showUp = EditorGUILayout.Foldout(showUp, "Up");
        if (showUp) DrawStringList(upLines);

        showDown = EditorGUILayout.Foldout(showDown, "Down");
        if (showDown) DrawStringList(downLines);

        showLeft = EditorGUILayout.Foldout(showLeft, "Left");
        if (showLeft) DrawStringList(leftLines);

        showRight = EditorGUILayout.Foldout(showRight, "Right");
        if (showRight) DrawStringList(rightLines);

        showUniversal = EditorGUILayout.Foldout(showUniversal, "Universal (Fallback)");
        if (showUniversal) DrawStringList(universalLines);

        EditorGUILayout.Space();
        DrawCopySection();

        EditorGUILayout.Space();
        if (GUILayout.Button("Clear All Lines"))
        {
            if (EditorUtility.DisplayDialog("Clear All", "Are you sure you want to clear all dialogue lines?", "Yes", "Cancel"))
            {
                ClearAllLines();
            }
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawStringList(SerializedProperty list)
    {
        EditorGUI.indentLevel++;
        for (int i = 0; i < list.arraySize; i++)
        {
            SerializedProperty element = list.GetArrayElementAtIndex(i);
            element.stringValue = EditorGUILayout.TextField($"Line {i + 1}", element.stringValue);
        }

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add Line")) list.arraySize++;
        if (GUILayout.Button("Remove Last") && list.arraySize > 0) list.arraySize--;
        EditorGUILayout.EndHorizontal();
        EditorGUI.indentLevel--;
        EditorGUILayout.Space();
    }

    private void DrawCopySection()
    {
        EditorGUILayout.LabelField("Copy Lines", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        copyFrom = (Direction)EditorGUILayout.EnumPopup("From", copyFrom);
        copyTo = (Direction)EditorGUILayout.EnumPopup("To", copyTo);
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Copy"))
        {
            if (copyFrom == copyTo)
            {
                EditorUtility.DisplayDialog("Invalid Copy", "Source and destination must be different.", "OK");
            }
            else
            {
                CopyLines(GetProperty(copyFrom), GetProperty(copyTo));
            }
        }
    }

    private SerializedProperty GetProperty(Direction dir)
    {
        return dir switch
        {
            Direction.Up => upLines,
            Direction.Down => downLines,
            Direction.Left => leftLines,
            Direction.Right => rightLines,
            Direction.Universal => universalLines,
            _ => null,
        };
    }

    private void CopyLines(SerializedProperty source, SerializedProperty destination)
    {
        destination.ClearArray();
        for (int i = 0; i < source.arraySize; i++)
        {
            destination.InsertArrayElementAtIndex(i);
            destination.GetArrayElementAtIndex(i).stringValue = source.GetArrayElementAtIndex(i).stringValue;
        }
        Debug.Log($"Copied {source.arraySize} lines from {copyFrom} to {copyTo}.");
    }

    private void ClearAllLines()
    {
        upLines.ClearArray();
        downLines.ClearArray();
        leftLines.ClearArray();
        rightLines.ClearArray();
        universalLines.ClearArray();
        Debug.Log("All dialogue lines cleared.");
    }
}
