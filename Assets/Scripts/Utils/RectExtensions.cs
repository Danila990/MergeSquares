using UnityEngine;

namespace Utils
{
    public static class RectExtensions
    {
//		public static Vector2 GetMaxWithRotate(this Rect rect, Quaternion rotation)
//		{
//			var local = rect.max - rect.center;
//			var localWithRotate = rotation * local.To3D();
//			return rect.center + localWithRotate.To2D();
//		}
//
//		public static Vector2 GetMinWithRotate(this Rect rect, Quaternion rotation)
//		{
//			var local = rect.min - rect.center;
//			var localWithRotate = rotation * local.To3D();
//			return rect.center + localWithRotate.To2D();
//		}

        public static Vector2 GetMaxWithRotate(this Rect rect, float angle)
        {
            return Rotate(rect.max, rect.center, angle);
        }

        public static Vector2 GetMinWithRotate(this Rect rect, float angle)
        {
            return Rotate(rect.min, rect.center, angle);
        }

        private static Vector2 Rotate(Vector2 point, Vector2 center, float angle)
        {
            float s = Mathf.Sin(angle);
            float c = Mathf.Cos(angle);

            // translate point back to origin:
            var newPX = point.x - center.x;
            var newPY = point.y - center.y;

            // rotate point
            float xnew = newPX * c - newPY * s;
            float ynew = newPX * s + newPY * c;

            // translate point back:
            return new Vector2(xnew + center.x, ynew + center.y);
        }
    }
}