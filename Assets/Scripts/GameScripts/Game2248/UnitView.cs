using System;
using System.Collections;
using Core.Repositories;
using LargeNumbers;
using GameScripts.MergeSquares;
using TMPro;
using UnityEngine;
using Zenject;
using Image = UnityEngine.UI.Image;

namespace GameScripts.Game2248
{
    [Serializable]
    public class UnitViewAnimParams
    {
        public float time;
        public float aColor;
        public float startRotation;
        public float endRotation;
        public float startScale = 1f;
        public float endScale;
    }
    public class UnitView : MonoBehaviour
    {
        [SerializeField] private ResourceRepository resourceRepository;
        [SerializeField] private TextMeshProUGUI number;
        [SerializeField] private Image image;
        [SerializeField] private Image secret;
        [SerializeField] private Image frame;
        [SerializeField] private Image maxFrame;
        [SerializeField] private Image maxFrameCrown;
        [SerializeField] private Image selectLight;
        [SerializeField] private UnitViewAnimator animator;
        [SerializeField] private Canvas canvas;
        [SerializeField] private ParticlesController particleController;
        [SerializeField] private GameObject changeOverlay;
        [Space]
        [SerializeField] private Vector2 defaultAnchorsMin;
        [SerializeField] private Vector2 defaultAnchorsMax;

        public LargeNumber Value => _value;
        public UnitViewAnimator Animator => animator;
        public ParticlesController ParticleController => particleController;
        public Canvas Canvas => canvas;
        public Square2248Image ImageData => imageData;

        private LargeNumber _value;
        private Square2248Image imageData;
        
        private GridManager _gridManager;
        
        [Inject]
        public void Construct(GridManager gridManager)
        {
            _gridManager = gridManager;
        }
        
        public void SetChangeOverlayActive(bool active) => changeOverlay.SetActive(active);

        public void Init(LargeNumber value, bool isMax = false)
        {
            if (_gridManager == null)
            {
                _gridManager = ZenjectBinding.FindObjectOfType<GridManager>();
            }

            _value = value;
            _gridManager.GetNearest2PowValue(value, out var pow);
            imageData = resourceRepository.GetSquare2248ImageByPow(pow);
            if (imageData != null)
            {
                // string str = "";
                // if (value.magnitude < 1)
                // {
                //     str = value.ToString();
                // }
                // else if(value.magnitude > 1)
                // {
                //     str = Math.Round(value.coefficient) + LargeNumber.GetLargeNumberName(value.magnitude)[0].ToString();
                // }
                // else
                // {
                //     str = Math.Round(value.coefficient) + "K";
                // }

                // if(value.magnitude < 1)
                // {
                //     str = (Math.Round(value.Standard()).ToString());
                // }
                // else
                //     str = $"{value.coefficient}{LargeNumber.GetLargeNumberName(value.magnitude)}";
                
                SetNumberText(new AlphabeticNotation(value).ToString());
                SetSkin(_gridManager.CurrentSkin);
                SetSelectLight(false);
                SetMaxFrame(isMax);
            }
        }

        public void SetSecret(bool value)
        {
            if (secret != null)
            {
                secret.gameObject.SetActive(value);
            }
        }

        public void SetSelectLight(bool isActive)
        {
            if (isActive)
            {
                selectLight.sprite = imageData.lightFrame;
                selectLight.color = imageData.color;
            }
            selectLight.gameObject.SetActive(isActive);
        }

        public void InitInvisible(LargeNumber value)
        {
            Init(value);
            SetInvisible();
        }

        public void SetInvisible()
        {
            number.gameObject.SetActive(false);
            image.gameObject.SetActive(false);
        }
        
        public void SetVisible()
        {
            number.gameObject.SetActive(true);
            image.gameObject.SetActive(true);
        }
        
        public IEnumerator AnimateByTime(UnitViewAnimParams animParams)
        {
            var startTime = Time.time;
            var timeLeft = animParams.time;
            
            var numberColor = new Color(number.color.r, number.color.g, number.color.b, animParams.aColor);
            var imageColor = new Color(image.color.r, image.color.g, image.color.b, animParams.aColor);
            var frameColor = new Color(frame.color.r, frame.color.g, frame.color.b, animParams.aColor);
            var maxFrameColor = new Color(maxFrame.color.r, maxFrame.color.g, maxFrame.color.b, animParams.aColor);
            var maxFrameCrownColor = new Color(maxFrameCrown.color.r, maxFrameCrown.color.g, maxFrameCrown.color.b, animParams.aColor);
            var selectLightColor = new Color(selectLight.color.r, selectLight.color.g, selectLight.color.b,
                animParams.aColor);

            var rotate = new Vector3(0, 0, animParams.endRotation);
            var scale = new Vector3(animParams.endScale, animParams.endScale, animParams.endScale);

            transform.eulerAngles = new Vector3 (0, 0, animParams.startRotation);
            transform.localScale = new Vector3 (animParams.startScale, animParams.startScale, animParams.startScale);
            
            while (startTime + animParams.time > Time.time)
            {
                var t = Time.deltaTime / timeLeft;
                number.color = Color.Lerp(number.color, numberColor, t);
                image.color = Color.Lerp(image.color, imageColor, t);
                frame.color = Color.Lerp(frame.color, frameColor, t);
                maxFrame.color = Color.Lerp(maxFrame.color, maxFrameColor, t);
                maxFrameCrown.color = Color.Lerp(maxFrame.color, maxFrameCrownColor, t);
                selectLight.color = Color.Lerp(selectLight.color, selectLightColor, t);
                transform.eulerAngles = Vector3.Lerp(transform.eulerAngles, rotate, t);
                transform.localScale = Vector3.Lerp(transform.localScale, scale, t);
                timeLeft -= Time.deltaTime;
                yield return null;
            }
            
            number.color = numberColor;
            image.color = imageColor;
            frame.color = frameColor;
            maxFrame.color = maxFrameColor;
            maxFrameCrown.color = maxFrameCrownColor;
            selectLight.color = selectLightColor;
            transform.eulerAngles = rotate;
            transform.localScale = scale;
        }

        public void SetMaxFrame(bool value)
        {
            if (maxFrame != null)
			{
            	maxFrame.gameObject.SetActive(value);
            }
            if (maxFrameCrown != null)
            {
            	maxFrameCrown.gameObject.SetActive(value);
            }
        }
        
        public void SetSkin(ESquareSkin skin)
        {
            switch (skin)
            {
                case ESquareSkin.baseSprite:
                    SetNewSkin(imageData.baseSprite, false);
                    break;
                case ESquareSkin.bubbleSprite:
                    SetNewSkin(imageData.bubbleSprite, false);
                    break;
                case ESquareSkin.candySprite:
                    SetNewSkin(imageData.candySprite, false);
                    break;
                case ESquareSkin.skySprite:
                    SetNewSkin(imageData.skySprite, false);
                    break;
                case ESquareSkin.woodSprite:
                    SetNewSkin(imageData.woodSprite, false);
                    break;
                case ESquareSkin.glassSprite:
                    SetNewSkin(imageData.glassSprite, false);
                    break;
                case ESquareSkin.normalSprite:
                    SetNewSkin(imageData.normalSprite, false);
                    break;
                case ESquareSkin.shineSprite:
                    SetNewSkin(imageData.shineSprite, false);
                    break;
                case ESquareSkin.external:
                    SetNewSkin(
                        imageData.external != null ? imageData.external : imageData.baseSprite,
                        imageData.externalFrame != null, imageData.externalFrame
                    );
                    break;
            }
        }
        
        private void SetNumberText(string text)
        {
            number.gameObject.SetActive(true);
            number.text = text;
        }

        private void SetNewSkin(Sprite sprite, bool frameActive, Sprite frameSprite = null)
        {
            SetAnchors(-0.01f, -0.01f, 1.01f, 1.01f);
            image.sprite = sprite;
            frame.gameObject.SetActive(frameActive);
            frame.sprite = frameSprite;
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
}