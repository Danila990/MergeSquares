using System;
using Core.Audio;
using Core.Windows;
using GameScripts.Game2248;
using GameScripts.Game2248.Shop;
using TMPro;
using UnityEngine;

public class SkinAddPopup : MonoBehaviour
{
    [SerializeField] private SoundSource bonus;
    [SerializeField] private SkinView skinView;
    [SerializeField] private PopupBase popupBase;
    
    public void PlayBonusSound() => bonus.Play();

    public void SetArgs(ESquareSkin skin, Color color, string rarityText, Action OnClose)
    {
        skinView.Init(skin, color, rarityText, "", true);
        popupBase.BeforeCloseWindow += OnClose;
        PlayBonusSound();
    }
}
