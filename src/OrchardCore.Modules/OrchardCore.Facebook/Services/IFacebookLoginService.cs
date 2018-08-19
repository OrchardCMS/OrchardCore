using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using OrchardCore.Facebook.Settings;

namespace OrchardCore.Facebook.Services
{
    public interface IFacebookLoginService
    {
        Task<FacebookLoginSettings> GetSettingsAsync();
        Task UpdateSettingsAsync(FacebookLoginSettings settings);
        Task<ImmutableArray<ValidationResult>> ValidateSettingsAsync(FacebookLoginSettings settings);
    }
}
