using System;
using Core.Anchors;
using Core.Audio;
using GameScripts.MergeSquares;
using UnityEngine;
using UnityEngine.UI;

namespace GameScripts.Game2248
{
    public class Cell : MonoBehaviour
    {
        [SerializeField] private SoundSource click;
        // [SerializeField] private SoundSource destroyWall;

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
        // public void PlayDestroy() => destroyWall.Play();
        
        public bool IsFree => view == null;
        public bool IsWall => !IsFree && view.Value == (int)CellValue.Wall;
        public bool IsJoker => !IsFree && view.Value == (int)CellValue.Joker;

        public Action<Cell> PointerDowned = cell => {};
        public Action<Cell> PointerUp = cell => {};
        public Action<Cell> PointerEnter = cell => {};
        
        public Action<Cell> SettedFull = cell => {};

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
            SettedFull.Invoke(this);
        }
        
        public void OnPointerDown()
        {
            PointerDowned.Invoke(this);
        }
        
        public void OnPointerUp()
        {
            PointerUp.Invoke(this);
        }
        
        public void OnPointerEnter()
        {
            PointerEnter.Invoke(this);
        }
    }
}