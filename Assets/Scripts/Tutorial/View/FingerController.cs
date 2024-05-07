using DG.Tweening;
using UnityEngine;

namespace Tutorial.View
{
    public enum EFingerAnimation
    {
        None,
        Pulse,
        LoopTap
    }

    public class FingerController : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private Canvas canvas;
        [SerializeField] private FingerSwipePointers swipePointers;

        private readonly int _hold = Animator.StringToHash("Hold");
        private readonly int _taped = Animator.StringToHash("Taped");
        private readonly int _pulse = Animator.StringToHash("Pulse");
        private readonly int _loopTap = Animator.StringToHash("LoopTap");

        public Canvas Canvas => canvas;
        public FingerSwipePointers SwipePointers => swipePointers;

        private Sequence _sequence;

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void SetScreenPosition(Vector3 position)
        {
            transform.position = position;
        }

        public void StartLoopTapAnimation()
        {
            animator.SetTrigger(_loopTap);
        }

        public void StartPulseAnimation()
        {
            animator.SetTrigger(_pulse);
        }

        public void StartSequenceSwipe(Vector2 start, Vector2 end)
        {
            transform.position = start;
            animator.SetTrigger(_hold);
            _sequence = GetSwipeSequence(end);
        }

        public void StopSequence()
        {
            _sequence.Kill();
        }

        public void StartAnimation(EFingerAnimation animationType)
        {
            switch (animationType)
            {
                case EFingerAnimation.Pulse:
                    StartPulseAnimation();
                    break;
                case EFingerAnimation.LoopTap:
                    StartLoopTapAnimation();
                    break;
            }
        }
        public void Upside()
        {
            transform.localScale = new Vector3(1, -1, 1);
        }
        
        public void Downside()
        {
            transform.localScale = new Vector3(-1, -1, 1);
        }
        
        public void LeftHand()
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }

        public void RightHand()
        {
            transform.localScale = Vector3.one;
        }

        private Sequence GetSwipeSequence(Vector2 end)
        {
            var sequence = DOTween.Sequence();
            sequence.AppendCallback(() => animator.SetBool(_taped, true));
            sequence.AppendInterval(.5f);
            sequence.Append(transform.DOMove(end, 1f));
            sequence.AppendCallback(() => animator.SetBool(_taped, false));
            sequence.AppendInterval(.5f);
            sequence.SetLoops(-1);
            return sequence;
        }
    }
}