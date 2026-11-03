namespace OrchardCore.Tests.Modules.OrchardCore.Workflows.Variables;

/// <summary>
/// Returns the text it is given, formatted with its arguments.
/// </summary>
internal sealed class PassThroughStringLocalizer<T> : IStringLocalizer<T>
{
    public LocalizedString this[string name] => new(name, name);

    public LocalizedString this[string name, params object[] arguments] => new(name, string.Format(CultureInfo.InvariantCulture, name, arguments));

    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) => [];
}
