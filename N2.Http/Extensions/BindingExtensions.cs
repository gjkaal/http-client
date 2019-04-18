using System.Linq;
using System.Reflection;

namespace N2.Http.Extensions
{
    public static class BindingExtensions
    {
        public static void Bind(this object current, object other)
        {
            if (current == null || other == null) return;
            var otherType = other.GetType();
            foreach (var p in current.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(q => q.CanWrite))
            {
                var pName = p.Name;
                var source = otherType.GetProperty(pName);
                if (source != null)
                {
                    if (source.CanRead)
                    {
                        var value = source.GetValue(other);
                        if (value != null) p.SetValue(current, value);
                    }
                    continue;
                }
                var method = otherType.GetMethod(pName);
                if (method != null)
                {
                    if (!method.IsConstructor && method.GetParameters().Length == 0)
                    {
                        var value = method.Invoke(other, null);
                        if (value != null) p.SetValue(current, value);
                    }
                }
            }
        }
    }

}
