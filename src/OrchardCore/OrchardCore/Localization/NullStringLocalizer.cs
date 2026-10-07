using Microsoft.Extensions.Localization;

namespace OrchardCore.Localization;

/// <summary>
/// Minimalistic localizer that does nothing.
/// </summary>
public class NullStringLocalizer : IStringLocalizer
{
    private static readonly PluralizationRuleDelegate s_defaultPluralRule = n => (n == 1) ? 0 : 1;

    /// <summary>
    /// Returns the shared instance of <see cref="NullStringLocalizer"/>.
    /// </summary>
    public static NullStringLocalizer Instance { get; } = new();

    /// <inheritdoc/>
    public LocalizedString this[string name] => new(name, name, false);

    /// <inheritdoc/>
    public LocalizedString this[string name, params object[] arguments]
    {
        get
        {
            var translation = name;

            if (arguments is [PluralizationArgument pluralArgument])
            {
                translation = pluralArgument.Forms[s_defaultPluralRule(pluralArgument.Count)];

                arguments = new object[pluralArgument.Arguments.Length + 1];
                arguments[0] = pluralArgument.Count;
                Array.Copy(pluralArgument.Arguments, 0, arguments, 1, pluralArgument.Arguments.Length);
            }

            translation = string.Format(translation, arguments);

            return new LocalizedString(name, translation, false);
        }
    }

    /// <inheritdoc/>
    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
        => Enumerable.Empty<LocalizedString>();

    /// <inheritdoc/>
    public LocalizedString GetString(string name) => this[name];

    /// <inheritdoc/>
    public LocalizedString GetString(string name, params object[] arguments) => this[name, arguments];
}
