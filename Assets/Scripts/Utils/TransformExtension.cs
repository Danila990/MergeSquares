using UnityEngine;

namespace Utils
{
    public static class TransformExtension
    {
        public static Vector3 LocalForward(this Transform transform) =>
            transform.localRotation * Vector3.forward;

        public static Vector3 LocalBack(this Transform transform) =>
            transform.localRotation * Vector3.back;

        public static Vector3 LocalUp(this Transform transform) =>
            transform.localRotation * Vector3.up;

        public static Vector3 LocalDown(this Transform transform) =>
            transform.localRotation * Vector3.down;

        public static Vector3 LocalRight(this Transform transform) =>
            transform.localRotation * Vector3.right;

        public static Vector3 LocalLeft(this Transform transform) =>
            transform.localRotation * Vector3.left;
    }
}