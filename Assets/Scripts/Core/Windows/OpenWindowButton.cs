using Core.Audio;
using UnityEngine;
using Utils;
using Zenject;

namespace Core.Windows
{
    public class OpenWindowButton : MonoBehaviour
    {
        [SerializeField] private EPopupType windowId;
        [SerializeField] private SoundSource clickSound;
        [SerializeField] private float delayTime = 0f;

        private float _timer;
        private bool _timerStarted;
        
        private WindowManager _windowManager;

        [Inject]
        public void Construct(WindowManager windowManager)
        {
            _windowManager = windowManager;
        }

        private void Update()
        {
            if (_timerStarted)
            {
                _timer -= Time.deltaTime;
                if (_timer <= 0f)
                {
                    _timerStarted = false;
                    OnClick();
                }
            }
        }

        public void OnClick()
        {
            if(clickSound != null)
            {
                clickSound.Play();
            }
            else
            {
                Debug.Log($"[OpenWindowButton][OnClick] doesn't have click sound at {gameObject.GetPath()}");
            }
            _windowManager.ShowWindow(windowId.ToString());
        }

        public void OnPress()
        {
            _timerStarted = true;
            _timer = delayTime;
        }

        public void OnRelease()
        {
            _timerStarted = false;
        }
    }
}