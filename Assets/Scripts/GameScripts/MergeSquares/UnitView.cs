using Core.Repositories;
using GameScripts.MergeSquares.Shop;
using UnityEngine;
using Zenject;
using Image = UnityEngine.UI.Image;

namespace GameScripts.MergeSquares
{
    public class UnitView : MonoBehaviour
    {
        [SerializeField] private ResourceRepository resourceRepository;
        [SerializeField] private Image number;
        [SerializeField] private Image image;
        [SerializeField] private Image frame;
        [SerializeField] private Image secret;
        [SerializeField] private GameObject changeOverlay;
        [SerializeField] private UnitViewAnimator animator;
        [SerializeField] private Canvas canvas;
        [Space]
        [SerializeField] private Vector2 defaultAnchorsMin;
        [SerializeField] private Vector2 defaultAnchorsMax;

        public int Value => _value;
        public UnitViewAnimator Animator => animator;
        public Canvas Canvas => canvas;

        private int _value;
        private SquareImage _imageData;
        
        private GridManager _gridManager;
        
        [Inject]
        public void Construct(GridManager gridManager)
        {
            _gridManager = gridManager;
        }

        public void SetChangeOverlayActive(bool active) => changeOverlay.SetActive(active);

        public void Init(int value)
        {
            _value = value;
            _imageData = resourceRepository.GetSquareImageByValue(value);
            if (_imageData != null)
            {
                if (_imageData.numberSprite != null)
                    SetNumberSprite(_imageData.numberSprite);
                
                if (_gridManager == null)
                {
                    _gridManager = ZenjectBinding.FindObjectOfType<GridManager>();
                }
                SetSkin(_gridManager.CurrentSkin);
            }
        }
        
        public void SetSecret(bool value)
        {
            if (secret != null)
            {
                secret.gameObject.SetActive(value);
            }
        }

        public void SetSkin(ESquareSkin skin)
        {
            switch (skin)
            {
                case ESquareSkin.baseSprite:
                    SetNewSkin(_imageData.baseSprite, false);
                    break;
                case ESquareSkin.bubbleSprite:
                    SetNewSkin(_imageData.bubbleSprite, false);
                    break;
                case ESquareSkin.candySprite:
                    SetNewSkin(_imageData.candySprite, false);
                    break;
                case ESquareSkin.glassSprite:
                    SetNewSkin(_imageData.glassSprite, false);
                    break;
                case ESquareSkin.skySprite:
                    SetNewSkin(_imageData.skySprite, false);
                    break;
                case ESquareSkin.softSprite:
                    SetNewSkin(_imageData.softSprite, false);
                    break;
                case ESquareSkin.woodSprite:
                    SetNewSkin(_imageData.woodSprite, false);
                    break;
                case ESquareSkin.colorFrame:
                    SetNewSkin(_imageData.baseSprite, true, _imageData.colorFrame);
                    break;
                case ESquareSkin.silverFrame:
                    SetNewSkin(_imageData.baseSprite, true, _imageData.silverFrame);
                    break;
                case ESquareSkin.goldFrame:
                    SetNewSkin(_imageData.baseSprite, true, _imageData.goldFrame);
                    break;
                case ESquareSkin.leavesFrame:
                    SetNewSkin(_imageData.baseSprite, true, _imageData.leavesFrame);
                    break;
                case ESquareSkin.woodFrame:
                    SetNewSkin(_imageData.baseSprite, true, _imageData.woodFrame);
                    break;
                case ESquareSkin.external:
                    SetNewSkin(_imageData.external != null ? _imageData.external : _imageData.baseSprite, _imageData.externalFrame != null, _imageData.externalFrame);
                    break;
                default:
                    if (_imageData != null)
                    {
                        SetNewSkin(_imageData.baseSprite, false);
                    }
                    break;
            }
        }

        private void SetNumberSprite(Sprite sprite)
        {
            number.gameObject.SetActive(true);
            number.sprite = sprite;
        }

        private void SetNewSkin(Sprite sprite, bool frameActive, Sprite frameSprite = null)
        {
            SetAnchorsDefault();
            image.sprite = sprite;
            frame.gameObject.SetActive(frameActive);
            frame.sprite = frameSprite;
            if (sprite == null)
            {
                SetSkin(ESquareSkin.baseSprite);
            }
        }

        private void SetAnchorsDefault()
        {
            SetAnchors(defaultAnchorsMin.x, defaultAnchorsMin.y, defaultAnchorsMax.x, defaultAnchorsMax.y);
        }

        private void SetAnchors(float xMin, float yMin, float xMax, float yMax)
        {
            frame.rectTransform.anchorMin = new Vector2(xMin, yMin);
            frame.rectTransform.anchorMax = new Vector2(xMax, yMax);
        }

        public void ResetRect()
        {
            var rect = (RectTransform) transform;
            rect.sizeDelta = Vector2.zero;
        }
    }

    class UnitViewImpl : UnitView
    {
    }
}