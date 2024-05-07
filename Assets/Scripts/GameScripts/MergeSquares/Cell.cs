using System;
using Core.Anchors;
using Core.Audio;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace GameScripts.MergeSquares
{
    public enum CellValue
    {
        Wall = 0,
        Joker = 1,
    }

    public class Cell : MonoBehaviour
    {
        [SerializeField] private SoundSource click;
        [SerializeField] private SoundSource destroyWall;

        public Vector2Int gridPosition;
        public Vector3 localPosition;
        public Vector3 worldPosition;
        public bool TutorialFullLock { get; set; }
        public bool TutorialMoveTapLock { get; set; }
        public UnitView view;
        public Button button;
        public Anchor anchor;
        public RectTransform Rect { get; private set; }
        
        public void PlayClick() => click.Play();
        public void PlayDestroy() => destroyWall.Play();
        
        public bool IsFree => view == null;
        public bool IsWall => !IsFree && view.Value == (int)CellValue.Wall;
        public bool IsJoker => !IsFree && view.Value == (int)CellValue.Joker;

        public Action<Cell> Clicked = cell => {};
        public Action<Cell> FullSet = cell => {};

        private void Start()
        {
            Rect = GetComponent<RectTransform>();
        }

        public void SetFree()
        {
            view = null;
        }

        public void Clear()
        {
            if (view)
            {
                Destroy(view.gameObject);
            }
            SetFree();
        }

        public void SetFull(UnitView view)
        {
            this.view = view;
            var viewTransform = view.transform;
            viewTransform.SetParent(transform);
            viewTransform.localPosition = Vector3.zero;
            viewTransform.localScale = Vector3.one;
            view.ResetRect();
            FullSet.Invoke(this);
        }
        
        public void OnClick()
        {
            Clicked.Invoke(this);
        }
    }
}