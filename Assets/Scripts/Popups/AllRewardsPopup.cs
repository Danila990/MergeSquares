using System;
using System.Collections;
using System.Collections.Generic;
using Core.Windows;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace GameScripts.MergeSquares.InfinityLevel
{
    public class AllRewardsParams
    {
        public int position;
        public Func<bool, int, List<RewardData>> GetRewards;
        public bool isMonth;
    }
    public class AllRewardsPopup : GenericPopupContent
    {
        [SerializeField] private RewardLine rewardLinePrefab;
        [SerializeField] private Transform root;
        [SerializeField] private ScrollRect _scrollRect;

        private RatingService _ratingService;
        
        [Inject]
        private void Construct(WindowManager windowManager)
        {
            _windowManager = windowManager;
        }
        
        public override string GetWindowId() => "AllRewardsPopup";

        public override void Init(object dataToInit, PopupBase popupBase)
        {
            int playerPos = -1;
            if (dataToInit is AllRewardsParams rewardsParams)
            {
                playerPos = rewardsParams.position;


                _ratingService = ZenjectBinding.FindObjectOfType<RatingService>();
                var rewardsList = _ratingService.WeekRewards;
                RectTransform playerPlacePosition = null;
                List<RewardData> rewardsMin;
                List<RewardData> rewardsMax;
                // var prevStep = -1;

                for (int i = rewardsList.Count - 1; i >= 0; i--)
                {
                    var rewardData = rewardsList[i];
                    if (rewardData.step <= 0)
                        continue;

                    // rewardsMin = _ratingService.CalcRewards(false, rewardData.placeInRatingLess);
                    // rewardsMax = _ratingService.CalcRewards(false, rewardData.placeInRatingMore);

                    rewardsMin = rewardsParams.GetRewards(rewardsParams.isMonth, rewardData.placeInRatingLess);
                    rewardsMax = rewardsParams.GetRewards(rewardsParams.isMonth, rewardData.placeInRatingMore);

                    var rewardLine = Instantiate(rewardLinePrefab, root);
                    string text = string.Empty;

                    if (rewardData.placeInRatingLess == rewardData.placeInRatingMore)
                        text = rewardData.placeInRatingLess.ToString();
                    else if (rewardData.placeInRatingLess <= 0)
                        text = $"1-{rewardData.placeInRatingMore}";
                    else
                        text = $"{rewardData.placeInRatingLess}-{rewardData.placeInRatingMore}";

                    rewardLine.Init(rewardsMin, rewardsMax, text);

                    // if (prevStep < playerPos && i >= playerPos)
                    // {
                    //     playerPlacePosition = rewardLine.Rect;
                    // }


                    rewardLine.SetThisPos(
                        rewardsList[i].placeInRatingLess <= playerPos && playerPos < rewardsList[i].placeInRatingMore && playerPos > 0,
                        playerPos);

                    // prevStep = i;
                }
            }


            // if (playerPlacePosition != null)
            // {
            //     var viewportSize = Vector2.Scale(_scrollRect.viewport.rect.size, _scrollRect.viewport.lossyScale);
            //     var dist = _scrollRect.content.position.y - playerPlacePosition.position.y + viewportSize.y / 2 +
            //                _scrollRect.viewport.position.y;
            //     // LogMarkerPosition(markerName);
            //     _scrollRect.velocity = Vector2.zero;
            //     _scrollRect.content.position = new Vector3(_scrollRect.content.position.x, dist);
            // }
        }

        public void OnClickClose()
        {
            _windowManager.CloseAll(GetWindowId());
        }

        public override void Dispose(PopupBaseCloseType closeType)
        {

        }
    }
}