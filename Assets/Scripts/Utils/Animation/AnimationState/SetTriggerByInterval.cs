using UnityEngine;

namespace Utils.Animation.AnimationState
{
    public class SetTriggerByInterval : StateMachineBehaviour
    {
        [Range(0, 20)] public int minDelay;
        [Range(0, 20)] public int maxDelay;

        public string triggerName;

        private float enterTime;
        private float delayTime;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
        {
            enterTime = Time.realtimeSinceStartup;
            delayTime = Random.Range(minDelay, maxDelay);
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (Time.realtimeSinceStartup - enterTime > delayTime)
            {
                animator.SetTrigger(triggerName);
            }
        }
    }
}