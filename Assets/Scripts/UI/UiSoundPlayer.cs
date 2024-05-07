using Core.Audio;
using UnityEngine;
using Utils;

namespace UI
{
    public class UiSoundPlayer : MonoBehaviour
    {
        [SerializeField] private SoundSource clickSound;

        public void OnClick()
        {
            if(clickSound == null)
            {
                Debug.Log($"[UiSoundPlayer][OnClick] doesn't have click sound at {gameObject.GetPath()}");
                return;
            }
            else
            {
                clickSound.Play();
            }
        }
    }
}