using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using CloudServices;
using TMPro;
using UnityEngine;
using Zenject;
using Screen = UnityEngine.Device.Screen;

namespace Core.Camera
{
    [Serializable]
    public class ShiftCameraConfig
    {
        public int Shift;
        public EPlatformType Type;
    }
    
    [RequireComponent(typeof(UnityEngine.Camera))]
    [DisallowMultipleComponent]
    public class ShiftCamera : MonoBehaviour
    {
        [SerializeField] private List<ShiftCameraConfig> shifts = new();
        [SerializeField] private int shiftDefault = 50;
        [SerializeField] private float percent = 0.1f;
        [SerializeField] private TextMeshProUGUI dpiText;
        [SerializeField] private TextMeshProUGUI screenSizeText;
        [SerializeField] private TextMeshProUGUI displaySizeText;
        [SerializeField] private TextMeshProUGUI canvasSizeText;

        private int _screenSizeX = 0;
        private int _screenSizeY = 0;
        private UnityEngine.Camera _camera;
        private bool _needShift = false;

        private CloudService _cloudService;

        [Inject]
        public void Construct(CloudService cloudService)
        {
            _cloudService = cloudService;
        }

        private void Start()
        {
            _needShift = _cloudService.CloudProvider.IsStartStickyShifted;
            _camera = GetComponent<UnityEngine.Camera>();
            Rescale();
        }

        private void Update()
        {
            Rescale();
        }

        private void Rescale()
        {
            if (!_needShift || Screen.width == _screenSizeX && Screen.height == _screenSizeY) return;

            var height = GPGetCanvasHeight();
            var scaleHeight = (height - GetShift()) / height;
            var rect = _camera.rect;
            rect.yMax = scaleHeight;
            _camera.rect = rect;

            _screenSizeX = Screen.width;
            _screenSizeY = Screen.height;
            if(dpiText != null)
            {
                dpiText.text = Screen.dpi.ToString();
            }
            if(screenSizeText != null)
            {
                screenSizeText.text = $"screenSizeText: {_screenSizeX}x{_screenSizeY}";
            }
            if(displaySizeText != null)
            {
                displaySizeText.text = $"displaySizeText: {Display.main.renderingWidth}x{Display.main.renderingHeight}";
            }
            if(canvasSizeText != null)
            {
                canvasSizeText.text =
                    $"canvasSizeText: {GPGetCanvasWidth().ToString()}x{GPGetCanvasHeight().ToString()}";
            }
        }
        
        [DllImport("__Internal")]
        private static extern float GPGetCanvasWidthExtern();
        
        [DllImport("__Internal")]
        private static extern float GPGetCanvasHeightExtern();
        
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        public float GPGetCanvasWidth() => Screen.width;
        public float GPGetCanvasHeight() => Screen.height;
#else
        public float GPGetCanvasWidth() => GPGetCanvasWidthExtern();
        public float GPGetCanvasHeight() => GPGetCanvasHeightExtern();
#endif

        private int GetShift()
        {
            foreach (var config in shifts)
            {
                if (config.Type == _cloudService.CloudProvider.GetPlatformType())
                {
                    return config.Shift;
                }
            }

            return shiftDefault;
        }
    }
}