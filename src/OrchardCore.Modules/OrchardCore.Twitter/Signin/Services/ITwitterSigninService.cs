using System.Threading.Tasks;
using OrchardCore.Twitter.Signin.Settings;

namespace OrchardCore.Twitter.Signin.Services
{
    public interface ITwitterSigninService
    {
        Task<TwitterSigninSettings> GetSettingsAsync();
        Task UpdateSettingsAsync(TwitterSigninSettings settings);
    }
}
