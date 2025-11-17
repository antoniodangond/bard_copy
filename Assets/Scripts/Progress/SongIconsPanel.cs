using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // for TextMeshProUGUI

public class SongIconsPanel : MonoBehaviour
{
    [Serializable]
    public class Entry
    {
        [Header("Song Info")]
        public string songId;     // e.g., "Melody1"

        [Header("Icon")]
        public Image image;       // UI Image to update
        public Sprite locked;     // gray sprite
        public Sprite learned;    // colored sprite

        [Header("Notes Display")]
        [Tooltip("The Instrument action names for this song in order, e.g. NoteC, NoteB, ...")]
        public string[] noteActions;    // e.g., [ "NoteC", "NoteB", "NoteC", "NoteD", "NoteE" ]

        [Tooltip("Text objects for each note position under the icon.")]
        public List<TextMeshProUGUI> noteLabels = new();   // 1 label per note slot (e.g. 5)
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

            // If you don't have note labels set up for this entry, skip
            if (e.noteLabels == null || e.noteLabels.Count == 0)
                continue;

            // If song not learned yet → clear labels & bail
            if (!learned)
            {
                foreach (var label in e.noteLabels)
                {
                    if (label) label.text = "";
                }
                continue;
            }

            // Song is learned → display the per-note inputs
            if (e.noteActions == null || e.noteActions.Length == 0)
            {
                // No note data assigned; avoid errors but clear labels
                foreach (var label in e.noteLabels)
                {
                    if (label) label.text = "";
                }
                continue;
            }

            // Use InputDisplayUtil to get the button string for this song
            // e.g. "A, W, A, S, D" on keyboard or a series of <sprite> tags on gamepad
            string buttonsString = InputDisplayUtil.GetLyreButtons(e.noteActions);
            string[] parts = buttonsString.Split(new[] { ", " }, StringSplitOptions.None);

            // Assign each part into the corresponding text label
            for (int i = 0; i < e.noteLabels.Count; i++)
            {
                var label = e.noteLabels[i];
                if (!label) continue;

                if (i < parts.Length)
                {
                    label.text = parts[i];
                }
                else
                {
                    // No corresponding note for this slot
                    label.text = "";
                }
            }
        }
    }
}
