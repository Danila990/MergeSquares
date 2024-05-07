using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class Bar : MonoBehaviour
    {
        [SerializeField] private Image image;

        public float Progress
        {
            get;
            private set;
        } = 0f;
        
        public void SetProgress(float progress)
        {
            Progress = progress;
            image.fillAmount = Progress;
        }
    }
}