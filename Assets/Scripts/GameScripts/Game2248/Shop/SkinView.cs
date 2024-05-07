using System;
using System.Collections;
using System.Collections.Generic;
using Core.Repositories;
using DG.Tweening;
using GameScripts.MergeSquares.Shop;
using LargeNumbers;
using TMPro;
using UnityEngine;
using Image = UnityEngine.UI.Image;

namespace GameScripts.Game2248.Shop
{
    public class SkinView : MonoBehaviour
    {
        [SerializeField] private UnitView view;
        [SerializeField] private Image rarityIcon;
        [SerializeField] private Image frame;
        [SerializeField] private TextMeshProUGUI rarityName;
        [SerializeField] private TextMeshProUGUI cashBackText;
        [SerializeField] private GameObject num;
        [SerializeField] private GameObject cashBackObject;
        [SerializeField] private AnimationCurve notOpenAnimationCurve;
        [SerializeField] private ParticleSystem particleSystem;
        
        private bool _notOpened;
        private string _cashBackString;

        public void Init(ESquareSkin skin, Color color, string rarityText, string cashBackText, bool notOpened)
        {
            _cashBackString = cashBackText;
            _notOpened = notOpened;
            if (notOpened)
                particleSystem.Play();
            view.Init(new LargeNumber(2));
            view.SetSkin(skin);
            view.SetSecret(false);
            
            rarityIcon.color = color;
            rarityName.text = rarityText;
            if (notOpened)
            {
                frame.gameObject.SetActive(true);
                frame.color = color;
            }
            else
            {
                frame.gameObject.SetActive(false);
            }
        }

        public void StartNotOpenAnimation()
        {
            if(!_notOpened)
                return;
             StartCoroutine(AnimateNotOpened());
        }

        public void AnimateDestroy(Action callback)
        {
            var move = DOTween.To(() => transform.localScale, newScale =>
            {
                transform.localScale = newScale;
            }, Vector3.zero, 0.5f);
            move.OnKill(() =>
            {
                callback.Invoke();
            });
        }

        public void SetCashback()
        {
            if(_notOpened)
                return;
            cashBackObject.SetActive(true);
            num.SetActive(false);
            cashBackText.text = _cashBackString;
        }
        
        private IEnumerator AnimateNotOpened()
        {
            float t = 0;
            while (gameObject.activeInHierarchy)
            {
                if (t >= 1)
                {
                    t = 0;
                }
                transform.localScale = Vector3.one * notOpenAnimationCurve.Evaluate(t);
                t += Time.deltaTime;
                yield return null;
            }
        }
    }
}