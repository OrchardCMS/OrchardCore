using System.Threading.Tasks;
using OrchardCore.Testing.Fakes;

namespace Microsoft.AspNetCore.Authorization;

public static class AuthorizationHandlerContextExtensions
{
    public static async Task SuccessAsync(this AuthorizationHandlerContext context, params string[] permissionNames)
    {
        var handler = new FakePermissionHandler(permissionNames);

        await handler.HandleAsync(context);
    }
}
