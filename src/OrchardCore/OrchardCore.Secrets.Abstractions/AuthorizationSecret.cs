using System.Threading.Tasks;

namespace OrchardCore.Secrets
{
    public class AuthorizationSecret : Secret
    {
        public string AuthenticationString { get; set; }
    }
}
