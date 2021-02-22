using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Scripting;

namespace OrchardCore.Forms.Entities.Scripting
{
    public class ValidateRuleMethod : IGlobalMethodProvider
    {
        private static GlobalMethod Contains = new GlobalMethod
        {
            Name = "contains",
            Method = serviceProvider => (Func<string,string,bool>)((str,compare) =>
            {
                return str.Contains(compare, StringComparison.InvariantCultureIgnoreCase);
            })
        };
        private static GlobalMethod IsDate = new GlobalMethod
        {
            Name = "isDate",
            Method = serviceProvider => (Func<string,bool>)((str) =>
           {
               DateTime result;
               return DateTime.TryParse(str, CultureInfo.InvariantCulture, DateTimeStyles.None, out result);
           })
        };

        private static GlobalMethod IsDecimal = new GlobalMethod
        {
            Name = "isDecimal",
            Method = serviceProvider => (Func<string,bool>)((str) =>
            {
                return ValidateIs<Decimal>(str);
            })
        };
        private static GlobalMethod IsEmail = new GlobalMethod
        {
            Name = "isEmail",
            Method = serviceProvider => (Func<string,bool>)((str) =>
            {
                if (String.IsNullOrEmpty(str)) return false;
                string exp = @"^((([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+(\.([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+)*)|((\x22)((((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(([\x01-\x08\x0b\x0c\x0e-\x1f\x7f]|\x21|[\x23-\x5b]|[\x5d-\x7e]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(\\([\x01-\x09\x0b\x0c\x0d-\x7f]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF]))))*(((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(\x22)))@((([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-||_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.)+(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+|(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+([a-z]+|\d|-|\.{0,1}|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])?([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))$";
                return new Regex(exp, RegexOptions.IgnoreCase).IsMatch(str);
            })
        };
        private static GlobalMethod IsEmpty = new GlobalMethod
        {
            Name = "isEmpty",
            Method = serviceProvider => (Func<string,bool>)((str) =>
            {
                return string.IsNullOrEmpty(str);
            })
        };
        private static GlobalMethod IsInt = new GlobalMethod
        {
            Name = "isInt",
            Method = serviceProvider => (Func<string,bool>)((str) =>
            {
                return ValidateIs<int>(str);
            })
        };
        private static GlobalMethod IsIP = new GlobalMethod
        {
            Name = "isIP",
            Method = serviceProvider => (Func<string,bool>)((str) =>
            {
                return IsValidIPAddress(str);
            })
        };
        private static GlobalMethod IsURL = new GlobalMethod
        {
            Name = "isURL",
            Method = serviceProvider => (Func<string,bool>)((str) =>
            {
                string urlRegex = @"^http(s)?://([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?$";
                return Regex.IsMatch(str, urlRegex);
            })
        };
        private static GlobalMethod Matches = new GlobalMethod
        {
            Name = "matches",
            Method = serviceProvider => (Func<string,string,bool>)((str,regexStr) =>
            {
                return Regex.IsMatch(str, regexStr);
            })
        };
        public IEnumerable<GlobalMethod> GetMethods()
        {
            return new[] { Contains, IsDate, IsDecimal, IsEmail, IsEmpty, IsInt, IsIP, IsURL, Matches };
        }

        public static bool ValidateIs<T>(string value)
        {
            if (value == null)
            {
                return false;
            }

            var converter = System.ComponentModel.TypeDescriptor.GetConverter(typeof(T));

            try
            {
                T result = (T)converter.ConvertFromString(value.ToString());
                return result != null;
            }
            catch (Exception)
            {
                return false;
            }
        }


        public static bool IsValidIPAddress(string IpAddress)
        {
            try
            {
                IPAddress IP;
                if (IpAddress.Count(c => c == '.') == 3)
                {
                    bool flag = IPAddress.TryParse(IpAddress, out IP);
                    if (flag)
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
                    return false;
                }
            }
            catch (Exception) {
                return false;
            }
        }
    }
}
