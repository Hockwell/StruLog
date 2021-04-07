using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace StruLog
{
    //internal class CaseInsensitiveStringsComparer : IEqualityComparer<string>
    //{
    //    public bool Equals([AllowNull] string x, [AllowNull] string y)
    //    {
    //        return String.Compare(x, y, true) == 0;
    //    }

    //    public int GetHashCode([DisallowNull] string obj)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}

    internal static class NewtonsoftJsonTools
    {
        internal static JToken ConvertIEnumerableToJToken<T>(IEnumerable<T> sequence)
        {
            if (sequence is null)
                throw new ArgumentNullException(nameof(sequence));
            JArray jsonSection = new JArray();
            foreach (var item in sequence)
            {
                jsonSection.Add(item);
            }
            return jsonSection.Count == 0 ? null : jsonSection;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T">Type of IEnumerable elements</typeparam>
        /// <param name="jsonSection"></param>
        /// <returns>null if jsonSection is empty</returns>
        internal static IEnumerable<T> ConvertJTokenToIEnumerable<T>(JToken jsonSection)
        {
            if (jsonSection is null)
                throw new ArgumentNullException(nameof(jsonSection));
            List<T> sequence = new List<T>();
            foreach (var item in jsonSection)
            {
                var val = item.Value<T>();
                sequence.Add(val);
            }
            return sequence.Count == 0 ? null : sequence;
        }
    }
    internal static class StringTools
    {
        internal static bool CompareStrings(string a, string b, bool ignoreCase = true)
        {
            return string.Compare(a, b, ignoreCase) == 0;
        }

        internal static T StringToEnum<T>(this string enumVal) where T : struct, IConvertible
        {
            if (!typeof(T).IsEnum)
            {
                throw new ArgumentException("T must be an enumerated type");
            }
            var o = (T)Enum.Parse(typeof(T), enumVal, true);
            return o;
        }
        internal static string EnumToString<T>(this T enumVal) where T : struct, IConvertible
        {
            if (!typeof(T).IsEnum)
            {
                throw new ArgumentException("T must be an enumerated type");
            }
            return Enum.GetName(typeof(T), enumVal);
        }
    }


}
