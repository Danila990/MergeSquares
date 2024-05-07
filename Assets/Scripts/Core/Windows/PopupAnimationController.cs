using System;
using UnityEngine;

namespace Core.Windows
{
    public class PopupAnimationController : StateMachineBehaviour
    {
        public event Action HideFinished;

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (stateInfo.IsName("Hide"))
            {
                HideFinished?.Invoke();
            }
        }
    }
}