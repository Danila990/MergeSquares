using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Core.SaveLoad.Popups
{
    public class SaveLoadPanelView : MonoBehaviour
    {
        [SerializeField] private Color highlightedColor;
        [SerializeField] private Color defaultColor;

        [SerializeField] private Image highlighter;

        [SerializeField] private TextMeshProUGUI text;

        [SerializeField] private Button saveButton;
        [SerializeField] private Button loadButton;
        [SerializeField] private Button removeButton;

        public bool IsHighlight { get; private set; }

        public void Init(string key, Action save, Action load, Action remove)
        {
            text.text = key;

            saveButton.onClick.AddListener(save.Invoke);
            loadButton.onClick.AddListener(load.Invoke);
            removeButton.onClick.AddListener(remove.Invoke);
        }

        public void Highlight(bool highlight)
        {
            IsHighlight = highlight;
            highlighter.color = IsHighlight ? highlightedColor : defaultColor;
        }
    }
}