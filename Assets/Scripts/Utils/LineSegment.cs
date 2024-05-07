using UnityEngine;

namespace Utils
{
    public class Line
    {
        public float a, b, c;
    }

    public static class LineSegment
    {
        public static bool AreCrossing(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, out Vector2 crossPoint)
        {
            var v1 = VectorProd(p4.x - p3.x, p4.y - p3.y, p1.x - p3.x, p1.y - p3.y);
            var v2 = VectorProd(p4.x - p3.x, p4.y - p3.y, p2.x - p3.x, p2.y - p3.y);
            var v3 = VectorProd(p2.x - p1.x, p2.y - p1.y, p3.x - p1.x, p3.y - p1.y);
            var v4 = VectorProd(p2.x - p1.x, p2.y - p1.y, p4.x - p1.x, p4.y - p1.y);
            crossPoint = Vector2.zero;
            if (v1 * v2 < 0 && v3 * v4 < 0)
            {
                var line1 = LineEquation(p1, p2);
                var line2 = LineEquation(p3, p4);
                crossPoint = CrossingPoint(line1, line2);
                return true;
            }

            return false;
        }

        //
        // Найти координаты перпендикуляра к середине прямой на заданном расстоянии
        //
        public static Vector2 GetCenterNormalPoint(Vector2 p1, Vector2 p2, float range, float centerOffset = 0)
        {
            var line = p2 - p1;
            var centerPoint = p1 + (0.5f + centerOffset) * line;
            // Cтроим вектор длиной R, сонаправленный с p1 p2
            var p = range / line.magnitude;
            var p3Tmp = p * line;
            // Поворачиваем вектор на Пи/2 против часовой стрелки
            p3Tmp = new Vector2(-p3Tmp.y, p3Tmp.x);
            // Сдвигаем вектор в точку centerPoint
            return p3Tmp + centerPoint;
        }

        private static float VectorProd(float ax, float ay, float bx, float by)
        {
            return ax * by - bx * ay;
        }

        private static Line LineEquation(Vector2 p1, Vector2 p2)
        {
            return new Line()
            {
                a = p2.y - p1.y,
                b = p1.x - p2.x,
                c = -p1.x * (p2.y - p1.y) + p1.y * (p2.x - p1.x)
            };
        }

        private static Vector2 CrossingPoint(Line line1, Line line2)
        {
            Vector2 pt = new Vector2();
            var d = line1.a * line2.b - line1.b * line2.a;
            var dx = -line1.c * line2.b + line1.b * line2.c;
            var dy = -line1.a * line2.c + line1.c * line2.a;
            pt.x = dx / d;
            pt.y = dy / d;
            return pt;
        }
    }
}