using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace OrchardCore.Localization.Data;

public class DataLocalizerFactory : IDataLocalizerFactory
{
    private readonly DataResourceManager _dataResourceManager;
    private readonly IOptionsMonitor<RequestLocalizationOptions> _requestLocalizationOptions;
    private readonly ILogger _logger;

    public DataLocalizerFactory(
        DataResourceManager dataResourceManager,
        IOptionsMonitor<RequestLocalizationOptions> requestLocalizationOptions,
        ILogger<DataLocalizerFactory> logger)
    {
        _dataResourceManager = dataResourceManager;
        _requestLocalizationOptions = requestLocalizationOptions;
        _logger = logger;
    }

    public IDataLocalizer Create()
        => new DataLocalizer(_dataResourceManager, _requestLocalizationOptions.CurrentValue.FallBackToParentUICultures, _logger);
}
