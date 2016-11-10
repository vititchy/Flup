using System;
using System.Text;

namespace vt.extensions
{
    public static class StringExtensions
    {
        public static string EncodeToBase64(this string value)
        {
            var toEncodeAsBytes = Encoding.ASCII.GetBytes(value);
            var base64Str = Convert.ToBase64String(toEncodeAsBytes);
            return base64Str;
        }

        static public string DecodeFromBase64(this string value)
        {
            var encodedDataAsBytes = Convert.FromBase64String(value);
            string returnValue = Encoding.ASCII.GetString(encodedDataAsBytes);
            return returnValue;
        }

        public static string ToHexString(this string value)
        {
            var toEncodeAsBytes = Encoding.ASCII.GetBytes(value);

            var s = new StringBuilder();
            foreach (byte b in toEncodeAsBytes)
            {
                s.Append(b.ToString("x2"));
            }
            var result = s.ToString();
            return result;
        }

        public static string ToHexString2(this string value)
        {
            var result = string.Empty;
            foreach(var ch in value)
            {
                var i = (int)ch;
                result += i.ToString("X10");
            }
            return result;
        }


        public static string FromHexBytes(this string hex)
        {
            int l = hex.Length / 2;
            var b = new byte[l];
            for (int i = 0; i < l; ++i)
            {
                b[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }

            string result = Encoding.ASCII.GetString(b);
            return result;
        }


        public static string FromHexBytes2(this string hex)
        {
            var pos = 0;
            var hexLen = 10;
            var result = string.Empty;
            while (hex.Length >= (pos + hexLen))
            {
                var kus = hex.Substring(pos, hexLen);
                int value = Convert.ToInt32(kus, 16);
                var ch = (char)value;
                result += ch;
                pos += hexLen;
            }
            return result;
        }


        /// <summary>
        /// remove text at beginning of string
        /// </summary>
        public static string RemoveStartText(this string value, string startWith)
        {
            if ((value != null) && !string.IsNullOrEmpty(startWith))
            {
                if (value.StartsWith(startWith, StringComparison.InvariantCultureIgnoreCase))
                {
                    value = value.Substring(startWith.Length, value.Length - startWith.Length);
                }
            }
            return value;
        }

        /// <summary>
        /// remove text at end of string
        /// </summary>
        public static string RemoveEndText(this string value, string endWith, StringComparison comparisonType = StringComparison.InvariantCulture)
        {
            if ((value != null) && !string.IsNullOrEmpty(endWith))
            {
                if (value.EndsWith(endWith, comparisonType))
                {
                    value = value.Substring(0, value.Length - endWith.Length);
                }
            }
            return value;
        }

    }
}
