using UnityEngine;
using UnityEngine.UI;

namespace Core.LoadingScreen
{
    public class LineBar : MonoBehaviour
    {
        [SerializeField] private Image line;

        private void Awake()
        {
            line.fillAmount = 0;
        }

        public void FillAmount(float value)
        {
            line.fillAmount = value;
        }
    }
}