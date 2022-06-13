using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace FileTransfer
{
    internal class Helper
    {
        public static string GetEnumDescription(Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());

            DescriptionAttribute[] attributes =
                (DescriptionAttribute[])fi.GetCustomAttributes(
                typeof(DescriptionAttribute),
                false);

            if (attributes != null &&
                attributes.Length > 0)
                return attributes[0].Description;
            else
                return value.ToString();
        }
    }

    internal class Validator
    {
        static bool IsContainSpeicalChar(string str, bool allowSpace)
        {
            string exp = allowSpace ? "[^A-Za-z0-9 _-]" : "[^A-Za-z0-9_-]";

            Regex RgxUrl = new Regex(exp);
            return RgxUrl.IsMatch(str);
        }

        public static bool IsValid(string str, bool allowSpace, int minLen, int maxLen)
        {
            if (str == null || str == "") return false;
            if (str.Length < minLen) return false;
            if (str.Length > maxLen) return false;

            //checking for conatin space
            if (!allowSpace)
                if (str.Contains(" "))
                    return false;

            if (IsContainSpeicalChar(str, allowSpace))
                return false;

            //checking for all space character
            for (int i = 0; i < str.Length; i++)
            {
                if (str[i] != ' ')
                    break;

                //All space
                if (i == str.Length - 1)
                    return false;
            }

            return true;
        }

        public static bool IsValid(string str, bool AllowSpace, int MaxLen)
        {
            if (str == null || str == "") return false;
            if (str.Length > MaxLen) return false;

            //checking for conatin space
            if (!AllowSpace)
                if (str.Contains(" "))
                    return false;

            if (IsContainSpeicalChar(str, AllowSpace))
                return false;

            //checking for all space character
            for (int i = 0; i < str.Length; i++)
            {
                if (str[i] != ' ')
                    break;

                //All space
                if (i == str.Length - 1)
                    return false;
            }

            return true;
        }

        public static bool IsValid(string val, int MinVal, int MaxVal)
        {
            if (val == null || val == "") return false;

            int tempint = 0;
            if (int.TryParse(val, out tempint))
            {
                if (tempint >= MinVal && tempint <= MaxVal)
                    return true;
                else
                    return false;
            }
            else
            {
                return false;
            }
        }

        public static bool IsValid(string val, long MinVal, long MaxVal)
        {
            if (val == null || val == "") return false;

            long templong = 0;
            if (long.TryParse(val, out templong))
            {
                if (templong >= MinVal && templong <= MaxVal)
                    return true;
                else
                    return false;
            }
            else
            {
                return false;
            }
        }

        public static bool IsValid(string val, float MinVal, float MaxVal)
        {
            if (val == null || val == "") return false;

            float tempfloat = 0;
            if (float.TryParse(val, out tempfloat))
            {
                if (tempfloat >= MinVal && tempfloat <= MaxVal)
                    return true;
                else
                    return false;
            }
            else
            {
                return false;
            }
        }

        public static bool IsValid(string val, double MinVal, double MaxVal)
        {
            if (val == null || val == "") return false;

            double tempDouble = 0;
            if (double.TryParse(val, out tempDouble))
            {
                if (tempDouble >= MinVal && tempDouble <= MaxVal)
                    return true;
                else
                    return false;
            }
            else
            {
                return false;
            }
        }

        public static string TimeSpanToReadableString(TimeSpan span)
        {
            string formatted = string.Format("{0}{1}{2}{3}",
                span.Duration().Days > 0 ? string.Format("{0:0}d: ", span.Days) : string.Empty,
                span.Duration().Hours > 0 ? string.Format("{0:0}h: ", span.Hours) : string.Empty,
                span.Duration().Minutes > 0 ? string.Format("{0:0}m: ", span.Minutes) : string.Empty,
                span.Duration().Seconds > 0 ? string.Format("{0:0}s", span.Seconds) : string.Empty);

            if (formatted.EndsWith(", ")) formatted = formatted.Substring(0, formatted.Length - 2);

            if (string.IsNullOrEmpty(formatted)) formatted = "0s";

            return formatted;
        }
    }
}
