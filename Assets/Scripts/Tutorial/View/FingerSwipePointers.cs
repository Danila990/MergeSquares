using UnityEngine;

namespace Tutorial.View
{
    public class FingerSwipePointers : MonoBehaviour
    {
        [SerializeField] private RectTransform upper;
        [SerializeField] private RectTransform lower;
        [SerializeField] private RectTransform left;
        [SerializeField] private RectTransform right;
        
        public RectTransform Upper => upper;
        public RectTransform Lower => lower;
        public RectTransform Left => left;
        public RectTransform Right => right;
    }
}