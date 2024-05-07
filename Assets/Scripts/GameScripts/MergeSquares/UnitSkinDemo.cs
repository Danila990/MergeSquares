using Core.Repositories;
using LargeNumbers;
using TMPro;
using UnityEngine;
using Image = UnityEngine.UI.Image;

namespace GameScripts.MergeSquares
{
    public class UnitSkinDemo : MonoBehaviour
    {
        [SerializeField] private bool for2248;
        [SerializeField] private TextMeshProUGUI numberText;
        [SerializeField] private int value;
        [SerializeField] private Image image;
        [SerializeField] private Image number;
        [SerializeField] private Image frame;
        [SerializeField] private ResourceRepository resourceRepository;

        
        private void Start()
        {
            if(!for2248)
            {
                InitForMergeSquares();
            }
            else
            {
                InitFor2248();
            }
        }

        private void InitForMergeSquares()
        {
            var imageData = resourceRepository.GetSquareImageByValue(value);
            if (imageData != null)
            {
                image.sprite = imageData.external;
                number.gameObject.SetActive(false);
                if (imageData.numberSprite != null)
                {
                    number.gameObject.SetActive(true);
                    number.sprite = imageData.numberSprite;
                }
                frame.gameObject.SetActive(false);
                if(imageData.externalFrame != null)
                {
                    frame.gameObject.SetActive(true);
                    frame.sprite = imageData.externalFrame;
                }
            }
        }
        
        private void InitFor2248()
        {
            var imageData = resourceRepository.GetSquare2248ImageByValue(new LargeNumber(value));
            if (imageData != null)
            {
                image.sprite = imageData.external;
                number.gameObject.SetActive(false);
                if (numberText != null)
                {
                    numberText.text = $"{value}";
                }
                frame.gameObject.SetActive(false);
                if(imageData.externalFrame != null)
                {
                    frame.gameObject.SetActive(true);
                    frame.sprite = imageData.externalFrame;
                }
            }
        }
    }
}