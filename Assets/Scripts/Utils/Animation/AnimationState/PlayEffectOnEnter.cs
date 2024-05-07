using JetBrains.Annotations;
using UnityEngine;

namespace Utils.Animation.AnimationState
{
    public class PlayEffectOnEnter : StateMachineBehaviour
    {
        [NotNull] public ParticleSystem prefab;
        public float customDuration = -1;

        private ParticleSystem instance;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            instance = Instantiate(prefab, animator.transform);
            instance.Play();
            if (!instance.main.loop)
            {
                Destroy(instance.gameObject, customDuration > 0.0f ? customDuration : instance.main.duration);
                instance = null;
            }
        }

        private void OnDisable()
        {
            DestroyEffect();
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
        {
            DestroyEffect();
        }

        private void DestroyEffect()
        {
            if (instance != null && instance.main.loop)
            {
                Destroy(instance.gameObject);
                instance = null;
            }
        }
    }
}