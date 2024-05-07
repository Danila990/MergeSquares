using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class ButtonLocker : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private float lockTime;

        private float _timer = 0;
        
        private void Update()
        {
            if(_timer < lockTime)
            {
                _timer += Time.deltaTime;
                button.interactable = _timer >= lockTime;
            }
        }
    }
}