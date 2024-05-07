using UnityEngine;
using UnityEngine.UI;

namespace Plugins.DDSystem.Script
{
    [RequireComponent(typeof(Image))]
    public class Character : MonoBehaviour
    {
        public string LocalizationNameKey;
        public Emotion Emotion;
        public AudioClip[] ChatSE;
        public AudioClip[] CallSE;
    }
}