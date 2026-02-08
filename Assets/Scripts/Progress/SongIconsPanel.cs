using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SongIconsPanel : MonoBehaviour
{
    [SerializeField] private GamepadIconLibrary iconLibrary;

    [Serializable]
    public class Entry
    {
        [Header("Song Info")]
        public string songId; // e.g., "Melody1"

        [Header("Icon")]
        public Image image;
        public Sprite locked;
        public Sprite learned;

        [Header("Notes Display")]
        [Tooltip("Instrument action names for this song in order, e.g. NoteC, NoteB, ...")]
        public string[] noteActions;

        [Header("Keyboard Row (TMP)")]
        public GameObject keyboardRowRoot;                   // KeyboardInputs#
        public List<TextMeshProUGUI> keyboardLabels = new();  // N1-N5

        [Header("Controller Row (Images)")]
        public GameObject controllerRowRoot;                 // ControllerInputs#
        public List<Image> controllerIcons = new();           // I1-I5
    }

    [SerializeField] private List<Entry> entries = new();

    private void OnEnable()
    {
        var pp = PlayerProgress.Instance;
        if (pp != null)
        {
            pp.OnLoaded += Refresh;
            pp.OnSaved  += Refresh;
        }
        Refresh();
    }

    private void OnDisable()
    {
        var pp = PlayerProgress.Instance;
        if (pp != null)
        {
            pp.OnLoaded -= Refresh;
            pp.OnSaved  -= Refresh;
        }
    }

    private void Start() => Refresh();

    public void Refresh()
    {
        var pp = PlayerProgress.Instance;
        if (pp == null) return;

        var mode = InputDisplayUtil.GetPromptMode();
        bool showKeyboard = (mode == InputDisplayUtil.PromptMode.Keyboard);

        foreach (var e in entries)
        {
            if (!e.image) continue;

            bool learned = pp.HasSong(e.songId);
            e.image.sprite = learned ? e.learned : e.locked;

            // Toggle rows (hide both if not learned)
            if (e.keyboardRowRoot)   e.keyboardRowRoot.SetActive(learned && showKeyboard);
            if (e.controllerRowRoot) e.controllerRowRoot.SetActive(learned && !showKeyboard);

            // If not learned -> clear both and continue
            if (!learned)
            {
                if (e.keyboardLabels != null)
                    foreach (var label in e.keyboardLabels)
                        if (label) label.text = "";

                if (e.controllerIcons != null)
                    foreach (var icon in e.controllerIcons)
                        if (icon) { icon.sprite = null; icon.enabled = false; }

                continue;
            }

            // Learned but no actions -> clear both and continue
            if (e.noteActions == null || e.noteActions.Length == 0)
            {
                if (e.keyboardLabels != null)
                    foreach (var label in e.keyboardLabels)
                        if (label) label.text = "";

                if (e.controllerIcons != null)
                    foreach (var icon in e.controllerIcons)
                        if (icon) { icon.sprite = null; icon.enabled = false; }

                continue;
            }

            if (showKeyboard)
            {
                // Keyboard: show letters (or whatever your util returns)
                string buttonsString = InputDisplayUtil.GetLyreButtons(e.noteActions);
                string[] parts = buttonsString.Split(new[] { ", " }, StringSplitOptions.None);

                for (int i = 0; i < e.keyboardLabels.Count; i++)
                {
                    var label = e.keyboardLabels[i];
                    if (!label) continue;

                    label.text = (i < parts.Length) ? parts[i] : "";
                }
            }
            else
            {
                // Controller: derive actual bound control paths from Input System, then map to sprites once via ScriptableObject
                string[] paths = InputDisplayUtil.GetLyreButtonPaths(e.noteActions);

                for (int i = 0; i < e.controllerIcons.Count; i++)
                {
                    var img = e.controllerIcons[i];
                    if (!img) continue;

                    if (i < paths.Length)
                    {
                        Sprite sp = iconLibrary ? iconLibrary.Get(paths[i], mode) : null;
                        img.sprite = sp;
                        img.enabled = (sp != null);
                    }
                    else
                    {
                        img.sprite = null;
                        img.enabled = false;
                    }
                }
            }
        }
    }
}
