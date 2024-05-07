using System;
using Core.Audio;
using Core.Windows;
using TMPro;
using UnityEngine;

public class CoinsAddPopup : MonoBehaviour
{
    [SerializeField] private SoundSource bonus;
    [SerializeField] private TextMeshProUGUI addCoinsText;
    [SerializeField] private PopupBase popupBase;
    
    public void PlayBonusSound() => bonus.Play();

    public void SetArgs(int addCount, Action OnClose)
    {
        addCoinsText.text = "+" + addCount.ToString();
        popupBase.BeforeCloseWindow += OnClose;
        PlayBonusSound();
    }
}
