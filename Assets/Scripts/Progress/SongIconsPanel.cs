using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SongIconsPanel : MonoBehaviour
{
    [Serializable]
    public class Entry
    {
        public string songId;     // e.g., "Melody1"
        public Image image;       // UI Image to update
        public Sprite locked;     // gray sprite
        public Sprite learned;    // colored sprite
    }

    [SerializeField] private List<Entry> entries = new();

    void OnEnable()
    {
        var pp = PlayerProgress.Instance;
        if (pp != null)
        {
            pp.OnLoaded += OnProgressChanged;
            pp.OnSaved  += OnProgressChanged;
        }
        Refresh();
    }

    void OnDisable()
    {
        var pp = PlayerProgress.Instance;
        if (pp != null)
        {
            pp.OnLoaded -= OnProgressChanged;
            pp.OnSaved  -= OnProgressChanged;
        }
    }

    void Start() => Refresh();

    private void OnProgressChanged()
    {
        Refresh();
    }

    public void Refresh()
    {
        var pp = PlayerProgress.Instance;
        if (pp == null) return;

        foreach (var e in entries)
        {
            if (!e.image) continue;
            bool learned = pp.HasSong(e.songId);
            e.image.sprite = learned ? e.learned : e.locked;
        }
    }
}
