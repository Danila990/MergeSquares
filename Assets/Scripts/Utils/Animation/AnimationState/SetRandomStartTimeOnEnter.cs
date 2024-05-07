using UnityEngine;

namespace Utils.Animation.AnimationState
{
    public class SetRandomStartTimeOnEnter : StateMachineBehaviour
    {
        private bool randomizeApplied = false;

        private void OnDisable()
        {
            randomizeApplied = false;
        }

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (!randomizeApplied)
            {
                randomizeApplied = true;
                animator.Play(stateInfo.fullPathHash, layerIndex, Random.value);
            }
        }
    }
}