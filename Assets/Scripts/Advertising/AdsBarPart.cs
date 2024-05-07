using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AdsBarPart : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private Image adsIcon;
    [SerializeField] private Image noAdsIcon;

    public void SetNotWatched()
    {
        adsIcon.gameObject.SetActive(false);
        noAdsIcon.gameObject.SetActive(false);
        slider.gameObject.SetActive(false);
    }

    public void SetWatched()
    {
        adsIcon.gameObject.SetActive(true);
        noAdsIcon.gameObject.SetActive(false);
        slider.gameObject.SetActive(true);
        slider.value = 1f;
    }
    
    public void SetNext()
    {
        adsIcon.gameObject.SetActive(false);
        noAdsIcon.gameObject.SetActive(true);
        slider.gameObject.SetActive(true);
        slider.value = 0.5f;
    }
}
