using System;
using System.Text;
using System.Linq;
using System.Globalization;
using System.IO.Compression;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace System
{
    public static class StringExtensions
    {
        #region [public] IsEmpty(this string input)
        public static bool IsEmpty(this string input)
        {
            return string.IsNullOrEmpty(input);
        }
        #endregion

        #region [public] QuoteSafe(this string input)
        public static string QuoteSafe(this string input)
        {
            return "\"" + input.Trim('\"').Replace("\"", "\\\"") + "\"";
        }
        #endregion

        #region [public] RemoveWhitespaces(this string input) 
        public static unsafe string RemoveWhitespaces(this string input)
        {
            char* output = stackalloc char[input.Length];
            char* current = output;

            for (int i = 0; i < input.Length; ++i)
            {
                if (input[i] == ' ' || input[i] < '\x0013' || input[i] == '\x0085') continue;
                *current++ = input[i];
            }

            return new string(output, 0, (int)(current - output));
        }
        #endregion

        #region [public] Remove(this string input, params char[] chars)
        public static unsafe string Remove(this string input, params char[] chars)
        {
            char* output = stackalloc char[input.Length];
            char* currentChar = output;

            string exclude = new string(chars);
            if (exclude.Length < 1) exclude = " ";

            for (int i = 0; i < input.Length; i++)
            {
                if (exclude.IndexOf(input[i]) == -1)
                {
                    *currentChar++ = input[i];
                }
            }
            return new string(output, 0, (int)(currentChar - output));
        }
        #endregion

        #region [public] Replace(this string input, params char[] chars)
        public static unsafe string Replace(this string input, params object[] objects)
        {
            if (objects.Length > 1)
            {
                for (int i = 0; i < objects.Length; i += 2)
                {
                    input = input.Replace(objects[i].ToString(), objects[i+1].ToString());
                }                
            }
            return input;
        }
        #endregion

        #region [public] Contains(this string str, string value, bool ignoreCase = false)
        public static bool Contains(this string str, string value, bool ignoreCase = false)
        {
            CompareOptions options = ignoreCase ? CompareOptions.IgnoreCase : CompareOptions.None;
            return (CultureInfo.InvariantCulture.CompareInfo.IndexOf(str, value, options) >= 0);
        }
        #endregion

        #region [public] SplitArg(this string str, int index, params char[] separators)
        public static string SplitArg(this string str, int index, params char[] separator)
        {
            if (!string.IsNullOrEmpty(str))
            {
                if (separator.Length == 0)
                {
                    separator = new char[] { ' ' };
                }

                string[] args = str.Split(separator);
                if (index < args.Length) return args[index];
            }
            return str;
        }
        #endregion

        #region [public] SplitKeyValuePair(this string str, params char[] separators)
        public static KeyValuePair<string, string> SplitKeyValuePair(this string str, params char[] separator)
        {            
            if (!string.IsNullOrEmpty(str))
            {
                if (separator.Length == 0)
                {
                    separator = new char[] { ' ' };
                }

                string[] arguments = str.Split(separator);

                if (arguments.Length > 1)
                {
                    return new KeyValuePair<string, string>(arguments[0], arguments[1]);
                }
                return new KeyValuePair<string, string>(arguments[0], null);                
            }
            return new KeyValuePair<string, string>();
        }
        #endregion

        #region [public] Capitalize(this string input, CapitalizeCase capitalize = CapitalizeCase.First)
        public enum CapitalizeCase { First, All }

        public static string Capitalize(this string input, CapitalizeCase capitalize = CapitalizeCase.First)
        {
            if (string.IsNullOrEmpty(input)) return input; input = input.ToLower();
            switch (capitalize)
            {
                case CapitalizeCase.All: return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(input);
            }
            return input.Substring(0, 1).ToUpper(CultureInfo.CurrentCulture) + input.Substring(1, input.Length - 1);
        }
        #endregion

        #region [public] ToString(this string[] values, string separator)
        public static string ToString(this string[] values, string separator)
        {
            if (values != null && values.Length > 0)
            {
                return string.Join(separator, values);
            }
            return string.Empty;
        }
        #endregion

        #region [public] ToString<T>(this IEnumerable<T> values, string separator)
        public static string ToString<T>(this IEnumerable<T> values, string separator)
        {
            try
            {
                if (values != null && values.Count() > 0)
                {
                    return string.Join(separator, Array.ConvertAll(values.ToArray(), x => x.ToString()));
                }
            }
            catch
            {
                /* Ignore all exceptions */
            }
            return string.Empty;
        }
        #endregion

        #region [public] SplitQuotes(this string input, string separators = ' ')
        public static string[] SplitQuotes(this string input, string separators = " ")
        {
            input = input.Replace("\\\"", "&qute;");
            MatchCollection matchs = new Regex("\"([^\"]+)\"|'([^']+)'|([^"+separators+"]+)").Matches(input);
            string[] result = new string[matchs.Count];
            for (int i = 0; i < matchs.Count; i++)
            {
                result[i] = matchs[i].Groups[0].Value.Trim('"');
                result[i] = result[i].Replace("&qute;", "\"");
            }
            return result;
        }
        #endregion        

        #region [public] Get(this string[] values, int index, string def = "")
        public static string Get(this string[] values, int index, string def = "")
        {
            if (index < values.Length)
            {
                return values[index];
            }
            return def;
        }
        #endregion     

        #region [public] Get<T>(this string[] values, int index)
        public static T Get<T>(this string[] values, int index, T def = default(T))
        {
            if (index < values.Length)
            {
                try
                {
                    return (T)Convert.ChangeType(values[index], typeof(T));
                }
                catch
                {
                    /* Ignore all exceptions */
                }
            }
            return def;
        }
        #endregion        

        #region [public] ConvertTo<T>(this string[] input)
        public static T[] ConvertTo<T>(this string[] input)
        {
            if (input != null && input.Length > 0)
            {
                List<T> result = new List<T>();
                for (int i = 0; i < input.Length; i++)
                {
                    result.Add((T)Convert.ChangeType(input[i], typeof(T)));
                }
                return result.ToArray();
            }
            return default(T[]);
        }
        #endregion        
    }
    
    public static class ByteArrayExtensions
    {
        #region [public] ToInt16(this byte[] bytes, int offset = 0)
        public static short ToInt16(this byte[] bytes, int offset = 0)
        {
            if (offset + 2 > bytes.Length) return 0;
            return (short)(bytes[offset++] | bytes[offset++] << 8);
        }
        #endregion

        #region [public] ToInt32(this byte[] bytes, int offset = 0)
        public static int ToInt32(this byte[] bytes, int offset = 0)
        {
            if (offset + 4 > bytes.Length) return 0;
            return (bytes[offset++] | bytes[offset++] << 8 | bytes[offset++] << 16 | bytes[offset] << 24);
        }
        #endregion

        #region [public] ToInt64(this byte[] bytes, int offset = 0)
        public static long ToInt64(this byte[] bytes, int offset = 0)
        {
            if (offset + 8 > bytes.Length) return 0;
            int i1 = (bytes[offset++] | bytes[offset++] << 8 | bytes[offset++] << 16 | bytes[offset++] << 24);
            int i2 = (bytes[offset++] | bytes[offset++] << 8 | bytes[offset++] << 16 | bytes[offset] << 24);
            return (int)i1 + ((long)i2 << 32);
        }
        #endregion

        #region [public] ToUInt16(this byte[] bytes, int offset = 0)
        public static ushort ToUInt16(this byte[] bytes, int offset = 0)
        {
            return (ushort)bytes.ToInt16(offset);
        }
        #endregion

        #region [public] ToUInt32(this byte[] bytes, int offset = 0)
        public static uint ToUInt32(this byte[] bytes, int offset = 0)
        {
            return (uint)bytes.ToInt32(offset);
        }
        #endregion

        #region [public] ToUInt64(this byte[] bytes, int offset = 0)
        public static ulong ToUInt64(this byte[] bytes, int offset = 0)
        {
            return (ulong)bytes.ToInt64(offset);
        }
        #endregion

        #region [public] IndexOf(this byte[] bytes, byte value, int startIndex = 0)
        public static int IndexOf(this byte[] bytes, byte value, int startIndex = 0)
        {
            while (startIndex < bytes.Length) if (bytes[startIndex++] == value) return startIndex - 1;
            return -1;
        }
        #endregion

        #region [public] GetString(this byte[] bytes, int offset)
        public static string GetString(this byte[] bytes, int offset)
        {
            string result = null;
            if (bytes != null && bytes.Length > 0 && offset < bytes.Length)
            {
                int index = bytes.IndexOf(0, offset);
                if (index >= offset)
                {
                    result = Encoding.ASCII.GetString(bytes, offset, index - offset);
                }
            }
            return result;
        }
        #endregion

        #region [public] ToStruct<T>(this byte[] bytes)
        public static T ToStruct<T>(this byte[] bytes) where T : struct
        {
            unsafe
            {
                fixed (byte* p = &bytes[0])
                {
                    return (T)Marshal.PtrToStructure(new IntPtr(p), typeof(T));
                }
            }
        }
        #endregion

        #region [public] ToStruct<T>(this byte[] bytes, int from)
        public static T ToStruct<T>(this byte[] bytes, int from) where T : struct
        {
            unsafe
            {
                fixed (byte* p = &bytes[from])
                {
                    return (T)Marshal.PtrToStructure(new IntPtr(p), typeof(T));
                }
            }
        }
        #endregion

        #region [public] ToStruct<T>(this byte[] bytes, uint from)
        public static T ToStruct<T>(this byte[] bytes, uint from) where T : struct
        {
            unsafe
            {
                fixed (byte* p = &bytes[from])
                {
                    return (T)Marshal.PtrToStructure(new IntPtr(p), typeof(T));
                }
            }
        }
        #endregion

        #region [public] ToHexString(this byte[] bytes)
        public static string ToHexString(this byte[] bytes)
        {
            char[] result = new char[bytes.Length * 2]; byte b;
            for (int bx = 0, cx = 0; bx < bytes.Length; ++bx, ++cx)
            {
                b = ((byte)(bytes[bx] >> 4));
                result[cx] = (char)(b > 9 ? b + 0x37 + 0x20 : b + 0x30);

                b = ((byte)(bytes[bx] & 0x0F));
                result[++cx] = (char)(b > 9 ? b + 0x37 + 0x20 : b + 0x30);
            }
            return new string(result);
        }
        #endregion

        #region [public] ToCRC32(this byte[] bytes)
        public static int ToCRC32(this byte[] bytes)
        {
            CRC32 crc32 = new CRC32();
            crc32.SlurpBlock(bytes, 0, bytes.Length);            
            return crc32.Crc32Result;
        }
        #endregion

        #region [public] ToMD5(this byte[] bytes) 
        public static string ToMD5(this byte[] bytes)
        {
            return new MD5CryptoServiceProvider().ComputeHash(bytes).ToHexString();
        }
        #endregion
    }

    public static class IntPtrExtensions
    {
        #region [public] Add(this IntPtr ptr, int offset)
        public static IntPtr Add(this IntPtr ptr, int offset)
        {
            return new IntPtr((long)ptr + offset);
        }
        #endregion

        #region [public] Add(this IntPtr ptr, long offset)
        public static IntPtr Add(this IntPtr ptr, long offset)
        {
            return new IntPtr((long)ptr + offset);
        }
        #endregion

        #region [public] Subtract(this IntPtr ptr, int offset)
        public static IntPtr Subtract(this IntPtr ptr, int offset)
        {
            return new IntPtr((long)ptr - offset);
        }
        #endregion

        #region [public] Subtract(this IntPtr ptr, long offset)
        public static IntPtr Subtract(this IntPtr ptr, long offset)
        {
            return new IntPtr((long)ptr - offset);
        }
        #endregion

        #region [public] ToStruct<T>(this IntPtr ptr, int from)
        public static T ToStruct<T>(this IntPtr ptr, int from) where T : struct
        {
            return (T)Marshal.PtrToStructure(new IntPtr(ptr.ToInt64() + from), typeof(T));
        }
        #endregion

        #region [public] ToStruct<T>(this IntPtr ptr, uint from)
        public static T ToStruct<T>(this IntPtr ptr, uint from) where T : struct
        {
            return (T)Marshal.PtrToStructure(new IntPtr(ptr.ToInt64() + from), typeof(T));
        }
        #endregion
    }

    public static class NumericExtension
    {
        private static System.Random RandomSeed = new System.Random();
        private static readonly object lockObject = new object();

        #region [public] Reverse(this ushort value)
        public static ushort Reverse(this ushort value)
        {
            return (ushort)((value & 0xFFU) << 8 | (value & 0xFF00U) >> 8);
        }
        #endregion

        #region [public] Reverse(this uint value)
        public static uint Reverse(this uint value)
        {
            return (value & 0x000000FFU) << 24 | (value & 0x0000FF00U) << 8 |
                   (value & 0x00FF0000U) >> 8 | (value & 0xFF000000U) >> 24;
        }
        #endregion

        #region [public] Reverse(this ulong value)
        public static ulong Reverse(this ulong value)
        {
            return (value & 0x00000000000000FFUL) << 56 | (value & 0x000000000000FF00UL) << 40 |
                   (value & 0x0000000000FF0000UL) << 24 | (value & 0x00000000FF000000UL) << 8 |
                   (value & 0x000000FF00000000UL) >> 8 | (value & 0x0000FF0000000000UL) >> 24 |
                   (value & 0x00FF000000000000UL) >> 40 | (value & 0xFF00000000000000UL) >> 56;
        }
        #endregion

        #region [public] Random(this byte maxValue, byte minValue = 0)
        public static byte Random(this byte maxValue, byte minValue = 0)
        {
            lock (lockObject)
            {
                return (byte)RandomSeed.Next(minValue, maxValue);
            }
        }
        #endregion

        #region [public] Random(this short maxValue, short minValue = 0)
        public static short Random(this short maxValue, short minValue = 0)
        {
            lock (lockObject)
            {
                return (short)RandomSeed.Next(minValue, maxValue);
            }
        }
        #endregion

        #region [public] Random(this int maxValue, int minValue = 0)
        public static int Random(this int maxValue, int minValue = 0)
        {
            lock (lockObject)
            {
                return RandomSeed.Next(minValue, maxValue);
            }
        }
        #endregion        

        #region [public] Random(this long maxValue, long minValue = 0)
        public static long Random(this long maxValue, long minValue = 0)
        {
            lock (lockObject)
            {
                return (long)(RandomSeed.NextDouble() * (maxValue - minValue)) + minValue;
            }
        }
        #endregion        

        #region [public] Random(this float maxValue, float minValue = 0)
        public static float Random(this float maxValue, float minValue = 0)
        {
            lock (lockObject)
            {
                return (float)(RandomSeed.NextDouble() * (maxValue - minValue)) + minValue;
            }
        }
        #endregion

        #region [public] Random(this float maxValue, float minValue = 0)
        public static double Random(this double maxValue, double minValue = 0)
        {
            lock (lockObject)
            {
                return (double)(RandomSeed.NextDouble() * (maxValue - minValue)) + minValue;
            }
        }
        #endregion
    }

    public static class ConvertExtensions
    {
        // Type: System.String //
        #region [public] ToEnum<T>(this string input)
        public static T ToEnum<T>(this string input)
        {
            if (string.IsNullOrEmpty(input)) input = "0";
            return (T)Enum.Parse(typeof(T), input, true);
        }
        #endregion

        #region [public] ToBool(this string input)
        public static bool ToBool(this string input)
        {
            if (!string.IsNullOrEmpty(input))
            {
                input = input.Trim().ToLower();
                return (input == "enabled" || input == "enable" || input == "true" || input == "t" || input == "yes" || input == "on" || input == "y" || input == "1");
            }
            return false;
        }
        #endregion

        #region [public] ToBoolean(this string input)
        public static bool ToBoolean(this string input)
        {
            if (!string.IsNullOrEmpty(input))
            {
                input = input.Trim().ToLower();
                return (input == "enabled" || input == "enable" || input == "true" || input == "t" || input == "yes" || input == "on" || input == "y" || input == "1");
            }
            return false;
        }
        #endregion

        #region [public] ToInt16(this string value, short def = 0)
        public static short ToInt16(this string value, short def = 0)
        {
            short result = def;
            try
            {
                if (value.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                {
                    result = short.Parse(value.Substring(2), NumberStyles.HexNumber);
                }
                else if (value.StartsWith("0") || !short.TryParse(value, out result))
                {
                    result = short.Parse(value, NumberStyles.HexNumber);
                }
            }
            catch (Exception)
            {
                /* Ignore all exceptions */
            }
            return result;
        }
        #endregion

        #region [public] ToUInt16(this string value, ushort def = 0)
        public static ushort ToUInt16(this string value, ushort def = 0)
        {
            ushort result = def;
            try
            {
                if (value.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                {
                    result = ushort.Parse(value.Substring(2), NumberStyles.HexNumber);
                }
                else if (value.StartsWith("0") || !ushort.TryParse(value, out result))
                {
                    result = ushort.Parse(value, NumberStyles.HexNumber);
                }
            }
            catch (Exception)
            {
                /* Ignore all exceptions */
            }
            return result;
        }
        #endregion

        #region [public] ToInt32(this string value, int def = 0)
        public static int ToInt32(this string value, int def = 0)
        {
            int result = def;
            try
            {
                if (value.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                {
                    result = int.Parse(value.Substring(2), NumberStyles.HexNumber);
                }
                else if (value.StartsWith("0") || !int.TryParse(value, out result))
                {
                    result = int.Parse(value, NumberStyles.HexNumber);
                }
            }
            catch (Exception)
            {
                /* Ignore all exceptions */
            }
            return result;
        }
        #endregion

        #region [public] ToUInt32(this string value, uint def = 0)
        public static uint ToUInt32(this string value, uint def = 0)
        {
            uint result = def;
            try
            {
                if (value.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                {
                    result = uint.Parse(value.Substring(2), NumberStyles.HexNumber);
                }
                else if (value.StartsWith("0") || !uint.TryParse(value, out result))
                {
                    result = uint.Parse(value, NumberStyles.HexNumber);
                }
            }
            catch (Exception)
            {
                /* Ignore all exceptions */
            }
            return result;
        }
        #endregion

        #region [public] ToInt64(this string value, long def = 0)
        public static long ToInt64(this string value, long def = 0)
        {
            long result = def;
            try
            {
                if (value.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                {
                    result = long.Parse(value.Substring(2), NumberStyles.HexNumber);
                }
                else if (value.StartsWith("0") || !long.TryParse(value, out result))
                {
                    result = long.Parse(value, NumberStyles.HexNumber);
                }
            }
            catch (Exception)
            {
                /* Ignore all exceptions */
            }
            return result;
        }
        #endregion

        #region [public] ToUInt64(this string value, ulong def = 0)
        public static ulong ToUInt64(this string value, ulong def = 0)
        {
            ulong result = def;
            try
            {
                if (value.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                {
                    result = ulong.Parse(value.Substring(2), NumberStyles.HexNumber);
                }
                else if (value.StartsWith("0") || !ulong.TryParse(value, out result))
                {
                    result = ulong.Parse(value, NumberStyles.HexNumber);
                }
            }
            catch (Exception)
            {
                /* Ignore all exceptions */
            }
            return result;
        }
        #endregion

        #region [public] ToFloat(this string value, float def = 0)
        public static float ToFloat(this string value, float def = 0)
        {
            try
            {
                return float.Parse(value, NumberStyles.Float);
            }
            catch (Exception)
            {
                return def;
            }
        }
        #endregion

        #region [public] ToSingle(this string value, float def = 0)
        public static float ToSingle(this string value, float def = 0)
        {
            try
            {
                return float.Parse(value, NumberStyles.Float);
            }
            catch (Exception)
            {
                return def;
            }
        }
        #endregion

        #region [public] ToDouble(this string value, double def = 0)
        public static double ToDouble(this string value, double def = 0)
        {
            try
            {
                return double.Parse(value, NumberStyles.Float);
            }
            catch (Exception)
            {
                return def;
            }
        }
        #endregion

        // Type: System.DateTime //
        #region [public] ToInt32(this DateTime datetime)
        public static int ToInt32(this DateTime datetime)
        {
            TimeSpan span = datetime - new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            return Convert.ToInt32(span.TotalSeconds);
        }
        #endregion

        #region [public] ToUInt32(this DateTime datetime)
        public static uint ToUInt32(this DateTime datetime)
        {
            TimeSpan span = datetime - new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            return Convert.ToUInt32(span.TotalSeconds);
        }
        #endregion        

        // Type: Numeric //
        #region [public] ToDateTime(this int input)
        public static DateTime ToDateTime(this int input)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc).AddSeconds(input);
        }
        #endregion

        #region [public] ToDateTime(this uint input)
        public static DateTime ToDateTime(this uint input)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc).AddSeconds(input);
        }
        #endregion

        #region [public] ToDateTime(this string input)
        public static DateTime ToDateTime(this string input)
        {
            DateTime result = new DateTime();
            foreach (Match match in Regex.Matches(input, @"(\d+\s*(y|M|d|h|m|s)?)"))
            {
                if (match.Value.EndsWith("y")) result = result.AddYears(Int32.Parse(match.Value.Trim('y')));
                else if (match.Value.EndsWith("M")) result = result.AddMonths(Int32.Parse(match.Value.Trim('M')));
                else if (match.Value.EndsWith("d")) result = result.AddDays(Double.Parse(match.Value.Trim('d')));
                else if (match.Value.EndsWith("h")) result = result.AddHours(Double.Parse(match.Value.Trim('h')));
                else if (match.Value.EndsWith("m")) result = result.AddMinutes(Double.Parse(match.Value.Trim('m')));
                else if (match.Value.EndsWith("s")) result = result.AddSeconds(Double.Parse(match.Value.Trim('s')));
                else result = result.AddSeconds(Double.Parse(match.Value));
            }
            return result;
        }
        #endregion

        #region [public] ToTimeSpan(this string input)
        public static TimeSpan ToTimeSpan(this string input)
        {
            DateTime result = DateTime.Now;
            foreach (Match match in Regex.Matches(input, @"(\d+\s*(y|M|d|h|m|s)?)"))
            {
                if (match.Value.EndsWith("y")) result = result.AddYears(Int32.Parse(match.Value.Trim('y')));
                else if (match.Value.EndsWith("M")) result = result.AddMonths(Int32.Parse(match.Value.Trim('M')));
                else if (match.Value.EndsWith("d")) result = result.AddDays(Double.Parse(match.Value.Trim('d')));
                else if (match.Value.EndsWith("h")) result = result.AddHours(Double.Parse(match.Value.Trim('h')));
                else if (match.Value.EndsWith("m")) result = result.AddMinutes(Double.Parse(match.Value.Trim('m')));
                else if (match.Value.EndsWith("s")) result = result.AddSeconds(Double.Parse(match.Value.Trim('s')));
                else result = result.AddSeconds(Double.Parse(match.Value));
            }
            return result.Subtract(DateTime.Now);
        }
        #endregion        

        #region [public] ToTimeString(this int seconds)
        public static string ToTimeString(this int seconds)
        {
            string result = "0";
            TimeSpan time = TimeSpan.FromSeconds(seconds);
            if (time.TotalSeconds >= 1) result = time.Seconds.ToString(time.TotalSeconds > 10 ? "00" : "0");
            if (time.TotalMinutes >= 1) result = time.Minutes.ToString(time.TotalMinutes > 10 ? "00" : "0") + ":" + result;
            if (time.TotalHours >= 1) result = Math.Ceiling(time.TotalHours).ToString() + ":" + result;
            return result;
        }
        #endregion

        #region [public] ToTimeString(this float seconds)
        public static string ToTimeString(this float seconds)
        {
            string result = "0";
            TimeSpan time = TimeSpan.FromSeconds(seconds);
            if (time.TotalSeconds >= 1) result = time.Seconds.ToString(time.TotalSeconds > 10 ? "00" : "0");
            if (time.TotalMinutes >= 1) result = time.Minutes.ToString(time.TotalMinutes > 10 ? "00" : "0") + ":" + result;
            if (time.TotalHours >= 1) result = Math.Ceiling(time.TotalHours).ToString() + ":" + result;
            return result;
        }
        #endregion

        #region [public] ToTimeString(this double seconds)
        public static string ToTimeString(this double seconds)
        {
            string result = "0";
            TimeSpan time = TimeSpan.FromSeconds(seconds);
            if (time.TotalSeconds >= 1) result = time.Seconds.ToString(time.TotalSeconds > 10 ? "00" : "0");
            if (time.TotalMinutes >= 1) result = time.Minutes.ToString(time.TotalMinutes > 10 ? "00" : "0") + ":" + result;
            if (time.TotalHours >= 1) result = Math.Ceiling(time.TotalHours).ToString() + ":" + result;
            return result;
        }
        #endregion

        #region [public] ToString(this TimeSpan time)
        public static string ToString(this TimeSpan time, string format = "")
        {
            if (string.IsNullOrEmpty(format))
            {
                string result = "0";
                if (time.TotalSeconds >= 1) result = time.Seconds.ToString(time.TotalSeconds > 10 ? "00" : "0");
                if (time.TotalMinutes >= 1) result = time.Minutes.ToString(time.TotalMinutes > 10 ? "00" : "0") + ":" + result;
                if (time.TotalHours >= 1) result = Math.Ceiling(time.TotalHours).ToString() + ":" + result;
                return result;
            }
            return new DateTime(time.Ticks).ToString(format);
        }
        #endregion

        // Type: System.Object //
        #region [public] ToBool(this object obj)
        public static bool ToBool(this object obj)
        {
            if (obj is bool) return (bool)obj;
            return false;
        }
        #endregion

        #region [public] ToBoolean(this object obj)
        public static bool ToBoolean(this object obj)
        {
            if (obj is bool) return (bool)obj;
            return false;
        }
        #endregion

        #region [public] ToInt16(this object obj)
        public static short ToInt16(this object obj)
        {
            if (obj is short) return (short)obj;
            return 0;
        }
        #endregion

        #region [public] ToUInt16(this object obj)
        public static ushort ToUInt16(this object obj)
        {
            if (obj is ushort) return (ushort)obj;
            return 0;
        }
        #endregion

        #region [public] ToInt32(this object obj)
        public static int ToInt32(this object obj)
        {
            if (obj is int) return (int)obj;
            return 0;
        }
        #endregion

        #region [public] ToUInt32(this object obj)
        public static uint ToUInt32(this object obj)
        {
            if (obj is uint) return (uint)obj;
            return 0;
        }
        #endregion

        #region [public] ToInt64(this object obj)
        public static long ToInt64(this object obj)
        {
            if (obj is long) return (long)obj;
            return 0;
        }
        #endregion

        #region [public] ToUInt64(this object obj)
        public static ulong ToUInt64(this object obj)
        {
            if (obj is ulong) return (ulong)obj;
            return 0;
        }
        #endregion

        #region [public] ToFloat(this object obj)
        public static float ToFloat(this object obj)
        {
            if (obj is float) return (float)obj;
            return 0f;
        }
        #endregion

        #region [public] ToSingle(this object obj)
        public static float ToSingle(this object obj)
        {
            if (obj is float) return (float)obj;
            return 0f;
        }
        #endregion

        #region [public] ToDouble(this object obj)
        public static double ToDouble(this object obj)
        {
            if (obj is double) return (double)obj;
            return 0f;
        }
        #endregion

        #region [public] ToString(this object obj)
        public static string ToString(this object obj)
        {
            if (obj != null) return obj.ToString();
            return string.Empty;
        }
        #endregion        
    }

    public static class VersionExtension
    {
        /// <summary>
        /// Conversion version to long value (maximum version 9999.9999.9999.9999999).
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        #region [public] ToInt64(this System.Version value)
        public static long ToInt64(this System.Version value)
        {
            int major = value.Major; if (value.Major > 9999) major = 9999;
            int minor = value.Minor; if (value.Minor > 9999) minor = 9999;
            int build = value.Build; if (value.Build > 9999) build = 9999;
            int revision = value.Revision; if (value.Revision > 9999999) revision = 9999999;
            return (major * 1000000000000000L + minor * 100000000000L + build * 10000000L + revision);
        }
        #endregion

        /// <summary>
        /// Convertion long value to version (maximum version 9999.9999.9999.9999999).
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        #region [public] ToVersion(this long value)
        public static System.Version ToVersion(this long value)
        {
            int major = 0, minor = 0, build = 0;
            if (value >= 1000000000000000L) { major = (int)(value / 1000000000000000L); value -= major * 1000000000000000L; }
            if (value >= 100000000000L) { minor = (int)(value / 100000000000L); value -= minor * 100000000000L; }
            if (value >= 10000000L) { build = (int)(value / 10000000L); value -= build * 10000000L; }
            return new Version(major, minor, build, (int)value);
        }
        #endregion
    }

    public static class ArrayExtension
    {
        #region Add<T>(this T[] array, T item)
        public static T[] Add<T>(this T[] array, T item)
        {
            Array.Resize(ref array, array.Length + 1);
            array[array.Length-1] = item;
            return array;
        }
        #endregion

        #region AddRange<T>(this T[] array, T[] items)
        public static T[] AddRange<T>(this T[] array, T[] items)
        {
            int length = array.Length;
            Array.Resize(ref array, length + items.Length);
            Array.Copy(items, 0, array, length, items.Length);            
            return array;
        }
        #endregion

        #region Remove<T>(this T[] array, T item)
        public static T[] Remove<T>(this T[] array, T item)
        {
            int index = Array.IndexOf(array, item);
            if (index == -1) return array;
            return array.RemoveAt(index);
        }
        #endregion

        #region RemoveAt<T>(this T[] array, int index)
        public static T[] RemoveAt<T>(this T[] array, int index)
        {
            if (array.Length == 0)
            {
                return array;
            }

            if (index >= array.Length)
            {
                index = array.Length - 1;
            }

            T[] dest = new T[array.Length - 1];

            if (index > 0)
            {
                Array.Copy(array, 0, dest, 0, index);
            }

            if (index < array.Length - 1)
            {
                Array.Copy(array, index + 1, dest, index, array.Length - index - 1);
            }
            return dest;
        }
        #endregion

        #region RemoveAll<T>(this T[] array, T item)
        public static T[] RemoveAll<T>(this T[] array, T item)
        {
            int count = 0;
            T[] dest = new T[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                if (!array[i].Equals(item))
                {
                    dest[count++] = array[i];
                }
            }          
            Array.Resize(ref dest, count);
            return dest;
        }
        #endregion
    }

    public static class EnumExtension
    {
        #region Has<T>(this Enum flags, T value) where T : struct
        public static bool Has<T>(this Enum flags, T value) where T : struct
        {
            UInt64 iFlags = Convert.ToUInt64(flags);
            UInt64 iValue = Convert.ToUInt64(value);
            return ((iFlags & iValue) == iValue);
        }
        #endregion

        #region SetFlag<T>(this Enum flags, T value, bool state = true)
        public static T SetFlag<T>(this Enum flags, T value, bool state = true)
        {
            if (!Enum.IsDefined(typeof(T), value)) throw new ArgumentException("Enum value and flags types don't match.");
            if (state) return (T)Enum.ToObject(typeof(T), Convert.ToUInt64(flags) | Convert.ToUInt64(value));
            return (T)Enum.ToObject(typeof(T), Convert.ToUInt64(flags) & ~Convert.ToUInt64(value));
        }
        #endregion
    }

    public static class SerializationExtension
    {
        #region [public] Serialize(this T structure)
        public static byte[] Serialize<T>(this T structure) where T : struct
        {
            byte[] bytes = new byte[Marshal.SizeOf(typeof(T))];
            IntPtr pointer = Marshal.AllocHGlobal(bytes.Length);
            Marshal.StructureToPtr(structure, pointer, true);
            Marshal.Copy(pointer, bytes, 0, bytes.Length);
            Marshal.FreeHGlobal(pointer);
            return bytes;
        }
        #endregion

        #region [public] Deserialize(this byte[] bytes)
        public static T Deserialize<T>(this byte[] bytes) where T : struct
        {
            try
            {
                GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
                T ticket = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
                handle.Free();
                return ticket;
            }
            catch (Exception)
            {
                // Ignore Exceptions //
            }
            return default(T);
        }
        #endregion  
    }    

    #region [struct] PROCESSENTRY32
    [StructLayout(LayoutKind.Sequential)]
    public struct PROCESSENTRY32
    {
        public int dwSize;
        public uint cntUsage;
        public uint th32ProcessID;
        public IntPtr th32DefaultHeapID;
        public uint th32ModuleID;
        public uint cntThreads;
        public uint th32ParentProcessID;
        public int pcPriClassBase;
        public uint dwFlags;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string szExeFile;
    }
    #endregion

    #region [struct] MODULEENTRY32
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct MODULEENTRY32
    {
        public int dwSize;
        public uint th32ModuleID;
        public uint th32ProcessID;
        public uint GlblcntUsage;
        public uint ProccntUsage;
        public IntPtr modBaseAddr;
        public uint modBaseSize;
        public IntPtr hModule;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string szModule;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string szExePath;
    }
    #endregion

    #region [struct] MODULEINFO
    [StructLayout(LayoutKind.Sequential)]
    public struct MODULEINFO
    {
        public IntPtr lpBaseOfDll;
        public uint SizeOfImage;
        public IntPtr EntryPoint;
    }
    #endregion

    public static class ProcessExtension
    {
        #region [structs] LUID & TOKEN
        [StructLayout(LayoutKind.Sequential)]
        public struct LUID
        {
            public uint LowPart;
            public int HighPart;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct LUID_AND_ATTRIBUTES
        {
            public LUID Luid;
            public uint Attributes;
        }

        public enum TOKEN_ELEVATION_TYPE
        {
            TokenElevationTypeDefault = 1,
            TokenElevationTypeFull,
            TokenElevationTypeLimited
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct TOKEN_PRIVILEGES
        {
            public uint PrivilegeCount;
            public LUID Luid;
            public uint Attributes;
        }
        #endregion

        public enum TOKEN_INFORMATION_CLASS
        {
            TokenUser = 1,
            TokenGroups,
            TokenPrivileges,
            TokenOwner,
            TokenPrimaryGroup,
            TokenDefaultDacl,
            TokenSource,
            TokenType,
            TokenImpersonationLevel,
            TokenStatistics,
            TokenRestrictedSids,
            TokenSessionId,
            TokenGroupsAndPrivileges,
            TokenSessionReference,
            TokenSandBoxInert,
            TokenAuditPolicy,
            TokenOrigin,
            TokenElevationType,
            TokenLinkedToken,
            TokenElevation,
            TokenHasRestrictions,
            TokenAccessInformation,
            TokenVirtualizationAllowed,
            TokenVirtualizationEnabled,
            TokenIntegrityLevel,
            TokenUIAccess,
            TokenMandatoryPolicy,
            TokenLogonSid,
            MaxTokenInfoClass
        }

        public static uint PAGE_NOACCESS = 0x01;
        public static uint PAGE_READONLY = 0x02;
        public static uint PAGE_READWRITE = 0x04;
        public static uint PAGE_WRITECOPY = 0x08;
        public static uint PAGE_EXECUTE = 0x10;
        public static uint PAGE_EXECUTE_READ = 0x20;
        public static uint PAGE_EXECUTE_READWRITE = 0x40;
        public static uint PAGE_EXECUTE_WRITECOPY = 0x80;

        public static uint TH32CS_SNAPHEAPLIST = 0x00000001;
        public static uint TH32CS_SNAPPROCESS = 0x00000002;
        public static uint TH32CS_SNAPTHREAD = 0x00000004;
        public static uint TH32CS_SNAPMODULE = 0x00000008;
        public static uint TH32CS_SNAPMODULE32 = 0x00000010;
        public static uint TH32CS_SNAPALL = 0x0000000F;
        public static uint TH32CS_INHERIT = 0x80000000;

        public static uint PROCESS_ALL_ACCESS = 0x001F0FFF;
        public static uint PROCESS_TERMINATE = 0x00000001;
        public static uint PROCESS_CREATE_THREAD = 0x00000002;
        public static uint PROCESS_VM_OPERATION = 0x00000008;
        public static uint PROCESS_VM_READ = 0x00000010;
        public static uint PROCESS_VM_WRITE = 0x00000020;
        public static uint PROCESS_DUP_HANDLE = 0x00000040;
        public static uint PROCESS_CREATE_PROCESS = 0x00000080;
        public static uint PROCESS_SET_QUOTA = 0x00000100;
        public static uint PROCESS_SET_INFORMATION = 0x00000200;
        public static uint PROCESS_QUERY_INFORMATION = 0x00000400;
        public static uint PROCESS_SUSPEND_RESUME = 0x00000800;
        public static uint PROCESS_QUERY_LIMITED_INFORMATION = 0x00001000;
        public static uint SYNCHRONIZE = 0x00100000;

        public static uint STANDARD_RIGHTS_REQUIRED = 0x000F0000;
        public static uint STANDARD_RIGHTS_READ = 0x00020000;
        public static uint TOKEN_ASSIGN_PRIMARY = 0x00000001;
        public static uint TOKEN_DUPLICATE = 0x00000002;
        public static uint TOKEN_IMPERSONATE = 0x00000004;
        public static uint TOKEN_QUERY = 0x00000008;
        public static uint TOKEN_QUERY_SOURCE = 0x00000010;
        public static uint TOKEN_ADJUST_PRIVILEGES = 0x00000020;
        public static uint TOKEN_ADJUST_GROUPS = 0x00000040;
        public static uint TOKEN_ADJUST_DEFAULT = 0x00000080;
        public static uint TOKEN_ADJUST_SESSIONID = 0x00000100;
        public static uint TOKEN_READ = 0x00020008;
        public static uint TOKEN_ALL_ACCESS = 0x000f01ff;

        public const uint SE_PRIVILEGE_ENABLED_BY_DEFAULT = 0x00000001;
        public const uint SE_PRIVILEGE_ENABLED = 0x00000002;
        public const uint SE_PRIVILEGE_REMOVED = 0x00000004;
        public const uint SE_PRIVILEGE_USED_FOR_ACCESS = 0x80000000;

        public const string SE_DEBUG_NAME = "SeDebugPrivilege";

        #region [ Native API Import ]
        private class Native
        {
            private const string PSAPI = "PSAPI.DLL";
            private const string USER32 = "USER32.DLL";
            private const string SHELL32 = "SHELL32.DLL";
            private const string KERNEL32 = "KERNEL32.DLL";
            private const string ADVAPI32 = "ADVAPI32.DLL";

            [DllImport(SHELL32, SetLastError = true)]
            public static extern bool IsUserAnAdmin();

            [DllImport(USER32)]
            public static extern int GetWindowTextLength(IntPtr hWnd);

            [DllImport(USER32)]
            public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

            [DllImport(ADVAPI32, SetLastError = true)]
            public static extern bool OpenProcessToken(IntPtr ProcessHandle, uint DesiredAccess, out IntPtr TokenHandle);

            [DllImport(ADVAPI32, SetLastError = true)]
            public static extern bool GetTokenInformation(IntPtr TokenHandle, TOKEN_INFORMATION_CLASS TokenInformationClass, IntPtr TokenInformation, int TokenInformationLength, out int ReturnLength);

            [DllImport(ADVAPI32, SetLastError = true)]
            public static extern bool PrivilegeCheck(IntPtr ClientToken, IntPtr RequiredPrivileges, ref int pfResult);

            [DllImport(ADVAPI32, SetLastError = true)]
            public static extern bool LookupPrivilegeValue(string lpSystemName, string lpName, out LUID lpLuid);

            [DllImport(ADVAPI32, SetLastError = true)]
            public static extern bool AdjustTokenPrivileges(IntPtr TokenHandle, bool DisableAllPrivileges, ref TOKEN_PRIVILEGES NewState, int BufferLength, IntPtr PreviousState, IntPtr ReturnLength);

            [DllImport(KERNEL32, SetLastError = true)]
            public static extern IntPtr CreateToolhelp32Snapshot(uint dwFlags, uint th32ProcessID);

            [DllImport(KERNEL32, SetLastError = true)]
            public static extern bool Process32First(IntPtr hSnapshot, ref PROCESSENTRY32 lppe);

            [DllImport(KERNEL32, SetLastError = true)]
            public static extern bool Process32Next(IntPtr hSnapshot, ref PROCESSENTRY32 lppe);

            [DllImport(KERNEL32, SetLastError = true)]
            public static extern IntPtr OpenProcess(UInt32 dwDesiredAccess, Int32 bInheritHandle, UInt32 dwProcessId);

            [DllImport(KERNEL32, SetLastError = true)]
            public static extern bool VirtualProtect(IntPtr address, uint size, uint newProtect, out uint oldProtect);

            [DllImport(KERNEL32)]
            public static extern bool ReadProcessMemory(IntPtr handle, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, out int lpNumberOfBytesRead);

            [DllImport(KERNEL32, SetLastError = true)]
            public static extern bool WriteProcessMemory(IntPtr handle, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, out int lpNumberOfBytesWritten);

            [DllImport(KERNEL32, SetLastError = true)]
            public static extern bool Module32First(IntPtr handle, ref MODULEENTRY32 entry);

            [DllImport(KERNEL32, SetLastError = true)]
            public static extern bool Module32Next(IntPtr handle, ref MODULEENTRY32 entry);

            [DllImport(KERNEL32)]
            public static extern int CloseHandle(IntPtr handle);

            [DllImport(KERNEL32, CallingConvention = CallingConvention.StdCall)]
            public static extern uint GetCurrentProcessId();

            [DllImport(KERNEL32, CallingConvention = CallingConvention.StdCall)]
            public static extern IntPtr GetCurrentProcess();

            [DllImport(PSAPI, SetLastError = true, CharSet = CharSet.Auto)]
            public static extern uint GetModuleFileNameEx(IntPtr hProcess, IntPtr hModule, [Out]StringBuilder lpBaseName, [In][MarshalAs(UnmanagedType.U4)]int nSize);

            [DllImport(PSAPI)]
            public static extern uint GetProcessImageFileName(IntPtr hProcess, [Out]StringBuilder lpImageFileName, [In][MarshalAs(UnmanagedType.U4)] int nSize);

            [DllImport(PSAPI, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
            public static extern int EnumProcessModules(IntPtr hProcess, [Out]IntPtr lphModule, uint cb, out uint lpcbNeeded);

            [DllImport(PSAPI, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
            public static extern uint GetModuleBaseName(IntPtr hProcess, IntPtr hModule, [Out]StringBuilder lpBaseName, uint nSize);

            [DllImport(PSAPI, SetLastError = true)]
            public static extern bool GetModuleInformation(IntPtr hProcess, IntPtr hModule, out MODULEINFO lpmodinfo, uint cb);
        }
        #endregion

        private static Dictionary<int, IntPtr> Dictionary = new Dictionary<int, IntPtr>();

        #region [public] OpenMemory(this Process process)
        public static bool OpenMemory(this Process process)
        {
            IntPtr handle;

            if (Dictionary.TryGetValue(process.Id, out handle) && handle != IntPtr.Zero)
            {
                return true;
            }

            handle = Native.OpenProcess(PROCESS_ALL_ACCESS, 0, (uint)process.Id);

            if (handle != IntPtr.Zero)
            {
                Dictionary[process.Id] = handle;
                return true;
            }            

            return false;
        }
        #endregion

        #region [public] CloseMemory(this Process process)
        public static void CloseMemory(this Process process)
        {
            IntPtr handle;
            if (Dictionary.TryGetValue(process.Id, out handle))
            {
                if (handle != IntPtr.Zero)
                {
                    Native.CloseHandle(handle);
                }
                Dictionary.Remove(process.Id);
            }
        }
        #endregion

        #region [public] GetModules(this Process process)
        public static MODULEENTRY32[] GetModules(this Process process)
        {
            List<MODULEENTRY32> modules = new List<MODULEENTRY32>();

            IntPtr snapshot = Native.CreateToolhelp32Snapshot(TH32CS_SNAPMODULE | TH32CS_SNAPMODULE32, (uint)process.Id);
            if (snapshot != IntPtr.Zero)
            {
                MODULEENTRY32 moduleEntry = new MODULEENTRY32();
                moduleEntry.dwSize = Marshal.SizeOf(moduleEntry);

                if (Native.Module32First(snapshot, ref moduleEntry))
                {
                    do
                    {
                        modules.Add(moduleEntry);
                        moduleEntry = new MODULEENTRY32();
                        moduleEntry.dwSize = Marshal.SizeOf(moduleEntry);
                    }
                    while (Native.Module32Next(snapshot, ref moduleEntry));
                }

                Native.CloseHandle(snapshot);
            }
            return modules.ToArray();
        }
        #endregion

        #region [public] GetModule(this Process process, string szFilename) 
        public static MODULEENTRY32 GetModule(this Process process, string szFilename)
        {
            MODULEENTRY32 moduleEntry = new MODULEENTRY32();
            moduleEntry.dwSize = Marshal.SizeOf(moduleEntry);

            IntPtr snapshot = Native.CreateToolhelp32Snapshot(TH32CS_SNAPMODULE | TH32CS_SNAPMODULE32, (uint)process.Id);
            if (snapshot != IntPtr.Zero)
            {
                Native.Module32First(snapshot, ref moduleEntry);
                do
                {
                    if (moduleEntry.szModule.Equals(szFilename, StringComparison.CurrentCultureIgnoreCase))
                    {
                        Native.CloseHandle(snapshot);
                        return moduleEntry;
                    }
                }
                while (Native.Module32Next(snapshot, ref moduleEntry));

                Native.CloseHandle(snapshot);
            }
            return new MODULEENTRY32();
        }
        #endregion        

        #region [public] GetModuleBaseAddress(this Process process, string szFilename)
        public static long GetModuleBaseAddress(this Process process, string szFilename)
        {
            long baseAddress = 0;

            IntPtr snapshot = Native.CreateToolhelp32Snapshot(TH32CS_SNAPMODULE | TH32CS_SNAPMODULE32, (uint)process.Id);

            if (snapshot != IntPtr.Zero)
            {
                MODULEENTRY32 moduleEntry = new MODULEENTRY32();
                moduleEntry.dwSize = Marshal.SizeOf(moduleEntry);

                Native.Module32First(snapshot, ref moduleEntry);
                do
                {
                    if (moduleEntry.szModule.Equals(szFilename, StringComparison.CurrentCultureIgnoreCase))
                    {
                        baseAddress = (long)moduleEntry.modBaseAddr;
                        break;
                    }
                }
                while (Native.Module32Next(snapshot, ref moduleEntry));

                Native.CloseHandle(snapshot);
            }
            return baseAddress;
        }
        #endregion

        #region [public] GetModuleEntryAddress(this Process process, string szFilename)
        public static long GetModuleEntryAddress(this Process process, string szFilename)
        {
            long entryAddress = 0;

            IntPtr snapshot = Native.CreateToolhelp32Snapshot(TH32CS_SNAPMODULE | TH32CS_SNAPMODULE32, (uint)process.Id);

            if (snapshot != IntPtr.Zero)
            {
                IntPtr handle;
                MODULEENTRY32 moduleEntry = new MODULEENTRY32();
                moduleEntry.dwSize = Marshal.SizeOf(moduleEntry);

                Native.Module32First(snapshot, ref moduleEntry);
                do
                {
                    if (moduleEntry.szModule.Equals(szFilename, StringComparison.CurrentCultureIgnoreCase))
                    {
                        entryAddress = (long)moduleEntry.modBaseAddr;

                        if (Dictionary.TryGetValue(process.Id, out handle) && handle != IntPtr.Zero)
                        {
                            MODULEINFO moduleInfo = new MODULEINFO();
                            if (Native.GetModuleInformation(handle, moduleEntry.hModule, out moduleInfo, 1024))
                            {
                                entryAddress = (uint)moduleInfo.EntryPoint;
                            }
                        }
                        break;
                    }
                }
                while (Native.Module32Next(snapshot, ref moduleEntry));

                Native.CloseHandle(snapshot);
            }
            return entryAddress;
        }
        #endregion

        // Read from Memory //
        #region [public] ReadMemory(this Process process, long lpAddress, int length)
        public static byte[] ReadMemory(this Process process, long lpAddress, int length)
        {
            IntPtr handle;
            if (Dictionary.TryGetValue(process.Id, out handle) && handle != IntPtr.Zero && lpAddress != 0)
            {
                int bytesRead = 0;
                byte[] buffer = new byte[length];
                if (Native.ReadProcessMemory(handle, (IntPtr)lpAddress, buffer, length, out bytesRead) && bytesRead == length)
                {
                    return buffer;
                }                
            }

            return new byte[0];
        }
        #endregion

        #region [public] ReadAddress(this Process process, long lpAddress)
        public static long ReadAddress(this Process process, long lpAddress, params long[] offset)
        {
            IntPtr handle;
            if (Dictionary.TryGetValue(process.Id, out handle) && handle != IntPtr.Zero && lpAddress != 0)
            {
                int bytesReaded = 0;
                int size = sizeof(long);
                byte[] lpBuffer = new byte[size];

                if (Native.ReadProcessMemory(handle, new IntPtr(lpAddress), lpBuffer, size, out bytesReaded))
                {
                    for (int i = 0; i < offset.Length; i++)
                    {
                        lpAddress = lpBuffer.ToInt64() + offset[i];
                        if (!Native.ReadProcessMemory(handle, new IntPtr(lpAddress), lpBuffer, size, out bytesReaded))
                        {
                            return 0;
                        }
                    }
                    return lpBuffer.ToInt64();
                }
            }
            return 0;
        }
        #endregion

        #region [public] ReadInt16(this Process process, long lpAddress)
        public static short ReadInt16(this Process process, long lpAddress)
        {
            return ReadShort(process, lpAddress);
        }

        public static short ReadShort(this Process process, long lpAddress)
        {
            IntPtr handle;
            if (Dictionary.TryGetValue(process.Id, out handle) && handle != IntPtr.Zero && lpAddress != 0)
            {
                int bytesReaded = 0;
                int size = sizeof(int);
                byte[] lpBuffer = new byte[size];
                if (Native.ReadProcessMemory(handle, new IntPtr(lpAddress), lpBuffer, size, out bytesReaded))
                {
                    return lpBuffer.ToInt16();
                }
            }

            return 0;
        }
        #endregion

        #region [public] ReadUInt16(this Process process, long lpAddress)
        public static ushort ReadUInt16(this Process process, long lpAddress)
        {
            return ReadUShort(process, lpAddress);
        }

        public static ushort ReadUShort(this Process process, long lpAddress)
        {
            IntPtr handle;
            if (Dictionary.TryGetValue(process.Id, out handle) && handle != IntPtr.Zero && lpAddress != 0)
            {
                int bytesReaded = 0;
                int size = sizeof(uint);
                byte[] lpBuffer = new byte[size];
                if (Native.ReadProcessMemory(handle, new IntPtr(lpAddress), lpBuffer, size, out bytesReaded))
                {
                    return lpBuffer.ToUInt16();
                }
            }

            return 0;
        }
        #endregion

        #region [public] ReadInt32(this Process process, long lpAddress)
        public static int ReadInt32(this Process process, long lpAddress)
        {
            return ReadInt(process, lpAddress);
        }        

        public static int ReadInt(this Process process, long lpAddress)
        {
            IntPtr handle;
            if (Dictionary.TryGetValue(process.Id, out handle) && handle != IntPtr.Zero && lpAddress != 0)
            {
                int bytesReaded = 0;
                int size = sizeof(int);
                byte[] lpBuffer = new byte[size];
                if (Native.ReadProcessMemory(handle, new IntPtr(lpAddress), lpBuffer, size, out bytesReaded))
                {
                    return lpBuffer.ToInt32();
                }
            }

            return 0;
        }
        #endregion

        #region [public] ReadUInt32(this Process process, long lpAddress)
        public static uint ReadUInt32(this Process process, long lpAddress)
        {
            return ReadUInt(process, lpAddress);
        }

        public static uint ReadUInt(this Process process, long lpAddress)
        {
            IntPtr handle;
            if (Dictionary.TryGetValue(process.Id, out handle) && handle != IntPtr.Zero && lpAddress != 0)
            {
                int bytesReaded = 0;
                int size = sizeof(uint);
                byte[] lpBuffer = new byte[size];
                if (Native.ReadProcessMemory(handle, new IntPtr(lpAddress), lpBuffer, size, out bytesReaded))
                {
                    return lpBuffer.ToUInt32();
                }
            }

            return 0;
        }
        #endregion

        #region [public] ReadInt64(this Process process, long lpAddress)
        public static long ReadLong(this Process process, long lpAddress)
        {
            return ReadInt64(process, lpAddress);
        }

        public static long ReadInt64(this Process process, long lpAddress)
        {
            IntPtr handle;
            if (Dictionary.TryGetValue(process.Id, out handle) && handle != IntPtr.Zero && lpAddress != 0)
            {
                int bytesReaded = 0;
                int size = sizeof(long);
                byte[] lpBuffer = new byte[size];
                if (Native.ReadProcessMemory(handle, new IntPtr(lpAddress), lpBuffer, size, out bytesReaded))
                {
                    return lpBuffer.ToInt64();
                }
            }

            return 0;
        }
        #endregion

        #region [public] ReadUInt64(this Process process, long lpAddress)
        public static ulong ReadULong(this Process process, long lpAddress)
        {
            return ReadUInt64(process, lpAddress);
        }

        public static ulong ReadUInt64(this Process process, long lpAddress)
        {
            IntPtr handle;
            if (Dictionary.TryGetValue(process.Id, out handle) && handle != IntPtr.Zero && lpAddress != 0)
            {
                int bytesReaded = 0;
                int size = sizeof(ulong);
                byte[] lpBuffer = new byte[size];
                if (Native.ReadProcessMemory(handle, new IntPtr(lpAddress), lpBuffer, size, out bytesReaded))
                {
                    return lpBuffer.ToUInt64();
                }
            }

            return 0;
        }
        #endregion

        #region [public] ReadFloat(this Process process, long lpAddress)
        public static float ReadFloat(this Process process, long lpAddress)
        {
            return ReadSingle(process, lpAddress);
        }

        public static float ReadSingle(this Process process, long lpAddress)
        {
            IntPtr handle;
            if (Dictionary.TryGetValue(process.Id, out handle) && handle != IntPtr.Zero && lpAddress != 0)
            {
                int bytesReaded = 0;
                int size = sizeof(float);
                byte[] lpBuffer = new byte[size];
                if (Native.ReadProcessMemory(handle, (IntPtr)lpAddress, lpBuffer, size, out bytesReaded))
                {
                    return lpBuffer.ToFloat();
                }
            }

            return 0;
        }
        #endregion

        #region [public] ReadDouble(this Process process, long lpAddress)
        public static double ReadDouble(this Process process, long lpAddress)
        {
            IntPtr handle;
            if (Dictionary.TryGetValue(process.Id, out handle) && handle != IntPtr.Zero && lpAddress != 0)
            {
                int bytesReaded = 0;
                int size = sizeof(double);
                byte[] lpBuffer = new byte[size];
                if (Native.ReadProcessMemory(handle, (IntPtr)lpAddress, lpBuffer, size, out bytesReaded))
                {
                    return lpBuffer.ToDouble();
                }
            }

            return 0;
        }
        #endregion

        #region [public] ReadString(this Process process, long lpAddress)
        public static string ReadString(this Process process, long lpAddress)
        {
            return ReadString(process, lpAddress, 260);
        }
        #endregion

        #region [public] ReadString(this Process process, long lpAddress, int size)
        public static string ReadString(this Process process, long lpAddress, int size)
        {
            IntPtr handle;
            if (Dictionary.TryGetValue(process.Id, out handle) && handle != IntPtr.Zero && lpAddress != 0)
            {
                int bytesReaded = 0;
                byte[] lpBuffer = new byte[size];
                if (Native.ReadProcessMemory(handle, new IntPtr(lpAddress), lpBuffer, size, out bytesReaded))
                {
                    int nulled = lpBuffer.IndexOf(0);
                    if (nulled < 0) nulled = lpBuffer.Length;
                    return Encoding.UTF8.GetString(lpBuffer, 0, nulled);
                }
            }

            return string.Empty;
        }
        #endregion

        // Write to Memory //
        #region [public] WriteMemory(this Process process, long lpAddress, byte[] lpBuffer)
        public static bool WriteMemory(this Process process, long lpAddress, byte[] lpBuffer)
        {
            IntPtr handle;
            if (Dictionary.TryGetValue(process.Id, out handle) && handle != IntPtr.Zero && lpAddress != 0 && lpBuffer != null && lpBuffer.Length > 0)
            {
                uint oldProtect;
                int bytesWritten = 0;
                if (Native.VirtualProtect((IntPtr)lpAddress, (uint)lpBuffer.Length, PAGE_EXECUTE_READWRITE, out oldProtect))
                {
                    Native.WriteProcessMemory(handle, (IntPtr)lpAddress, lpBuffer, lpBuffer.Length, out bytesWritten);
                    Native.VirtualProtect((IntPtr)lpAddress, (uint)lpBuffer.Length, oldProtect, out oldProtect);
                }
                return (bytesWritten == lpBuffer.Length);
            }
            return false;
        }
        #endregion

        #region [public] Write(this Process process, long lpAddress, bool value)
        public static bool Write(this Process process, long lpAddress, bool value)
        {
            return WriteMemory(process, lpAddress, BitConverter.GetBytes(value));
        }
        #endregion

        #region [public] Write(this Process process, long lpAddress, byte value)
        public static bool Write(this Process process, long lpAddress, byte value)
        {
            return WriteMemory(process, lpAddress, BitConverter.GetBytes(value));
        }
        #endregion

        #region [public] Write(this Process process, long lpAddress, sbyte value)
        public static bool Write(this Process process, long lpAddress, sbyte value)
        {
            return WriteMemory(process, lpAddress, BitConverter.GetBytes(value));
        }
        #endregion        

        #region [public] Write(this Process process, long lpAddress, short value)
        public static bool Write(this Process process, long lpAddress, short value)
        {
            return WriteMemory(process, lpAddress, BitConverter.GetBytes(value));
        }
        #endregion

        #region [public] Write(this Process process, long lpAddress, ushort value)
        public static bool Write(this Process process, long lpAddress, ushort value)
        {
            return WriteMemory(process, lpAddress, BitConverter.GetBytes(value));
        }
        #endregion

        #region [public] Write(this Process process, long lpAddress, int value)
        public static bool Write(this Process process, long lpAddress, int value)
        {            
            return WriteMemory(process, lpAddress, BitConverter.GetBytes(value));
        }
        #endregion

        #region [public] Write(this Process process, long lpAddress, uint value)
        public static bool Write(this Process process, long lpAddress, uint value)
        {
            return WriteMemory(process, lpAddress, BitConverter.GetBytes(value));
        }
        #endregion

        #region [public] Write(this Process process, long lpAddress, long value)
        public static bool Write(this Process process, long lpAddress, long value)
        {
            return WriteMemory(process, lpAddress, BitConverter.GetBytes(value));
        }
        #endregion

        #region [public] Write(this Process process, long lpAddress, ulong value)
        public static bool Write(this Process process, long lpAddress, ulong value)
        {
            return WriteMemory(process, lpAddress, BitConverter.GetBytes(value));
        }
        #endregion

        #region [public] Write(this Process process, long lpAddress, float value)
        public static bool Write(this Process process, long lpAddress, float value)
        {
            return WriteMemory(process, lpAddress, BitConverter.GetBytes(value));
        }
        #endregion

        #region [public] Write(this Process process, long lpAddress, double value)
        public static bool Write(this Process process, long lpAddress, double value)
        {
            return WriteMemory(process, lpAddress, BitConverter.GetBytes(value));
        }
        #endregion
        
        #region [public] Write(this Process process, long lpAddress, string value)
        public static bool Write(this Process process, long lpAddress, string value)
        {
            return WriteMemory(process, lpAddress, Encoding.UTF8.GetBytes(value + '\0'));
        }
        #endregion
    }

    public static class UnityEngineExtension
    {
        // Vector2 //
        #region Vector2::AsString(this Vector2 vector)
        public static string AsString(this UnityEngine.Vector2 vector)
        {
            return string.Format("{0},{1}", vector.x, vector.y);
        }
        #endregion

        #region String::ToVector2(this string input)
        public static UnityEngine.Vector2 ToVector2(this string input)
        {
            UnityEngine.Vector2 vector = UnityEngine.Vector2.zero;
            string[] args = input.RemoveWhitespaces().Split(',');
            if (args.Length > 0) float.TryParse(args[0], out vector.x);
            if (args.Length > 1) float.TryParse(args[1], out vector.y);
            return vector;
        }
        #endregion

        // Vector3 //
        #region Vector3::Distance(this Vector3 from, Vector position)
        public static float Distance(this UnityEngine.Vector3 from, UnityEngine.Vector3 position)
        {
            UnityEngine.Vector3 vector = from - position;
            return vector.magnitude;
        }
        #endregion

        #region Vector3::Distance2D(this Vector3 from, Vector position)
        public static float Distance2D(this UnityEngine.Vector3 from, UnityEngine.Vector3 position)
        {
            UnityEngine.Vector3 vector = from - position;
            return UnityEngine.Mathf.Sqrt((vector.x * vector.x) + (vector.z * vector.z));
        }
        #endregion

        #region Vector3::Direction(this Vector3 from, Vector position)
        public static UnityEngine.Vector3 Direction(this UnityEngine.Vector3 from, UnityEngine.Vector3 position)
        {
            UnityEngine.Vector3 vector = position - from;
            return vector.normalized;
        }
        #endregion

        #region Vector3::AsString(this Vector3 vector)
        public static string AsString(this UnityEngine.Vector3 vector)
        {
            return string.Format("{0},{1},{2}", vector.x, vector.y, vector.z);
        }
        #endregion

        #region String::ToVector3(this string input)
        public static UnityEngine.Vector3 ToVector3(this string input)
        {
            UnityEngine.Vector3 vector = UnityEngine.Vector3.zero;
            string[] args = input.RemoveWhitespaces().Split(',');
            if (args.Length > 0) float.TryParse(args[0], out vector.x);
            if (args.Length > 1) float.TryParse(args[1], out vector.y);
            if (args.Length > 2) float.TryParse(args[2], out vector.z);
            return vector;
        }
        #endregion

        // Quaternion //
        #region Quaternion::AsString(this Quaternion quaternion)
        public static string AsString(this UnityEngine.Quaternion quaternion)
        {
            return string.Format("{0},{1},{2},{3}", quaternion.x, quaternion.y, quaternion.z, quaternion.w);
        }
        #endregion

        #region String::ToQuaternion(this string input)
        public static UnityEngine.Quaternion ToQuaternion(this string input)
        {
            UnityEngine.Quaternion quaternion = UnityEngine.Quaternion.identity;
            string[] args = input.RemoveWhitespaces().Split(',');
            if (args.Length > 0) float.TryParse(args[0], out quaternion.x);
            if (args.Length > 1) float.TryParse(args[1], out quaternion.y);
            if (args.Length > 2) float.TryParse(args[2], out quaternion.z);
            if (args.Length > 3) float.TryParse(args[3], out quaternion.w);
            return quaternion;
        }
        #endregion
    }
}
