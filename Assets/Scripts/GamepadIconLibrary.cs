using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GamepadIconLibrary", menuName = "UI/Gamepad Icon Library")]
public class GamepadIconLibrary : ScriptableObject
{
    [Serializable]
    public class Mapping
    {
        public string controlPath; // e.g. "<Gamepad>/buttonSouth"
        public Sprite playstationSprite;
        public Sprite xboxSprite;
    }

    public List<Mapping> mappings = new();

    private Dictionary<string, Mapping> map;

    private void Build()
    {
        if (map != null) return;
        map = new Dictionary<string, Mapping>(StringComparer.Ordinal);
        foreach (var m in mappings)
        {
            if (!string.IsNullOrEmpty(m.controlPath))
                map[m.controlPath] = m;
        }
    }

    public Sprite Get(string controlPath, InputDisplayUtil.PromptMode mode)
    {
        Build();
        if (string.IsNullOrEmpty(controlPath)) return null;

        if (!map.TryGetValue(controlPath, out var m)) return null;

        return mode == InputDisplayUtil.PromptMode.PlayStation ? m.playstationSprite : m.xboxSprite;
    }
}
