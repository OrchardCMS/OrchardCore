using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace OrchardCore.Forms.Helpers
{
    public static class ValidateExtensionMethods
    {
        public static bool IsContains(this string value, string compare)
        {
            return value.Contains(compare, StringComparison.InvariantCultureIgnoreCase);
        }


        public static bool IsNotNullOrEmpty(this string value)
        {
            return !string.IsNullOrEmpty(value);
        }

        public static bool IsNullOrEmpty(this string value)
        {
            return !value.IsNotNullOrEmpty();
        }
        public static bool IsNotNullOrWhiteSpace(this string value)
        {
            return !string.IsNullOrWhiteSpace(value);
        }

        public static bool IsNullOrWhiteSpace(this string value)
        {
            return !value.IsNotNullOrWhiteSpace();
        }

        public static bool IsBetweenLength(this string value, int min, int max)
        {
            if (value.IsNullOrEmpty() && min == 0)
            {
                return true; // if it's null it has length 0
            }
            else if (value.IsNullOrEmpty())
            {
                return false;
            }
            else
            {
                return value.Length >= min && value.Length <= max;
            }
        }

        public static bool IsMaxLength(this string value, int max)
        {
            if (value.IsNullOrEmpty())
            {
                return true; // if it's null it has length 0 and that has to be less than max
            }
            else
            {
                return value.Length <= max;
            }
        }

        public static bool IsMinLength(this string value, int min)
        {
            if (value.IsNullOrEmpty() && min == 0)
            {
                return true; // if it's null it has length 0
            }
            else if (value.IsNullOrEmpty())
            {
                return false;
            }
            else
            {
                return value.Length >= min;
            }
        }

        public static bool IsExactLength(this string value, int length)
        {
            return value.IsBetweenLength(length, length);
        }

        public static bool IsEmail(this string value)
        {
            if (value.IsNullOrEmpty())
            {
                return false; // if it's null it cannot possibly be an email
            }
            else
            {
                string exp = @"^((([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+(\.([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+)*)|((\x22)((((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(([\x01-\x08\x0b\x0c\x0e-\x1f\x7f]|\x21|[\x23-\x5b]|[\x5d-\x7e]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(\\([\x01-\x09\x0b\x0c\x0d-\x7f]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF]))))*(((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(\x22)))@((([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-||_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.)+(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+|(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+([a-z]+|\d|-|\.{0,1}|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])?([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))$";

                return new Regex(exp, RegexOptions.IgnoreCase).IsMatch(value);
            }
        }

        public static bool IsRegex(this string value, string exp)
        {
            if (value.IsNotNullOrEmpty())
            {
                return false;
            }

            string check = value.ToString();

            return new Regex(exp, RegexOptions.IgnoreCase).IsMatch(check);
        }

        public static bool IsEqualTo(this string value, string compare)
        {
            if (value.IsNullOrEmpty() && compare.IsNullOrEmpty())
            {
                return true;
            }
            if (value.IsNullOrEmpty() || compare.IsNullOrEmpty())
            {
                return false;
            }
            return String.Equals(value, compare, StringComparison.Ordinal);
        }

        public static bool IsDate(this string value)
        {
            return value.IsDate(CultureInfo.InvariantCulture);
        }

        public static bool IsDate(this string value, CultureInfo info)
        {
            return value.IsDate(CultureInfo.InvariantCulture, DateTimeStyles.None);
        }

        public static bool IsDate(this string value, CultureInfo info, DateTimeStyles styles)
        {
            if (value.IsNotNullOrEmpty())
            {
                DateTime result;

                if (DateTime.TryParse(value.ToString(), info, styles, out result))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false; // if it's null it cannot be a date
            }
        }

    }
}
