namespace Microsoft.Extensions.Localization
{
    public interface IPluralStringLocalizer
    {
        /// <summary>
        /// Gets the string resource with the given name and formatted with the supplied arguments.
        /// </summary>
        /// <param name="localizer"></param>
        /// <param name="name">The name of the string resource.</param>
        /// <param name="pluralName">The name of the plural string resource.</param>
        /// <param name="count">The number of items represented in the plural form.</param>
        /// <param name="arguments">The values to format the string with.</param>
        /// <returns>The formatted string resource as a <see cref="LocalizedString"/>.</returns>
        LocalizedString this[string name, string pluralName, int count, params object[] arguments] { get; }
    }

    public interface IPluralStringLocalizer<T> : IPluralStringLocalizer
    {
    }
}
