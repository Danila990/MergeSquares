using UnityEngine;

namespace Utils.Animation.AnimationState
{
    public class UpdateBoolOnExit : StateMachineBehaviour
    {
        public string fieldName;
        public bool value;

        // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            animator.SetBool(fieldName, value);
        }
    }
}