using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Advertising;
using Advertising.AnalyticsSignals;
using Core.Windows;
using DG.Tweening;
using GameScripts.Game2248;
using GameScripts.PointPanel;
using GameStats;
using LargeNumbers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Zenject;

public class SquareSliderParams
{
    public int targetPowNum;
    public int winBonus;
    public int giftBonus;
    public float delay;
    public bool delete = false;
    public Action ClosePopup;
}

public class SquareSlider : MonoBehaviour
{
    [SerializeField] protected PopupBase popupBase;
    [SerializeField] protected List<UnitView> units;
    [SerializeField] protected UnitView firstTarget;
    [SerializeField] protected RewardAdButton rewardAdButton;
    [SerializeField] protected Button closeButton;
    [SerializeField] protected List<Image> deleteIcons;
    [SerializeField] protected float animTime = 1f;
    [SerializeField] protected TextMeshProUGUI giftBonusText;
    [SerializeField] protected TextMeshProUGUI giftMultiplierText;
    [SerializeField] protected GameObject arrow;
    [SerializeField] protected RectTransform arrowField;
    [SerializeField] protected AnimationCurve curve;
    [SerializeField] protected float animationDuration = 2f;
    [SerializeField] protected float arrowYOffset = -0.5f;
    [SortingLayer]
    [SerializeField] protected string sortingLayerName;

    protected SquareSliderParams squareSliderParams;
    protected int adsMultiplier = 2;
    protected bool moveArrow = false;

    protected SignalBus _signalBus;
    protected WindowManager _windowManager;
    protected GameStatService _gameStatService;

    [Inject]
    private void Construct(GameStatService gameStatService, SignalBus signalBus, WindowManager windowManager)
    {
        _signalBus = signalBus;
        _gameStatService = gameStatService;
        _windowManager = windowManager;

        popupBase.ShowArgsGot += OnShowArgsGot;
        popupBase.Inited += OnInited;

        rewardAdButton.gameObject.SetActive(false);
        closeButton.gameObject.SetActive(false);
    }

    private void Start()
    {
        var newPos = new Vector3(arrowField.sizeDelta.x * curve.Evaluate(0), arrowField.sizeDelta.y * arrowYOffset, 0);
        arrow.transform.localPosition = newPos;
    }

    private void OnInited()
    {
        popupBase.Canvas.sortingLayerName = sortingLayerName;
    }
    
    private void OnDestroy()
    {
        popupBase.ShowArgsGot -= OnShowArgsGot;
        popupBase.Inited -= OnInited;
    }
    
    public void OnClick()
    {
        popupBase.CloseWindow();
        squareSliderParams.ClosePopup();
    }

    public void SetMultiplier(int multiplier)
    {
        adsMultiplier = multiplier;
        giftBonusText.text = $"+{squareSliderParams.giftBonus*adsMultiplier}";
        giftMultiplierText.text = $"X{adsMultiplier}";
    }
    
    public void GetGiftWithAds()
    {
        if(rewardAdButton.CanShow())
        {
            moveArrow = false;
            rewardAdButton.GetComponent<Button>().interactable = false;
            rewardAdButton.Rewarded += OnRewarded;
            rewardAdButton.Failed += OnFailed;
            rewardAdButton.ShowAd();
        }
    }

    protected virtual void OnShowArgsGot(object[] args)
    {
        if (args.Length > 0)
        {
            squareSliderParams = args.First() as SquareSliderParams;
            StartCoroutine(ShowSlider());
        }
    }

    private IEnumerator ShowSlider()
    {
        var startPos = arrow.transform.position;
        var newPos = new Vector3(curve.Evaluate(0),  startPos.y, startPos.z);
        arrow.transform.position = newPos;
        
        giftBonusText.text = $"+{squareSliderParams.giftBonus}";
        giftMultiplierText.text = $"X{adsMultiplier}";
        if (squareSliderParams.delete)
        {
            deleteIcons[0].gameObject.SetActive(true);
            var icon = deleteIcons[1];
            icon.gameObject.SetActive(true);
            var color = icon.color;
            DOTween.To(() => Color.clear, newColor =>
            {
                icon.color = newColor;
            }, color, 1f);
        }
        
        var startTime = Time.time + squareSliderParams.delay;
        var startI = 0;
        List<int> values = new List<int>(units.Count);

        var posNeg1 = squareSliderParams.targetPowNum - 1;
        if (posNeg1 >= 1)
        {
            var posNeg2 = posNeg1 - 1;
            if (posNeg2 >= 1)
            {
                values.Add(posNeg2);
            }
            else
            {
                deleteIcons[0].gameObject.SetActive(false);
                units[0].SetInvisible();
                units[0].SetSelectLight(false);
                startI = 1;
            }
            values.Add(posNeg1);
        }
        else
        {
            deleteIcons[0].gameObject.SetActive(false);
            deleteIcons[1].gameObject.SetActive(false);
            units[0].SetInvisible();
            units[0].SetSelectLight(false);
            units[1].SetInvisible();
            units[1].SetSelectLight(false);
            startI = 2;
        }
        
        values.Add(squareSliderParams.targetPowNum);
        for (int i = values.Count; i < units.Count; i++)
        {
            var newValue = values.Last() + 1;
            values.Add(newValue);
        }

        var valuesCounter = 0;
        for (int i = startI; i < Math.Min(units.Count, values.Count); i++)
        {
            units[i].Init(new LargeNumber(Math.Pow(2, values[valuesCounter])));
            valuesCounter++;
        }
        
        while (startTime > Time.time)
        {
            yield return null;
        }

        for (int i = 0; i < Math.Min(units.Count, values.Count); i++)
        {
            var nextPos = Vector3.one;
            if (i == 0)
            {
                nextPos = (firstTarget.gameObject.transform as RectTransform).position;
            }
            else
            {
                nextPos = (units[i - 1].gameObject.transform as RectTransform).position;
            }
            var t = units[i].gameObject.transform as RectTransform;
            var _positionTweener = BezierUtils.CreateTween(t, t.position, nextPos, animTime);
            _positionTweener.onComplete += () =>
            {
                AnimEnds();
            };
        }
    }

    private IEnumerator ArrowMove()
    {
        float t = 0;
        moveArrow = true;

        while (moveArrow)
        {
            if (t >= animationDuration)
            {
                t = 0;
            }

            var newPos = new Vector3(arrowField.sizeDelta.x * curve.Evaluate(t / animationDuration), arrowField.sizeDelta.y * arrowYOffset, 0);
            arrow.transform.localPosition = newPos;

            t += Time.deltaTime;
                
            yield return null;
        }
    }

    private void AnimEnds()
    {
        rewardAdButton.gameObject.SetActive(true);
        closeButton.gameObject.SetActive(true);
        StartCoroutine(ArrowMove());
    }

    private void OnRewarded()
    {
        var gettedBunus = squareSliderParams.giftBonus * adsMultiplier;
        _signalBus.Fire(new AdSignal(AdType.Rewarded, AdStatus.Completed, "Gift on SquareSlider"));
        _gameStatService.TryIncWithAnim(EGameStatType.Soft, gettedBunus);

        rewardAdButton.Rewarded -= OnRewarded;
        rewardAdButton.Failed -= OnFailed;

        if (_windowManager.TryShowAndGetWindow<CoinsAddPopup>(EPopupType.CoinsAdd.ToString(), out var coinsAddPopup))
        {
            coinsAddPopup.SetArgs(gettedBunus, () =>
            {
                squareSliderParams.ClosePopup();
                popupBase.CloseWindow();
            });
        }
    }

    private void OnFailed()
    {
        _signalBus.Fire(new AdSignal(AdType.Rewarded, AdStatus.Failed, "Square slider fail"));

        rewardAdButton.Rewarded -= OnRewarded;
        rewardAdButton.Failed -= OnFailed;
    }
}
