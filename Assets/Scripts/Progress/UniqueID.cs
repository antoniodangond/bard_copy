using UnityEngine;

[ExecuteAlways] // lets it work in edit mode
public class UniqueId : MonoBehaviour
{
    [SerializeField] private string id;
    public string Id => id;

#if UNITY_EDITOR
    [ContextMenu("Generate New ID")]
    public void GenerateNewId()
    {
        id = System.Guid.NewGuid().ToString("N"); // 32-char hex string
        UnityEditor.EditorUtility.SetDirty(this);
    }
#endif
}