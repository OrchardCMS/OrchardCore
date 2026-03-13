namespace OrchardCore.Localization.Data;

public sealed class SharedLocalizationDataProvider : ILocalizationDataProvider
{
    private const string Context = "Shared";

    private readonly IEnumerable<ISharedLocalizationDataProvider> _sharedLocalizationDataProviders;

    private IEnumerable<DataLocalizedString> _descriptors;

    public SharedLocalizationDataProvider(IEnumerable<ISharedLocalizationDataProvider> sharedLocalizationDataProviders)
    {
        _sharedLocalizationDataProviders = sharedLocalizationDataProviders;
    }

    public async Task<IEnumerable<DataLocalizedString>> GetDescriptorsAsync()
    {
        if (_descriptors is null)
        {
            var descriptors = new HashSet<string>();

            foreach (var provider in _sharedLocalizationDataProviders)
            {
                var providerDscriptors = await provider.GetDescriptorsAsync();

                foreach (var descriptor in providerDscriptors)
                {
                    descriptors.Add(descriptor);
                }
            }

            _descriptors = descriptors.Select(d => new DataLocalizedString(Context, d, string.Empty));
        }

        return _descriptors;
    }
}
