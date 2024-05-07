using System;
using Core.Anchors;
using UnityEngine;
using Zenject;
using Button = UnityEngine.UI.Button;
using Image = UnityEngine.UI.Image;

namespace GameScripts.PointPanel
{
    public class BottomCell : MonoBehaviour
    {
        [SerializeField] private Image pointImage;
        [SerializeField] private Image highlightImage;
        [SerializeField] private Image hatImage;
        [SerializeField] private GameObject check;

        [SerializeField] private Button button;
        [SerializeField] private Canvas canvas;
        [SerializeField] private Anchor _anchor;
    
        public Action<EPointId, RectTransform> clicked;
        public event Action<BottomCell> tutorialClicked;

        public EPointId PointId { get; set; }
        public bool WasClicked { get; private set; }
        public Anchor Anchor => _anchor;

        private ColorRepTranslator _сolorRepTranslator;
        private global::GameScripts.PointPanel.PointPanel _pointPanel;

        [Inject]
        private void Construct(ColorRepTranslator colorRepTranslator, global::GameScripts.PointPanel.PointPanel pointPanel)
        {
            _сolorRepTranslator = colorRepTranslator;
            _pointPanel = pointPanel;
        }

        public void OnClick()
        {
            clicked.Invoke(PointId, transform as RectTransform);
            tutorialClicked?.Invoke(this);
            WasClicked = true;
        }

        public void SetClickable(bool isClickable)
        {
            button.enabled = isClickable;
        }

        public void SetOverrideSorting(bool isOverride)
        {
            canvas.overrideSorting = isOverride;
        }

        public void SetDisabledColor()
        {
            check.SetActive(true);
            SetColor(EPointId.None);
            SetSkin(_pointPanel.CurrentBallSkin);
        }

        public void SetSkin(BallSkin ballSkin)
        {
            var skin = _сolorRepTranslator.AllPointsList[PointId];
            pointImage.sprite = skin.sprites.Find(point => point.skinPointId == ballSkin.skinId).sprite;
            highlightImage.sprite = _сolorRepTranslator.AllPointsList[PointId].highlight;
            if (ballSkin.isHat)
            {
                hatImage.gameObject.SetActive(true);
                hatImage.transform.localPosition = ballSkin.hatOffset;
                hatImage.sprite = skin.hats.Find(point => point.hatPointId == ballSkin.hatId).sprite;
                hatImage.SetNativeSize();
            }
            else
            {
                hatImage.gameObject.SetActive(false);
            }
        }
    
        public void SetColor(EPointId pointId)
        {
            PointId = pointId;
            pointImage.sprite = _сolorRepTranslator.AllPointsList[PointId].sprites.Find(point => point.skinPointId == _pointPanel.CurrentBallSkin.skinId).sprite;
        }
    }
}
