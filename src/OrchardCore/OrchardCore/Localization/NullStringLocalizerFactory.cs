using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Localization;

namespace OrchardCore.Localization
{
    /// Represents a null <see cref="IStringLocalizerFactory"/> which is used by default when the localization module is disabled.
    /// <remarks>
    /// A LocalizedString is not encoded, so it can contain the formatted string
    /// including the argument values.
    /// </remarks>
    public class NullStringLocalizerFactory : IStringLocalizerFactory
    {
        /// <inheritdocs />
        public IStringLocalizer Create(Type resourceSource) => NullLocalizer.Instance;

        /// <inheritdocs />
        public IStringLocalizer Create(string baseName, string location) => NullLocalizer.Instance;

        internal class NullLocalizer : IStringLocalizer
        {
            private static readonly PluralizationRuleDelegate _defaultPluralRule = n => (n == 1) ? 0 : 1;

            public static NullLocalizer Instance { get; } = new NullLocalizer();

            public LocalizedString this[string name] => new(name, name, false);

            public LocalizedString this[string name, params object[] arguments]
            {
                get
                {
                    var translation = name;

                    if (arguments.Length == 1 && arguments[0] is PluralizationArgument pluralArgument)
                    {
                        translation = pluralArgument.Forms[_defaultPluralRule(pluralArgument.Count)];

                        arguments = new object[pluralArgument.Arguments.Length + 1];
                        arguments[0] = pluralArgument.Count;
                        Array.Copy(pluralArgument.Arguments, 0, arguments, 1, pluralArgument.Arguments.Length);
                    }

                    translation = String.Format(translation, arguments);

                    return new LocalizedString(name, translation, false);
                }
            }

            public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
                => Enumerable.Empty<LocalizedString>();

            public LocalizedString GetString(string name) => this[name];

            public LocalizedString GetString(string name, params object[] arguments) => this[name, arguments];
        }
    }
}
