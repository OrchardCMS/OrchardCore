using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace OrchardCore.Localization.Data;

public class DataLocalizerFactory : IDataLocalizerFactory
{
    private readonly DataResourceManager _dataResourceManager;
    private readonly bool _fallBackToParentCulture;
    private readonly ILogger _logger;

    public DataLocalizerFactory(
        DataResourceManager dataResourceManager,
        IOptions<RequestLocalizationOptions> requestLocalizationOptions,
        ILogger<DataLocalizerFactory> logger)
    {
        _dataResourceManager = dataResourceManager;
        _fallBackToParentCulture = requestLocalizationOptions.Value.FallBackToParentUICultures;
        _logger = logger;
    }

    public IDataLocalizer Create() => new DataLocalizer(_dataResourceManager, _fallBackToParentCulture, _logger);
}
