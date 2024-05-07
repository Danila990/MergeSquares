using UnityEngine;

namespace Utils.Animation.AnimationState
{
    public class UpdateBoolOnUpdate : StateMachineBehaviour
    {
        public string fieldName;
        public bool value;
        public int layerIndex;
        [Range(0.0f, 1.0f)] public float normalizedTime;
        private bool reported;

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (this.layerIndex == layerIndex && stateInfo.normalizedTime >= normalizedTime &&
                !animator.IsInTransition(layerIndex) && !reported)
            {
                reported = true;
                animator.SetBool(fieldName, value);
            }
        }

        // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            reported = false;
        }
    }
}