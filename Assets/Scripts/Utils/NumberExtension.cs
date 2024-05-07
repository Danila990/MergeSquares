using UnityEngine;

namespace Utils
{
    public static class NumberExtension
    {
        public static bool IsZero(this float value)
        {
            return Mathf.Abs(value) <= 0.001f;
        }
    }
}