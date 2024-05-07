using UnityEngine;

namespace Utils.Animation.AnimationState
{
    public class UpdateBoolOnEnter : StateMachineBehaviour
    {
        public string fieldName;
        public bool value;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            animator.SetBool(fieldName, value);
        }
    }
}