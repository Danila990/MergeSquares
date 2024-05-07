using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Localization;
using Core.Windows;
using DG.Tweening;
using GameScripts.MergeSquares.Shop;
using GameStats;
using Mono.CSharp;
using Shop;
using TMPro;
using Tutorial;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Utils.Instructions;
using Zenject;
using Enum = System.Enum;

namespace GameScripts.MergeSquares.Shop
{
    [Serializable]
    public class SummonSkinChance
    {
        public float weight;
        public ESquareSkin type;
        public SkinRarity rarity;

        public SummonSkinChance(){}
        public SummonSkinChance(SummonSkinChance summonedSkin)
        {
            weight = summonedSkin.weight;
            type = summonedSkin.type;
            rarity = summonedSkin.rarity;
        }
    }

    [Serializable]
    public class GridLayoutParams
    {
        public int count;
        public int columns;
        public int rows;
    }

    public class SummonSkinsPopup : MonoBehaviour
    {
        [SerializeField] private EGameStatType levelType;
        [SerializeField] private Transform root;
        [SerializeField] private Transform largeRoot;
        [SerializeField] private GameObject largePanel;
        [SerializeField] private List<RarityCount> rarityCounts;
        [SerializeField] private SkinView skinPrefab;
        [SerializeField] private PopupBase popupBase;
        [SerializeField] private SquaresSkinsManager skinsManager;
        [SerializeField] private FlexibleGridLayout gridLayout;
        [SerializeField] private List<GridLayoutParams> gridLayoutParams;
        [SerializeField] private LocalizationRepository _localizationRepository;
        [SerializeField] private Button summonSmall;
        [SerializeField] private Button summonLarge;
        [SerializeField] private Transform additionButtonsRoot;
        // [SerializeField] private GameObject additionButtonsScroll;
        [SerializeField] private SummonSkinsButton buttonPrefab;
        [SerializeField] private SkinLevelsView skinLevelsView;
        [SerializeField] private TextMeshProUGUI cashbackCount;
        [Space]
        [SerializeField] private float animSpaceTime = 0.2f;

        public Action EndSummon = () => { };
        
        private GameScripts.Game2248.Shop.SummonSkinsPopupParams _popupParams;
        private List<SummonSkinsButton> additionButtons = new List<SummonSkinsButton>();
        private List<SkinView> _skins = new();
        private bool _busy;
        private SquaresShop _squaresShop;
    
        private GameStatLeveled _gameStatLeveled;
        private GridManager _gridManager;
        private WindowManager _windowManager;
        private TutorialService _tutorialService;
        private GameStatService _gameStatService;

        [Inject]
        private void Construct(GameStatLeveled gameStatLeveled, GridManager gridManager, WindowManager windowManager,
            TutorialService tutorialService, GameStatService gameStatService)
        {
            _gameStatLeveled = gameStatLeveled;
            _gridManager = gridManager;
            _windowManager = windowManager;
            _tutorialService = tutorialService;
            _gameStatService = gameStatService;
            popupBase.ShowArgsGot += OnShowArgsGot;
            popupBase.Inited += OnInited;

            _windowManager.TryGetWindow(EPopupType.Shop.ToString(), out _squaresShop);
        }

        private void OnDestroy()
        {
            UpdateSkinsCells();
            popupBase.ShowArgsGot -= OnShowArgsGot;
            popupBase.Inited -= OnInited;
        }

        public bool CanSummon() => !_busy;

        public void Summon(int count, List<ESquareSkin> readySkins = null)
        {
            _busy = true;
            foreach (var button in additionButtons)
            {
                Destroy(button.gameObject);
            }
            additionButtons.Clear();
            summonSmall.interactable = false;
            summonLarge.interactable = false;
            if (readySkins == null || readySkins.Count == 0)
            {
                StartCoroutine(SummonSkins(count));
            }
            else
            {
                var readySkinsChances = new List<SummonSkinChance>();
                var skinsList = skinsManager.Skins.ToList();
                foreach (var readySkin in readySkins)
                {
                    var skin = skinsList.Find(a => a.Skin == readySkin);
                    var rarity = skinsManager.GetRarity(skin.Rarity);
                    readySkinsChances.Add(new SummonSkinChance
                    {
                        type = skin.Skin,
                        weight = rarity.chanceForLevel[0],
                        rarity = rarity
                    });
                }
                StartCoroutine(SummonSkins(count, readySkinsChances));
            }
        }

        private void SetAdditonButtons()
        {
            var count = 0;
            var softCount = _gameStatService.GetStatValue(EGameStatType.Soft);
            for (int i = 2; i < 10; i++)
            {
                var cost = skinLevelsView.LargeCostAmount * i;
                if (cost <= softCount)
                {
                    var skinsCount = skinLevelsView.SummonLargeAmount * i;
                    var newButton = Instantiate(buttonPrefab, additionButtonsRoot);
                    newButton.Init(skinsCount, cost);
                    newButton.Clicked += SummonSkinsByAddition;
                    additionButtons.Add(newButton);
                }
                else
                {
                    break;
                }

                count++;
            }
            // additionButtonsScroll.SetActive(count > 0);
        }

        private void SummonSkinsByAddition(SummonSkinsButton skinsButton)
        {
            skinsButton.Clicked -= SummonSkinsByAddition;

            if (CanSummon())
            {
                var cost = skinsButton.Cost;
                if (_gameStatService.TryDecWithAnim(EGameStatType.Soft, cost))
                {
                    _gameStatService.TryIncWithAnim(skinLevelsView.ExperienceType, skinsButton.Count);
                    Summon(skinsButton.Count);
                }
                else
                {
                    SquaresShop.OpenSection(_windowManager, EShopMarkers.InApps);
                }
            }
        }

        private IEnumerator SummonSkins(int count, List<SummonSkinChance> readySkins = null)
        {
            var newSkins = new List<SummonSkinChance>();
            var openedSkins = new List<SummonSkinChance>();
            if (readySkins == null)
            {
                var chances = CollectChances();
                for (var i = 0; i < count; i++)
                {
                    if (chances.TryWeightRandom(s => s.weight, out var summonedSkin))
                    {
                        newSkins.Add(new SummonSkinChance(summonedSkin));
                        CheckSkin(openedSkins, summonedSkin);
                    }
                }
            }
            else
            {
                newSkins = readySkins;
                foreach (var newSkin in newSkins)
                {
                    CheckSkin(openedSkins, newSkin);
                }
            }
            
            if (_skins.Count > 0)
            {
                // Start remove anim (wait for last finished)
                // yield return new WaitForCallback(callback => { cell.view.Animator.AnimateDestroy(callback); });
                // All destroyed
                for (int i = 0; i < _skins.Count - 1; i++)
                {
                    _skins[i].AnimateDestroy(()=> {});
                }
                yield return new WaitForCallback(callback => { _skins.Last().AnimateDestroy(callback); });
                foreach (var skin in _skins)
                {
                    Destroy(skin.gameObject);
                }
                _skins.Clear();
            }

            if (count > skinLevelsView.SummonLargeAmount)
            {
                root.gameObject.SetActive(false);
                largePanel.gameObject.SetActive(true);
                yield return ShowSkinsForMany(newSkins, openedSkins);
            }
            else
            {
                root.gameObject.SetActive(true);
                largePanel.gameObject.SetActive(false);
                var param = gridLayoutParams.Find(p => p.count == count);
                if(param != null) 
                    gridLayout.SetSize(param.rows, param.columns);
                else
                {
                    var gridSize = (int)Math.Ceiling(Math.Sqrt(count));
                    gridLayout.SetSize(gridSize, gridSize);
                }
                yield return ShowSkinsForPreButtons(newSkins, openedSkins);
            }

            
            // wait for last finished
            // yield return new WaitForCallback(callback => { cell.view.Animator.AnimateDestroy(callback); });
            UpdateSkinsCells();
            _busy = false;
            StartCoroutine(WaitToActivateButtons());
            EndSummon.Invoke();
        }

        private IEnumerator ShowSkinsForPreButtons(List<SummonSkinChance> newSkins, List<SummonSkinChance> openedSkins)
        {
            foreach (var newSkin in newSkins)
            {
                var skin = Instantiate(skinPrefab, root);
                var saveScale = skin.transform.localScale;
                skin.transform.localScale = Vector3.zero;
                var text = _localizationRepository.GetTextInCurrentLocale($"{newSkin.rarity.rarity.ToString()}Name").Substring(0, 1);

                bool notOpened = openedSkins.GetBy(s => s.type == newSkin.type) != null;
                string cashBackText = "";
                if (!notOpened)
                {
                    var cashBack = newSkin.rarity.cashBack;
                    cashBackText = $"+{cashBack}";
                    _gameStatService.TryInc(EGameStatType.Soft, cashBack);
                }
                skin.Init(newSkin.type, newSkin.rarity.color, text, cashBackText, notOpened);
                _skins.Add(skin);
                // TODO: Add anim
                var move = DOTween.To(() => Vector3.zero, newScale =>
                {
                    skin.transform.localScale = newScale;
                }, saveScale, 1f);

                if (newSkin == newSkins.Last())
                {
                    move.OnKill(() =>
                    {
                        skin.StartNotOpenAnimation();
                        foreach (var skin in _skins)
                        {
                            skin.SetCashback();
                        }
                    });
                }
                else
                {
                    move.OnKill(() =>
                    {
                        skin.StartNotOpenAnimation();
                    });
                }


                yield return new WaitForSeconds(animSpaceTime);
            }
        }
        
        private IEnumerator ShowSkinsForMany(List<SummonSkinChance> newSkins, List<SummonSkinChance> openedSkins)
        {
            var time = (animSpaceTime * 10) / newSkins.Count();
            foreach (var view in rarityCounts)
            {
                view.gameObject.SetActive(false);
            }

            var cashBack = 0;
            Dictionary<ESkinRarity, int> summonedRarityCounts = new Dictionary<ESkinRarity, int>();
            
            foreach (var newSkin in newSkins)
            {
                var text = _localizationRepository.GetTextInCurrentLocale($"{newSkin.rarity.rarity.ToString()}Name").Substring(0, 1);

                bool notOpened = openedSkins.GetBy(s => s.type == newSkin.type) != null;
                string cashBackText = "";
                if (!notOpened)
                {
                    cashBack += newSkin.rarity.cashBack;
                    cashBackText = $"+{cashBack}";
                    _gameStatService.TryInc(EGameStatType.Soft, newSkin.rarity.cashBack);
                    cashbackCount.text = cashBackText;
                }
                else
                {
                    var skin = Instantiate(skinPrefab, largeRoot);
                    // var saveScale = skin.transform.localScale;
                    // skin.transform.localScale = Vector3.zero;
                    skin.Init(newSkin.type, newSkin.rarity.color, text, "", true);
                    _skins.Add(skin);
                }
                foreach (var view in rarityCounts)
                {
                    var rarity = newSkin.rarity.rarity;
                    if (view.Rarity == rarity)
                    {
                        if (summonedRarityCounts.ContainsKey(rarity))
                        {
                            summonedRarityCounts[rarity]++;
                        }
                        else
                        {
                            summonedRarityCounts.Add(rarity, 1);
                        }
                        view.gameObject.SetActive(true);
                        view.SetCount($"+{summonedRarityCounts[rarity]}");
                    }
                }

                // var move = DOTween.To(() => Vector3.zero, newScale =>
                // {
                //     skin.transform.localScale = newScale;
                // }, saveScale, 1f);

                // if (newSkin == newSkins.Last())
                // {
                //     move.OnKill(() =>
                //     {
                //         skin.StartNotOpenAnimation();
                //         foreach (var skin in _skins)
                //         {
                //             skin.SetCashback();
                //         }
                //     });
                // }
                // else
                // {
                //     move.OnKill(() =>
                //     {
                //         skin.StartNotOpenAnimation();
                //     });
                // }

                yield return new WaitForSeconds(time);
            }
        }

        private IEnumerator WaitToActivateButtons()
        {
            yield return new WaitForSeconds(1.5f);
            summonSmall.interactable = true;
            summonLarge.interactable = true;
            SetAdditonButtons();
        }

        private void CheckSkin(List<SummonSkinChance> openedSkins, SummonSkinChance newSkin)
        {
            var skin = newSkin.type;
            if (_gridManager.OpenedSkins.Find(s => s.skinType == skin) == null)
            {
                _gridManager.OpenSkin(skin);
                openedSkins.Add(new SummonSkinChance(newSkin));
                _gridManager.SaveGrid();
            }
            else
            {
                _gridManager.AddSkin(skin);
            }
        }

        private List<SummonSkinChance> CollectChances()
        {
            var res = new List<SummonSkinChance>();
            var level = _gameStatLeveled.GetLevel(levelType);
            var amount = new Dictionary<SkinRarity, float>();
            foreach (var skin in skinsManager.Skins)
            {
                var rarity = skinsManager.GetRarity(skin.Rarity);
                if (!amount.ContainsKey(rarity))
                {
                    amount.Add(rarity, 1);
                }
                else
                {
                    switch (rarity.rarity)
                    {
                        case ESkinRarity.Uncommon:
                            amount[rarity] += 40;
                            break;
                        case ESkinRarity.Rare:
                            amount[rarity] += 20;
                            break;
                        case ESkinRarity.Epic:
                            amount[rarity] += 20;
                            break;
                        case ESkinRarity.Legendary:
                            amount[rarity] += 2;
                            break;
                    }
                }
                
                res.Add(new SummonSkinChance
                {
                    type = skin.Skin,
                    weight = rarity.chanceForLevel[level] / amount[rarity],
                    rarity = rarity
                });
            }
            
            return res;
        }

        private void OnInited()
        {
            if (_popupParams != null && !_tutorialService.HasActiveTutorial)
            {
                Summon(_popupParams.Count);
            }
        }

        private void OnShowArgsGot(object[] args)
        {
            if(args.Length > 0)
            {
                _popupParams = args.First() as GameScripts.Game2248.Shop.SummonSkinsPopupParams;
            }
        }

        private void UpdateSkinsCells()
        {
            if(_squaresShop != null)
                _squaresShop.UpdateSkinsCells();
        }
    }
}