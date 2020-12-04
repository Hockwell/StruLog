using System;

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
