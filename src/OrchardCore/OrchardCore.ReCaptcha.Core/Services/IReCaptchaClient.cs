using System.Threading.Tasks;

namespace OrchardCore.ReCaptcha.Core.Services
{
    public interface IReCaptchaClient
    {
        Task<bool> VerifyAsync(string responseToken, string secretKey);
    }
}