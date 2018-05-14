using System.Threading.Tasks;

namespace OrchardCore.Forms.Services
{
    public interface IReCaptchaClient
    {
        Task<bool> VerifyAsync(string responseToken);
    }
}
