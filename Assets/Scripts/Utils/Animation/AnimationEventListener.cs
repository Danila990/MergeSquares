using UnityEngine;
using UnityEngine.Events;

namespace Utils.Animation
{
    public class AnimationEventListener : MonoBehaviour
    {
        [SerializeField] private UnityEvent animationEvent; 
        
        public void Fire()
        {
            animationEvent.Invoke();
        }
    }
}