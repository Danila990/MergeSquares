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
using Tutorial;
using UnityEngine;
using Utils;
using Utils.Instructions;
using Zenject;
using Enum = System.Enum;

namespace GameScripts.Game2248.Shop
{
    public class SummonSkinsPopupParams
    {
        public int Count;
    }

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
        [SerializeField] private SkinView skinPrefab;
        [SerializeField] private PopupBase popupBase;
        [SerializeField] private SquaresSkinsManager skinsManager;
        [SerializeField] private FlexibleGridLayout gridLayout;
        [SerializeField] private List<GridLayoutParams> gridLayoutParams;
        [SerializeField] private LocalizationRepository _localizationRepository;
        [Space]
        [SerializeField] private float animSpaceTime = 0.2f;

        public Action EndSummon = () => { };
        
        private SummonSkinsPopupParams _popupParams;
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
            var param = gridLayoutParams.Find(p => p.count == count);
            if(param != null) 
                gridLayout.SetSize(param.rows, param.columns);
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
            // wait for last finished
            // yield return new WaitForCallback(callback => { cell.view.Animator.AnimateDestroy(callback); });
            UpdateSkinsCells();
            _busy = false;
            EndSummon.Invoke();
        }

        private void CheckSkin(List<SummonSkinChance> openedSkins, SummonSkinChance newSkin)
        {
            var skin = newSkin.type;
            if (!_gridManager.OpenedSkins.Contains(skin))
            {
                _gridManager.OpenSkin(skin);
                openedSkins.Add(new SummonSkinChance(newSkin));
                _gridManager.SaveGrid();
            }
        }

        private List<SummonSkinChance> CollectChances()
        {
            var res = new List<SummonSkinChance>();
            var level = _gameStatLeveled.GetLevel(levelType);
            foreach (var skin in skinsManager.Skins)
            {
                var rarity = skinsManager.GetRarity(skin.Rarity);
                res.Add(new SummonSkinChance
                {
                    type = skin.Skin,
                    weight = rarity.chanceForLevel[level],
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
                _popupParams = args.First() as SummonSkinsPopupParams;
            }
        }

        private void UpdateSkinsCells()
        {
            if(_squaresShop != null)
                _squaresShop.UpdateSkinsCells();
        }
    }
}