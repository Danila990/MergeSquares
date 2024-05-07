using UnityEngine;

namespace Utils
{
    public static class VectorExtensions
    {
        public static Vector2 To2D(this Vector3 v)
            => new Vector2(v.x, v.z);

        public static Vector3 To3D(this Vector2 v)
            => new Vector3(v.x, 0, v.y);

        public static float Determinant(Vector2 from, Vector2 to)
            => @from.x * to.y - @from.y * to.x;

        public static bool IsLeft(Vector2 forward, Vector2 direction)
            => forward.x * direction.y - forward.y * direction.x > 0;

        public static Vector2Int ToInt2(this Vector2 v)
            => new Vector2Int((int) v.x, (int) v.y);
    }
}