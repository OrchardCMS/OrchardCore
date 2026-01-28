using System.ComponentModel.DataAnnotations;
using OrchardCore.ReverseProxy.Settings;

namespace OrchardCore.ReverseProxy.Services;

public interface IReverseProxyService
{
    Task<ReverseProxySettings> GetSettingsAsync();

    Task<ReverseProxySettings> LoadSettingsAsync();

    Task UpdateSettingsAsync(ReverseProxySettings settings);

    IEnumerable<ValidationResult> ValidateSettings(ReverseProxySettings settings);
}
