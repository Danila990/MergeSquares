using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class ScrollViewAnim : MonoBehaviour
    {
        [SerializeField] private ScrollRect rect;
        [Range(0, 1)] [SerializeField] private float startPos = 0f;
        [Range(0, 1)] [SerializeField] private float endPos = 1f;
        [SerializeField] private float animTime = 2f;
        
        private void Start()
        {
            rect.verticalNormalizedPosition = startPos;
            rect.DOVerticalNormalizedPos(endPos, animTime);
        }
    }
}