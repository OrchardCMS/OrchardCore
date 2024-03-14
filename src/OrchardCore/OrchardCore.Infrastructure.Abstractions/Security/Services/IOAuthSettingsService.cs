using System.Threading.Tasks;

namespace OrchardCore.Security.Services;

public interface IOAuthSettingsService<TAuthenticationSettings> where TAuthenticationSettings : OAuthSettings, new()
{
    public Task<TAuthenticationSettings> GetSettingsAsync();

    public Task UpdateSettingsAsync(TAuthenticationSettings settings);
}
