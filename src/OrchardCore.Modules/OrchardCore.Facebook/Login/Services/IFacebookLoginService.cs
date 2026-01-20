using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using OrchardCore.Facebook.Login.Settings;

namespace OrchardCore.Facebook.Login.Services
{
    public interface IFacebookLoginService
    {
        Task<FacebookLoginSettings> GetSettingsAsync();
        Task<FacebookLoginSettings> LoadSettingsAsync();
        Task UpdateSettingsAsync(FacebookLoginSettings settings);
        Task<IEnumerable<ValidationResult>> ValidateSettingsAsync(FacebookLoginSettings settings);
    }
}
