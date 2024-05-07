using System.Text;

namespace Utils
{
    public static class ObjectInfo
    {
        public static string Report(object obj, params object[] prm)
        {
            if (prm.Length == 0)
            {
                return obj.GetType().Name + "()";
            }

            var builder = new StringBuilder();
            builder.Append(obj.GetType().Name).Append("(");
            var tail = false;
            foreach (var p in prm)
            {
                if (tail)
                {
                    builder.Append(",");
                }

                builder.Append(p);
                tail = true;
            }

            builder.Append(")");
            return builder.ToString();
        }
    }
}
