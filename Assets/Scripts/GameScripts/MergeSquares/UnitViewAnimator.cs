using DG.Tweening;
using System;
using System.Collections;
using Core.Audio;
using UnityEngine;
using Utils;

namespace GameScripts.MergeSquares
{
    public class UnitViewAnimator : MonoBehaviour
    {
        [SerializeField] private SoundSource crit;
        [SerializeField] private UnitView viewPrefab;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Canvas canvas;
        [SortingLayer]
        [SerializeField] private string overrideLayer;

        [Header("Scale")]
        [SerializeField] private float scaleDuration;
        [SerializeField] private float scaleCoefficient;

        [Header("Merge")]
        [SerializeField] private float mergeDuration;
        [SerializeField] private UnitViewDestroyableParticles mergeParticlesPrefab;

        [Header("Destroy")]
        [SerializeField] private float destroyDuration;
        [SerializeField] private UnitViewDestroyableParticles destroyParticlesPrefab;

        [Header("Upgrade")]
        [SerializeField] private float upgradeDuration;
        [SerializeField] private UnitViewDestroyableParticles upgradeParticlesPrefab;

        [Header("Critical")]
        [SerializeField] private float critPeriodDuration;
        [SerializeField] private float critScaleCoefficient;
        [SerializeField] private float critRotationAngle;
        [SerializeField] private AnimationCurve critRotationCurve;

        [Header("Flying")]
        [SerializeField] private float flyingDuration;

        [SerializeField] private RectTransform rect;
        [SerializeField] private UnitView unitView;

        private int _sortingOrder;
        private string _overrideLayer;
        private bool _overrideSorting;
        
        public float FlyingDuration => flyingDuration;

        private void Start()
        {
            if(canvas != null)
            {
                _sortingOrder = canvas.sortingOrder;
                _overrideSorting = canvas.overrideSorting;
                _overrideLayer = canvas.sortingLayerName;
            }
        }

        public IEnumerator AnimateCritical(int target)
        {
            do
            {
                crit.Play();
                AnimateScalePingPong();
                CreateSparksParticles();
                unitView.Init(unitView.Value * 2);
                // Debug.Log($"[UnitViewAnimator][AnimateCritical] Update value to: {unitView.Value} target: {target}");

                Tween myTween = transform
                    .DORotate(new Vector3(0, 0, 15), critPeriodDuration * 2)
                    .SetEase(critRotationCurve);
                yield return myTween.WaitForKill();
                // Debug.Log($"[UnitViewAnimator][AnimateCritical] After anim: {unitView.Value} target: {target} can continue:{(unitView.Value < target).ToString()}");
            }
            while (unitView.Value < target);
        }

        public void AnimateMerge(UnitView target, Action callback = null)
        {
            var overlayView = Instantiate(viewPrefab, gameObject.transform);
            overlayView.transform.localPosition = Vector3.zero;
            overlayView.Init(target.Value);
            overlayView.Animator.canvasGroup.alpha = 0f;

            overlayView.Animator.canvasGroup
                .DOFade(1f, mergeDuration);
            canvasGroup
                .DOFade(0, mergeDuration);
            gameObject.transform
                .DOLocalMove(target.transform.position, mergeDuration)
                .OnKill(() => callback?.Invoke());
        }

        public void AnimateFlying(Cell target, Action callback = null)
        {
            transform
                .DOScale(target.Rect.rect.width / rect.rect.width, flyingDuration);

            transform
                .DOMove(target.transform.position, flyingDuration)
                .OnKill(() => callback?.Invoke());
        }

        public void SetSorting(int order)
        {
            if(canvas != null)
            {
                canvas.overrideSorting = true;
                canvas.sortingOrder = order;
                canvas.sortingLayerName = overrideLayer;
            }
        }

        public void ResetSorting()
        {
            if(canvas != null)
            {
                canvas.sortingOrder = _sortingOrder;
                canvas.sortingLayerName = _overrideLayer;
                canvas.overrideSorting = _overrideSorting;
            }
        }

        public void AnimateFlying(Game2248.Cell target, Action callback = null)
        {
            transform
                .DOScale(target.Rect.rect.width / rect.rect.width, flyingDuration);

            transform
                .DOMove(target.transform.position, flyingDuration)
                .OnKill(() => callback?.Invoke());
        }

        public void AnimateDestroy(Action callback = null)
        {
            CreateSparksParticles();
            AnimateScalePingPong();

            canvasGroup
                .DOFade(0, destroyDuration)
                .OnKill(() => callback?.Invoke());
        }

        public void CreateSparksParticles()
        {
            Instantiate(mergeParticlesPrefab, transform);
        }

        public void AnimateUpgrade(int currentValue)
        {
            AnimateUpgradeOverlay(currentValue);
            Instantiate(upgradeParticlesPrefab, transform);
        }

        private void AnimateUpgradeOverlay(int value)
        {
            var overlay = Instantiate(viewPrefab, transform);
            overlay.Init(value);

            overlay.Animator.canvasGroup
                .DOFade(0f, upgradeDuration)
                .OnKill(() => Destroy(overlay));
        }

        public void AnimateScalePingPong()
        {
             var tweenerCore = transform
                .DOScale(scaleCoefficient, scaleDuration / 2)
                .SetEase(Ease.OutCubic)
                .SetLoops(2, LoopType.Yoyo);
             tweenerCore.onComplete += () => { transform.localScale = Vector3.one; };
        }
    }
}
