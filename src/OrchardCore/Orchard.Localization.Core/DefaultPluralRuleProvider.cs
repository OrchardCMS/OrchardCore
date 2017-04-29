using Orchard.Localization.Abstractions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Orchard.Localization.Core
{
    public class DefaultPluralRuleProvider : IPluralRuleProvider
    {
        private static Func<int, int> DefaultRule = n => (n != 1 ? 1 : 0);

        public Func<int, int> GetRule(CultureInfo culture)
        {
            var rule = GetRule(culture.Name);

            if (rule == null && culture.Parent != null)
            {
                rule = GetRule(culture.Parent.Name);
            }

            return rule ?? DefaultRule;
        }

        private Func<int, int> GetRule(string cultureName)
        {
            switch (cultureName)
            {
                case "ay":
                case "bo":
                case "cgg":
                case "dz":
                case "fa":
                case "id":
                case "ja":
                case "jbo":
                case "ka":
                case "kk":
                case "km":
                case "ko":
                case "ky":
                case "lo":
                case "ms":
                case "my":
                case "sah":
                case "su":
                case "th":
                case "tt":
                case "ug":
                case "vi":
                case "wo":
                case "zh-CN":
                case "zh-HK":
                case "zh-TW":
                    return n => 0;
                case "ach":
                case "ak":
                case "am":
                case "arn":
                case "br":
                case "fil":
                case "fr":
                case "gun":
                case "ln":
                case "mfe":
                case "mg":
                case "mi":
                case "oc":
                case "pt-BR":
                case "tg":
                case "ti":
                case "tr":
                case "uz":
                case "wa":
                    return n => (n > 1 ? 1 : 0);
                case "af":
                case "an":
                case "anp":
                case "as":
                case "ast":
                case "az":
                case "bg":
                case "bn":
                case "brx":
                case "ca":
                case "da":
                case "de":
                case "doi":
                case "el":
                case "en":
                case "eo":
                case "es":
                case "es-AR":
                case "et":
                case "eu":
                case "ff":
                case "fi":
                case "fo":
                case "fur":
                case "fy":
                case "gl":
                case "gu":
                case "ha":
                case "he":
                case "hi":
                case "hne":
                case "hu":
                case "hy":
                case "ia":
                case "it":
                case "kl":
                case "kn":
                case "ku":
                case "lb":
                case "mai":
                case "ml":
                case "mn":
                case "mni":
                case "mr":
                case "nah":
                case "nap":
                case "nb":
                case "ne":
                case "nl":
                case "nn":
                case "no":
                case "nso":
                case "or":
                case "pa":
                case "pap":
                case "pms":
                case "ps":
                case "pt":
                case "rm":
                case "rw":
                case "sat":
                case "sco":
                case "sd":
                case "se":
                case "si":
                case "so":
                case "son":
                case "sq":
                case "sv":
                case "sw":
                case "ta":
                case "te":
                case "tk":
                case "ur":
                case "yo":
                    return n => (n != 1 ? 1 : 0);
                case "is":
                    return n => (n % 10 != 1 || n % 100 == 11 ? 1 : 0);
                case "jv":
                    return n => (n != 0 ? 1 : 0);
                case "mk":
                    return n => (n == 1 || n % 10 == 1 ? 0 : 1);
                case "be":
                case "bs":
                case "hr":
                case "lt":
                    return n => (n % 10 == 1 && n % 100 != 11 ? 0 : n % 10 >= 2 && n % 10 <= 4 && (n % 100 < 10 || n % 100 >= 20) ? 1 : 2);
                case "cs":
                    return n => ((n == 1) ? 0 : (n >= 2 && n <= 4) ? 1 : 2);
                case "csb":
                case "pl":
                    return n => ((n == 1) ? 0 : n % 10 >= 2 && n % 10 <= 4 && (n % 100 < 10 || n % 100 >= 20) ? 1 : 2);
                case "lv":
                    return n => (n % 10 == 1 && n % 100 != 11 ? 0 : n != 0 ? 1 : 2);
                case "mnk":
                    return n => (n == 0 ? 0 : n == 1 ? 1 : 2);
                case "ro":
                    return n => (n == 1 ? 0 : (n == 0 || (n % 100 > 0 && n % 100 < 20)) ? 1 : 2);
                case "cy":
                    return n => ((n == 1) ? 0 : (n == 2) ? 1 : (n != 8 && n != 11) ? 2 : 3);
                case "gd":
                    return n => ((n == 1 || n == 11) ? 0 : (n == 2 || n == 12) ? 1 : (n > 2 && n < 20) ? 2 : 3);
                case "kw":
                    return n => ((n == 1) ? 0 : (n == 2) ? 1 : (n == 3) ? 2 : 3);
                case "mt":
                    return n => (n == 1 ? 0 : n == 0 || (n % 100 > 1 && n % 100 < 11) ? 1 : (n % 100 > 10 && n % 100 < 20) ? 2 : 3);
                case "sl":
                    return n => (n % 100 == 1 ? 1 : n % 100 == 2 ? 2 : n % 100 == 3 || n % 100 == 4 ? 3 : 0);
                case "ru":
                case "sr":
                case "uk":
                    return n => (n % 10 == 1 && n % 100 != 11 ? 0 : n % 10 >= 2 && n % 10 <= 4 && (n % 100 < 10 || n % 100 >= 20) ? 1 : 2);
                case "sk":
                    return n => ((n == 1) ? 0 : (n >= 2 && n <= 4) ? 1 : 2);
                case "ga":
                    return n => (n == 1 ? 0 : n == 2 ? 1 : (n > 2 && n < 7) ? 2 : (n > 6 && n < 11) ? 3 : 4);
                case "ar":
                    return n => (n == 0 ? 0 : n == 1 ? 1 : n == 2 ? 2 : n % 100 >= 3 && n % 100 <= 10 ? 3 : n % 100 >= 11 ? 4 : 5);
            }

            return null;
        }
    }
}