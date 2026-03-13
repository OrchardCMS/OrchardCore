namespace OrchardCore.Localization.Data;

public interface ISharedLocalizationDataProvider
{
    Task<IEnumerable<string>> GetDescriptorsAsync();
}
