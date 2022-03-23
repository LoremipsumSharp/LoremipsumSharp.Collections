using System;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LoremipsumSharp.Common
{
    public static class StringExtension
    {
        public static string AppendIf(this string s, bool predicate, string str)
        {
            if (predicate)
            {
                return s + str;
            }
            return s;
        }

        public static string AppendIf(this string s, bool predicate, Func<string> str)
        {
            if (predicate)
            {
                return s + str();
            }
            return s;
        }
        public static StringBuilder AppendIf(this StringBuilder sb, bool condition, string value)
        {
            if (condition)
            {
                return sb.Append(value);
            }
            return sb;
        }

        public static bool IsNullOrWhiteSpace(this string str)
        {
            return string.IsNullOrWhiteSpace(str);
        }

        public static string SurroundWith(this string text, string ends)
        {
            return ends + text + ends;
        }

        public static string SurroundWithSingleQuote(this string text)
        {
            return SurroundWith(text, "'");
        }

        public static bool TryFormat(this string target, out string result, params object[] args)
        {
            try
            {
                result = string.Format(target, args);
                return true;
            }
            catch
            {
                result = target;
                return false;
            }
        }


        public static bool IsJson(this string value)
        {
            bool isJson;
            try
            {
                JToken.Parse(value);
                isJson = true;
            }
            catch (JsonReaderException)
            {
                isJson = false;
            }
            return isJson;

        }

    }
}