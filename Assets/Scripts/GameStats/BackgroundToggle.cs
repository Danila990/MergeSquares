using UnityEngine;

namespace GameStats
{
    public class BackgroundToggle : MonoBehaviour
    {
        [SerializeField] private GameObject backgroundWithTimer;
        [SerializeField] private GameObject shortenedBackground;
        [SerializeField] private GameStatReloaderView gameStatReloaderView;
        
        private void Start()
        {
            gameStatReloaderView.ValueChanged += OnValueChanged;
            OnValueChanged(!gameStatReloaderView.InReload);
        }

        private void OnValueChanged(bool maxValue)
        {
            if (!maxValue)
            {
                backgroundWithTimer.SetActive(true);
                shortenedBackground.SetActive(false);
            }
            else
            {
                backgroundWithTimer.SetActive(false);
                shortenedBackground.SetActive(true);
            }
        }
    }
}

