using System;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace GameScripts.PointPanel
{
    public enum EPointState
    {
        Fail = 0,
        WrongPlace = 1,
        Win = 2
    }
    public class PointCell : MonoBehaviour
    {
        [SerializeField] private Image pointImage;
        [SerializeField] private Image stateImage;
        [SerializeField] private Image hatImage;
        [SerializeField] private Image highlightImage;
        [SerializeField] private Animator animator;

        [SerializeField] private Sprite fail;
        [SerializeField] private Sprite wrongPlace;
        [SerializeField] private Sprite win;

        public RectTransform TargetTransform => pointImage.transform as RectTransform;
    
        public Action animationEnd;
    
        private EPointId EColorId;
        
        private ColorRepTranslator _сolorRepTranslator;
        private global::GameScripts.PointPanel.PointPanel _pointPanel;

        [Inject]
        private void Construct(ColorRepTranslator colorRepTranslator, global::GameScripts.PointPanel.PointPanel pointPanel)
        {
            _сolorRepTranslator = colorRepTranslator;
            _pointPanel = pointPanel;
        }

        public void SetSkin(BallSkin ballSkin)
        {
            var skin = _сolorRepTranslator.AllPointsList[EColorId];
            if (EColorId != EPointId.None)
            {
                pointImage.sprite = skin.sprites.Find(point => point.skinPointId == ballSkin.skinId).sprite;
            }

            if (ballSkin.isHat)
            {
                hatImage.gameObject.SetActive(pointImage.gameObject.activeInHierarchy);
                hatImage.transform.localPosition = ballSkin.hatOffset;
                hatImage.sprite = skin.hats.Find(point => point.hatPointId == ballSkin.hatId).sprite;
                hatImage.SetNativeSize();
            }
            else
            {
                hatImage.gameObject.SetActive(false);
            }
        }

        public void SetPointColor(EPointId pointId)
        {
            highlightImage.gameObject.SetActive(true);

            EColorId = pointId;
            pointImage.gameObject.SetActive(true);

            var currentSkin = _pointPanel.CurrentBallSkin;
            pointImage.sprite = _сolorRepTranslator.AllPointsList[EColorId].sprites.Find(point => point.skinPointId == currentSkin.skinId).sprite;
            highlightImage.sprite = _сolorRepTranslator.AllPointsList[EColorId].highlight;
            hatImage.gameObject.SetActive(currentSkin.isHat);
        }

        public void SetState(EPointState state)
        {
            highlightImage.gameObject.SetActive(true);
            stateImage.gameObject.SetActive(true);
            switch (state)
            {
                case EPointState.Fail:
                    stateImage.sprite = fail;
                    _pointPanel.PlayWrongBallSound();
                    break;
                case EPointState.WrongPlace:
                    stateImage.sprite = wrongPlace;
                    _pointPanel.PlayWrongBallSound();
                    break;
                case EPointState.Win:
                    stateImage.sprite = win;
                    _pointPanel.PlayNextCorrectSound();
                    break;
            }
            animator.SetTrigger("setState");
        }

        public void Hide()
        {
            stateImage.color = new Color(0, 0, 0, 0);
            pointImage.color = Color.white;
            gameObject.SetActive(false);
        }

        public void OnAnimationEnd()
        {
            animationEnd?.Invoke();
        }
    }
}