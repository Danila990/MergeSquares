using System;
using System.Collections.Generic;
using Core.Anchors;
using UnityEngine;
using Utils;
using Zenject;

namespace Tutorial.View
{
    [Serializable]
    public class TutorialAnchor
    {
        public EAnchorType anchorType;
        public string anchorId;
        [SortingLayer]
        public string sortingLayerName;
        public int sortingOrder;
    }
    public class TutorialClue : MonoBehaviour
    {
        [SerializeField] private Canvas canvas;
        [SerializeField] private EAnchorType anchorType;
        [SerializeField] private string anchorId;
        [SerializeField] private bool autoEndAnim;
        [SortingLayer]
        [SerializeField] private string sortingLayerName;
        [SerializeField] private int sortingOrder;
        [SerializeField] private List<TutorialAnchor> moreAnchors = new();

        public Action animationEnd = () => {};
        public bool Completed => _completed || autoEndAnim;

        private Anchor _anchor;
        private bool _completed;

        private AnchorService _anchorService;

        [Inject]
        public void Construct
        (
            Camera worldCamera,
            AnchorService anchorService
        )
        {
            canvas.worldCamera = worldCamera;
            _anchorService = anchorService;

            if (_anchorService.TryGetAnchor(anchorType, out _anchor, anchorId))
            {
                _anchor.SetSorting(sortingLayerName, sortingOrder);
            }

            foreach (var anchor in moreAnchors)
            {
                SetAnchorSorting(anchor);
            }
        }

        private void OnDestroy()
        {
            if (_anchor != null)
            {
                _anchor.ResetSorting();
            }
            foreach (var anchor in moreAnchors)
            {
                ResetAnchorSorting(anchor);
            }
        }

        public void OnAnimationEnd()
        {
            _completed = true;
            animationEnd.Invoke();
        }

        private void SetAnchorSorting(TutorialAnchor data)
        {
            if (_anchorService.TryGetAnchor(data.anchorType, out var anchor, data.anchorId))
            {
                anchor.SetSorting(data.sortingLayerName, data.sortingOrder);
            }
        }
        
        private void ResetAnchorSorting(TutorialAnchor data)
        {
            if (_anchorService.TryGetAnchor(data.anchorType, out var anchor, data.anchorId))
            {
                anchor.ResetSorting();
            }
        }
    }
}
