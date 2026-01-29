using System.ComponentModel.DataAnnotations;
using OrchardCore.ReverseProxy.Settings;

namespace OrchardCore.ReverseProxy.Services;

public interface IReverseProxyService
{
    Task<ReverseProxySettings> GetSettingsAsync();
}
