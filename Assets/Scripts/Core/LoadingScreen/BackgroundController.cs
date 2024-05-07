using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace Core.LoadingScreen
{
    public class BackgroundController : MonoBehaviour
    {
        [SerializeField] Image vignetteEffect;
        [Range(0, 1f)] [SerializeField] private float vignetteEffectVolume = .5f;
        [SerializeField] private List<GameObject> backgrounds;

        public void Launch()
        {
            vignetteEffect.color = new Color(vignetteEffect.color.r, vignetteEffect.color.g, vignetteEffect.color.b,
                vignetteEffectVolume);

            backgrounds.GetRandom().SetActive(true);
        }
    }
}