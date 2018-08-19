using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using OrchardCore.Facebook.Settings;

namespace OrchardCore.Facebook.Services
{
    public interface IFacebookCoreService
    {
        Task<FacebookCoreSettings> GetSettingsAsync();
        Task UpdateSettingsAsync(FacebookCoreSettings settings);
        Task<ImmutableArray<ValidationResult>> ValidateSettingsAsync(FacebookCoreSettings settings);
    }
}
