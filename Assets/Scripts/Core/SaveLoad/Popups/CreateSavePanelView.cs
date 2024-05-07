using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Core.SaveLoad.Popups
{
    public class CreateSavePanelView : MonoBehaviour
    {
        public event Action<string, bool> OnSaveCreated = (saveName, empty) => { };

        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private TextMeshProUGUI currentSaveKey;
        [SerializeField] private Toggle emptyToggle;

        public void UpdateCurrentKey(string key) => currentSaveKey.text = key;
        
        public void OnPerform()
        {
            OnSaveCreated.Invoke(inputField.text, emptyToggle.isOn);
        }
    }
}