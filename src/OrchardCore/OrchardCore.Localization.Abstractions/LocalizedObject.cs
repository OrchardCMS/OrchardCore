using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace OrchardCore.Localization
{
    public class LocalizedObject : Dictionary<string, string>
    {
        private static readonly string DefaultCulture = String.Empty;

        public LocalizedObject()
        {

        }

        public LocalizedObject(string value)
        {
            if (!ContainsKey(DefaultCulture))
            {
                Add(DefaultCulture, value);
            }
            else
            {
                this[DefaultCulture] = value;
            }
        }

        public static implicit operator string(LocalizedObject localizedObject) => localizedObject?.ToString();

        public string Default => Values.First();

        public string GetValueOrDefault(string key) => TryGetValue(key, out var value) ? value : null;

        public override string ToString() => GetValueOrDefault(CultureInfo.CurrentUICulture.Name);
    }
}