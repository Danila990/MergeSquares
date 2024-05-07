using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class SwitchImageButton : Button
    {
        [SerializeField] private Image additionalImage;
        [SerializeField] private Sprite onSprite;
        [SerializeField] private Sprite offSprite;
        
        public void UpdateStateImage(bool state)
        {
            additionalImage.sprite = state ? onSprite : offSprite;
            additionalImage.SetNativeSize();
        }
    }
}

