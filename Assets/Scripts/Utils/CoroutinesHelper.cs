using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utils
{
    public static class CoroutinesHelper
    {
        public static IEnumerator Graduate
        (
            Action<float> action,
            float duration,
            bool reverse = false,
            AnimationCurve curve = default
        )
        {
            for (float time = 0f; time < duration; time += Time.deltaTime)
            {
                float ratio = time / duration;
                ratio = reverse ? 1f - ratio : ratio;

                float progress = curve?.Evaluate(ratio) ?? ratio;

                action.Invoke(progress);

                yield return null;
            }

            action.Invoke(curve?.Evaluate(reverse ? 0f : 1f) ?? (reverse ? 0f : 1f));
        }

        public static IEnumerator ExecuteConsistently(float cooldown, IEnumerable<IEnumerator> coroutines)
        {
            foreach (var coroutine in coroutines)
            {
                yield return coroutine;
                yield return new WaitForSeconds(cooldown);
            }
        }

        public static IEnumerator ExecuteConsistently(float cooldown = 0f, params IEnumerator[] coroutines)
        {   
            yield return ExecuteConsistently(cooldown, coroutines);
        }

        public static IEnumerator CoroutineWithCallback(IEnumerator coroutine, Action callback)
        {
            yield return coroutine;
            callback.Invoke();
        }

        
        public static IEnumerator Repeat(Action action, int count, float duration)
        {
            for (int i = 0; i < count; i++)
            {
                action.Invoke();
                yield return new WaitForSeconds(duration);
            }
        }
        
        public static IEnumerator ForeachWithStep<T>(IEnumerable<T> collection, Action<T> action, float step, Action callback = default)
        {
            foreach (var element in collection)
            {
                action.Invoke(element);
                yield return new WaitForSeconds(step);
            }

            callback?.Invoke();
        }

        public static IEnumerator Jump(Vector3 from, Vector3 to, Action<Vector3> action, float duration, float height, AnimationCurve curve)
        {
            yield return Graduate(SetProgress, duration, false, curve);

            void SetProgress(float progress)
            {
                var position = Vector2.Lerp(from, to, progress);

                var currentHeight = Mathf.Lerp(from.y, to.y, progress);
                var currentJumpHeight = Mathf.Sin(progress * Mathf.PI) * height;

                position.y = currentHeight + currentJumpHeight;

                action.Invoke(position);
            }
        }
    }
}
