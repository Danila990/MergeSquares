using System.Collections;
using System.Collections.Generic;
using GameScripts.MergeSquares.InfinityLevel;
using UnityEngine;
using Utils;
using Zenject;

namespace Core.Windows
{
    public class OpenGenericPopup : MonoBehaviour
    {
        [SerializeField] private GenericPopupContent genericPopupPrefab;
        [SortingLayer] [SerializeField] private string layer = "Default";
        [SerializeField] private int sortingOrder = 300;
        
        private WindowManager _windowManager;

        [Inject]
        public void Construct(WindowManager windowManager)
        {
            _windowManager = windowManager;
        }

        public void OpenPopupClick()
        {
            var popupParams = new GenericPopupParams
            {
                prefabToCreate = genericPopupPrefab,
            };
            var window = _windowManager.ShowWindow(EPopupType.GenericPopup.ToString(), new[] { popupParams });
            window.Canvas.sortingLayerName = layer;
            window.Canvas.sortingOrder = sortingOrder;
        }
    }
}
