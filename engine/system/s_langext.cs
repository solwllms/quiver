using System;
using System.Collections.Generic;

namespace engine.system
{
    public static class Extensions
    {
        public static T Clamp<T>(this T val, T min, T max) where T : IComparable<T>
        {
            if (val.CompareTo(min) < 0) return min;
            if (val.CompareTo(max) > 0) return max;
            return val;
        }

        public static IEnumerable<string> SplitEvery(this string s, int partLength)
        {
            for (var i = 0; i < s.Length; i += partLength)
                yield return s.Substring(i, Math.Min(partLength, s.Length - i));
        }

        public static string Truncate(this string text, int strLength)
        {
            strLength -= 3;
            var truncatedString = text;

            if (strLength <= 0) return truncatedString;

            if (text == null || text.Length <= strLength) return truncatedString;

            truncatedString = text.Substring(0, strLength);
            truncatedString = truncatedString.TrimEnd();
            return truncatedString + "...";
        }

        public static string GetLast(this string source, int tailLength)
        {
            if (tailLength >= source.Length)
                return source;
            return source.Substring(source.Length - tailLength);
        }

        public static string PadCenter(this string source, int length, char c)
        {
            return source.PadLeft(length / 2 + source.Length / 2, c).PadRight(length - 1, c);
        }

        // https://social.msdn.microsoft.com/Forums/vstudio/en-US/66bcce8d-0d29-4c55-9e35-634d37e25505/how-can-i-find-indices-of-an-element-in-2d-array?forum=csharpgeneral
        public static Tuple<int, int> CoordinatesOf<T>(this T[,] matrix, T value)
        {
            int w = matrix.GetLength(0); // width
            int h = matrix.GetLength(1); // height

            for (int x = 0; x < w; ++x)
            {
                for (int y = 0; y < h; ++y)
                {
                    if (matrix[x, y].Equals(value))
                        return Tuple.Create(x, y);
                }
            }

            return Tuple.Create(-1, -1);
        }
    }
}