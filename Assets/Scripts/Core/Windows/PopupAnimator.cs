using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopupAnimator : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float duration;
    [SerializeField] private AnimationCurve curve;
    [SerializeField] private Transform animatedTransform;
    [SerializeField] private List<Button> interactables = new(); 

    public void AnimateShow(Action callback = null)
    {
        SetInteractable(false);
        StartCoroutine(AnimateScalePingPong());
        canvasGroup.alpha = 0f;
        canvasGroup
            .DOFade(1f, duration)
            .OnKill(() =>
            {
                SetInteractable(true);
                callback?.Invoke();
            });
    }

    public void AnimateHide(Action callback = null)
    {
        SetInteractable(false);
        StartCoroutine(AnimateScalePingPong());
        canvasGroup
            .DOFade(0f, duration)
            .OnKill(() =>
            {
                SetInteractable(true);
                callback?.Invoke();
            });
    }

    private IEnumerator AnimateScalePingPong()
    {
        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            animatedTransform.localScale = Vector3.one * curve.Evaluate(t / duration);
            yield return null;
        }
    }

    private void SetInteractable(bool interactable)
    {
        foreach (var item in interactables)
            item.interactable = interactable;
    }
}
