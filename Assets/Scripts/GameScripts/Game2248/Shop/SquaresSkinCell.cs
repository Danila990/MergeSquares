using System;
using Core.Anchors;
using Core.Localization;
using GameScripts.MergeSquares.Shop;
using LargeNumbers;
using TMPro;
using UnityEngine;
using Zenject;
using Image = UnityEngine.UI.Image;

namespace GameScripts.Game2248.Shop
{
    public class SquaresSkinCell : MonoBehaviour
    {
        [SerializeField] private Anchor anchor;
        [SerializeField] private UnitView unitView;
        [SerializeField] private Image frame;
        [SerializeField] private Image bottomImage;
        [SerializeField] private Image rarityBackground;
        [SerializeField] private Image rarityIcon;
        [SerializeField] private TextMeshProUGUI rarityName;
        public ESquareSkin Skin { get; private set; }
    
        private Action<ESquareSkin> Click = (skinEnum) => { };

        private bool _isOpened = false;
        
        private LocalizationRepository _localizationRepository;

        [Inject]
        private void Construct(LocalizationRepository localizationRepository)
        {
            _localizationRepository = localizationRepository;
        }
        
        public void OnClick()
        {
            Click.Invoke(Skin);
        }

        public void Init(int num, SquaresSkin skin, SkinRarity skinRarity, Action<ESquareSkin> OnClick)
        {
            Skin = skin.Skin;
            Click += OnClick;
            anchor.Id += num.ToString();
            if (rarityBackground != null)
            {
                rarityBackground.color = skinRarity.color;
            }

            if (rarityIcon != null)
            {
                rarityIcon.color = skinRarity.color;
            }

            if (rarityName != null)
            {
                rarityName.text = _localizationRepository.GetTextInCurrentLocale($"{skin.Rarity.ToString()}Name").Substring(0, 1);
            }

            SetUnitView();
            bottomImage.gameObject.SetActive(false);
        }

        public void SetOpened()
        {
            unitView.SetSecret(false);
            _isOpened = true;
        }

        public void Select(bool selected)
        {
            frame.gameObject.SetActive(selected);
            if(_isOpened)
            {
                bottomImage.gameObject.SetActive(!selected);
            }
        }

        private void SetUnitView()
        {
            unitView.gameObject.SetActive(true);
            unitView.Init(new LargeNumber(2));
            unitView.SetSkin(Skin);
            unitView.SetSecret(true);
        }
    }
}
