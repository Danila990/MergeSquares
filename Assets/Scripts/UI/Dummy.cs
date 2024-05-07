using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using GameScripts.PointPanel;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Zenject;

namespace UI
{
    public class Dummy : MonoBehaviour
    {
        [SerializeField] private Image pointImage;
        [SerializeField] private Image hatImage;
        [SerializeField] private Image highlightImage;

        [SerializeField] private float animTime = 1f;
        public event Action PointReached = () => { };

        private Vector2 _startPosition;
        private Vector2 _finishPosition;
        private Vector2 _startSizeDelta;
        private Vector2 _finishSizeDelta;

        private TweenerCore<float, float, FloatOptions> _positionTweener;
        private TweenerCore<Vector2, Vector2, VectorOptions> _rectTweener;
        
        public void SetStartPosition(Vector2 position) => _startPosition = position;
        public void SetFinishPosition(Vector2 position) => _finishPosition = position;
        public void SetStartSizeDelta(Vector2 scale) => _startSizeDelta = scale;
        public void SetFinishSizeDelta(Vector2 scale) => _finishSizeDelta = scale;

        private ColorRepTranslator _сolorRepTranslator;
        
        [Inject]
        private void Construct(ColorRepTranslator colorRepTranslator)
        {
            _сolorRepTranslator = colorRepTranslator;
        }
        
        public void Launch(float range = 0.5f, float centerOffset = 0)
        {
            var t = transform;
            t.position = _startPosition;
            if (t is RectTransform rt)
            {
                rt.sizeDelta = _startSizeDelta;
                _rectTweener = rt.DOSizeDelta(_finishSizeDelta, animTime);
            }
            _positionTweener = BezierUtils.CreateTween(t, t.position, _finishPosition, animTime, range, centerOffset);
            _positionTweener.onComplete += () => {
                PointReached.Invoke();
                Destroy(gameObject);
            };
        }

        public void SetSkin(BallSkin ballSkin, EPointId pointId)
        {
            pointImage.sprite = _сolorRepTranslator.AllPointsList[pointId].sprites.Find(point => point.skinPointId == ballSkin.skinId).sprite;
            highlightImage.sprite = _сolorRepTranslator.AllPointsList[pointId].highlight;

            if (ballSkin.isHat)
            {
                hatImage.gameObject.SetActive(true);
                hatImage.transform.localPosition = ballSkin.hatOffset;
                hatImage.sprite = _сolorRepTranslator.AllPointsList[pointId].hats.Find(point => point.hatPointId == ballSkin.hatId).sprite;
                hatImage.SetNativeSize();
            }
            else
            {
                hatImage.gameObject.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            _rectTweener?.Complete();
            _positionTweener?.Complete();
        }
    }
}
