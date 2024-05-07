using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

namespace Utils
{
    public class BezierUtils
    {
        // Got from https://catlikecoding.com/unity/tutorials/curves-and-splines/
        public static Vector3 GetPoint(Vector3 p0, Vector3 p1, Vector3 p2, float t)
        {
            t = Mathf.Clamp01(t);
            float oneMinusT = 1f - t;
            return
                oneMinusT * oneMinusT * p0 +
                2f * oneMinusT * t * p1 +
                t * t * p2;
        }

        public static TweenerCore<float, float, FloatOptions> CreateTween(Transform entity, Vector3 from, Vector3 to,
            float duration, float range = 0.5f, float centerOffset = 0.5f)

        {
            var angle = Vector3.SignedAngle(Vector3.right, to - from, Vector3.forward);
            var curvDir = angle > -90 && angle <= 90;
            var point = LineSegment.GetCenterNormalPoint(from, to,
                curvDir ? range : -1 * range, centerOffset);
            var time = 0f;
            return DOTween.To(() => time, newTime =>
            {
                time = newTime;
                entity.transform.position = GetPoint(from, point, to, time);
            }, 1f, 0.5f);
        }
    }
}