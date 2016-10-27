using System;
using Microsoft.AspNetCore.Mvc.Localization;
using Orchard.Utility;

namespace Orchard.Modules.Extensions {
    public static class StringExtensions {
        public static string AsFeatureId(this string text, Func<string, LocalizedHtmlString> localize) {
            return string.IsNullOrEmpty(text)
                       ? ""
                       : string.Format(localize("{0} feature").ToString(), text).HtmlClassify();
        }
    }
}