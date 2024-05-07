using UnityEngine;

namespace Utils.Animation.AnimationState
{
    public class UpdateTriggerOnExit : StateMachineBehaviour
    {
        public string TriggerName;
        public bool triggered;

        // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (triggered)
            {
                animator.SetTrigger(TriggerName);
            }
            else
            {
                animator.ResetTrigger(TriggerName);
            }
        }
    }
}