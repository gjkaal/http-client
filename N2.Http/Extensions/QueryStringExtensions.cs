using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Web;

namespace N2.Http.Extensions
{
    public static class QueryStringExtensions
    {
        public static string AsQueryString(this Dictionary<string, object> queryParameters)
        {
            var sb = new StringBuilder();
            var e = queryParameters.GetEnumerator();
            var first = true;
            while (e.MoveNext())
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    sb.Append('&');
                }
                var kv = e.Current;
                var kvKey = HttpUtility.UrlEncode(kv.Key.InitialLowerCase());
                sb.Append(AsQueryString(kv.Value, kvKey));
            }
            return sb.ToString();
        }

        public static string AsQueryString(this object value, string prefix)
        {
            var sb = new StringBuilder();
            var type = value.GetType();
            if (type.IsPrimitive
                || type.Equals(typeof(string))
                || type.Equals(typeof(DateTime))
                || value is Array
                )
            {
                sb.Append(prefix);
                sb.Append('=');
                sb.Append(value.AsQueryString());
            }
            else
            {
                var t = value.GetType();
                var first = true;
                foreach (var p in t.GetProperties())
                {
                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        sb.Append('&');
                    }
                    var pName = HttpUtility.UrlEncode(p.Name.InitialLowerCase());
                    var pValue = p.GetValue(value);
                    sb.Append(AsQueryString(pValue, $"{prefix}.{pName}"));
                }
            }
            return sb.ToString();
        }

        public static string AsQueryString(this object value)
        {
            if (value is bool || value is int || value is char || value is short || value is long)
            {
                return value.ToString();
            }
            if (value is float)
            {
                return ((float)value).ToString("G9", CultureInfo.InvariantCulture);
            }
            if (value is double)
            {
                return ((double)value).ToString("G17", CultureInfo.InvariantCulture);
            }
            if (value is DateTime)
            {
                return ((DateTime)value).ToString("O", CultureInfo.InvariantCulture);
            }
            if (value is string)
            {
                return HttpUtility.UrlEncode((string)value);
            }

            var sb = new StringBuilder();
            if (value is Array)
            {
                var firstItem = true;
                foreach (var current in (Array)value)
                {
                    if (firstItem)
                    {
                        firstItem = false;
                    }
                    else
                    {
                        sb.Append(',');
                    }
                    sb.Append(AsQueryString(current));
                }
                return sb.ToString();
            }

            var t = value.GetType();
            var first = true;
            foreach (var p in t.GetProperties())
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    sb.Append('&');
                }
                var pName = HttpUtility.UrlEncode(p.Name.InitialLowerCase());
                var pValue = p.GetValue(value);
                sb.Append(AsQueryString(pValue, pName));
            }
            return sb.ToString();
        }

        public static string InitialLowerCase(this string source)
        {
            if (string.IsNullOrWhiteSpace(source)) return source;
            var charArray = source.ToCharArray();
            charArray[0] = char.ToLower(charArray[0]);
            return new string(charArray);
        }

    }

}
