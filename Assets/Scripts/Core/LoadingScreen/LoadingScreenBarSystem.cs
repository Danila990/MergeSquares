using System;
using System.Text;
using System.Threading.Tasks;
using CloudServices;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using TMPro;
using Zenject;

namespace Core.LoadingScreen
{
    public class LoadingScreenBarSystem : MonoBehaviour
    {
        [SerializeField] private int gameSceneIndex = 1;
        [SerializeField] private RotateBar rotateBar;
        [SerializeField] private BackgroundController backgroundController;
        [SerializeField] private TextMeshProUGUI loadStep;
        [SerializeField] private TextMeshProUGUI version;

        private float _loadProgress = 0;
        private Sequence _sequence;
        private bool _loaded;
        
        private CloudService _cloudService;

        [Inject]
        public void Construct(CloudService cloudService)
        {
            _cloudService = cloudService;
        }

        private async void Start()
        {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            version.gameObject.SetActive(true);
            version.text = $"Version {Application.version}";
#else
            version.gameObject.SetActive(false);        
#endif
            backgroundController.Launch();

            _sequence = DOTween.Sequence();
            _sequence.Append(DOTween.To(() => _loadProgress, x => _loadProgress = x, 0.5f, 1f).
                OnUpdate(() => { rotateBar.UpdateProgress(_loadProgress); }
                ));
            await Load();
        }

        private async Task Load()
        {
            var builder = new StringBuilder();
            UpdateLoadStep(builder, $"Check scene index...");
            if (SceneManager.GetActiveScene().buildIndex == gameSceneIndex)
            {
                return;
            }
            if(_cloudService.NeedWatch)
            {
                Debug.Log($"[LoadingScreen][Load] Start loading...");
            }
            UpdateLoadStep(builder, $"Init sdk...");
            while (!_cloudService.CloudProvider.IsSdkInit())
            {
                await Task.Yield();
            }
            UpdateLoadStep(builder, $"Wait preloader over...");
            while (_cloudService.CloudProvider.IsPreloaderPlaying)
            {
                await Task.Yield();
            }
            UpdateLoadStep(builder, $"Start load...");
            _cloudService.CloudProvider.Loaded += OnLoaded;
            _cloudService.CloudProvider.LoadOnInit();
            while (!_loaded)
            {
                await Task.Yield();
            }

            _loaded = false;
            UpdateLoadStep(builder, $"Start get purchases...");
            _cloudService.CloudProvider.ShowStickyFromStart();
            _cloudService.CloudProvider.PurchasesGot += OnPurchasesGot;
            _cloudService.CloudProvider.StartGetPurchases();
            while (!_loaded)
            {
                await Task.Yield();
            }
            UpdateLoadStep(builder, $"Start scene load...");
            var waitNextScene = SceneManager.LoadSceneAsync(gameSceneIndex);
            waitNextScene.completed += op =>
            {
                _sequence.Kill();
                _cloudService.CloudProvider.GameReady();
            };
            while (!waitNextScene.isDone)
            {
                UpdateLoadStep(builder, $"Start scene load...{waitNextScene.progress}");
                var sceneLoadProgress = _loadProgress + waitNextScene.progress / 2;
                rotateBar.UpdateProgress(sceneLoadProgress);
                await Task.Yield();
            }

            UpdateLoadStep(builder, $"Scene load finished...");
        }

        private void OnLoaded(string json, string localJson)
        {
            _cloudService.CloudProvider.Loaded -= OnLoaded;
            _loaded = true;
        }
        
        private void OnPurchasesGot()
        {
            _cloudService.CloudProvider.PurchasesGot -= OnPurchasesGot;
            _loaded = true;
        }

        private void UpdateLoadStep(StringBuilder builder, string data)
        {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            if (loadStep != null)
            {
                builder.Append($"{data}\n");
                loadStep.text = builder.ToString();
            }
#else
            if (loadStep != null)
            {
                loadStep.text = String.Empty;
            }
#endif
        }
    }
}