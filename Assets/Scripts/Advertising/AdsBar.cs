using System;
using System.Collections;
using System.Collections.Generic;
using Advertising;
using Advertising.AnalyticsSignals;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class AdsBar : MonoBehaviour
{
    [SerializeField] private AdsBarPart partPrefab;
    [SerializeField] private Transform barsParent;
    [SerializeField] private RewardAdButton rewardAdButton;
    [SerializeField] private Button getRewardButton;

    private List<AdsBarPart> adsBarParts = new List<AdsBarPart>();
    private int adsCount = 1;
    private int watchedAds = 0;

    private Action onGetReward = () => { };
    
    private SignalBus _signalBus;
    
    [Inject]
    private void Construct(SignalBus signalBus)
    {
        _signalBus = signalBus;
    }

    public void Init(int adsCount, Action getReward)
    {
        this.adsCount = adsCount;
        if (adsCount == 1)
        {
            barsParent.gameObject.SetActive(false);
        }
        else
        {
            for (int i = 0; i < adsCount; i++)
            {
                var bar = Instantiate(partPrefab, barsParent);
                adsBarParts.Add(bar);
            }
        }

        SetBars();
        onGetReward += getReward;
    }

    public void GetReward()
    {
        onGetReward.Invoke();
    }

    public void SetAbleGetReward()
    {
        rewardAdButton.gameObject.SetActive(false);
        getRewardButton.gameObject.SetActive(true);
    }

    public void WatchAds()
    {
        if(rewardAdButton.CanShow())
        {
            rewardAdButton.GetComponent<Button>().interactable = false;
            rewardAdButton.Rewarded += OnRewarded;
            rewardAdButton.Failed += OnFailed;
            rewardAdButton.ShowAd();
        }
    }

    private void SetBars()
    {
        for (int i = 0; i < adsBarParts.Count; i++)
        {
            if (i < watchedAds)
            {
                adsBarParts[i].SetWatched();
            }
            else if (i == watchedAds)
            {
                adsBarParts[i].SetNext();
            }
            else
            {
                adsBarParts[i].SetNotWatched();
            }
        }
    }
    
    private void OnRewarded()
    {
        _signalBus.Fire(new AdSignal(AdType.Rewarded, AdStatus.Completed, "Gift on AdsBar"));
        rewardAdButton.GetComponent<Button>().interactable = true;
        rewardAdButton.Rewarded -= OnRewarded;
        rewardAdButton.Failed -= OnFailed;
        
        watchedAds++;
        SetBars();
        if (watchedAds >= adsCount)
        {
            SetAbleGetReward();
        }
    }

    private void OnFailed()
    {
        _signalBus.Fire(new AdSignal(AdType.Rewarded, AdStatus.Failed, "Failed AdsBar"));
        rewardAdButton.GetComponent<Button>().interactable = true;
        rewardAdButton.Rewarded -= OnRewarded;
        rewardAdButton.Failed -= OnFailed;
    }
}
