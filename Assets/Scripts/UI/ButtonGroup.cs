using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UI
{
    public class ButtonGroup : MonoBehaviour
    {
        [SerializeField] private Button mainButton;
        [SerializeField] private List<Image> images;
        [SerializeField] private UnityEventBool turnedOn;

        public Button Main => mainButton;
        
        public void SetInteractable(bool value)
        {
            mainButton.interactable = value;
            
            foreach (var image in images)
            {
                image.color = value ? mainButton.colors.normalColor : mainButton.colors.disabledColor;
            }
            turnedOn.Invoke(value);
        }
    }
}