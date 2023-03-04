using System.Security.Principal;

namespace OrchardCore.Testing.Stubs
{
    public class IdentityStub : IIdentity
    {
        public string AuthenticationType => "Testing";

        public bool IsAuthenticated => true;

        public string Name => "OrchardCore";
    }
}
