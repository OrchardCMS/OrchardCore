namespace OrchardCore.Localization.Data;

internal sealed class SharedLocalizationDataProvider : ILocalizationDataProvider
{
    private static readonly string _context = "Shared";

    private readonly HashSet<string> _names;

    public SharedLocalizationDataProvider(HashSet<string> names) => _names = names;

    public Task<IEnumerable<DataLocalizedString>> GetDescriptorsAsync()
        => Task.FromResult(_names.Select(n => new DataLocalizedString(_context, n, string.Empty)));
}
