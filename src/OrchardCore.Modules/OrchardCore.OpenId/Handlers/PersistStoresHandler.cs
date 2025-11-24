using Microsoft.Extensions.DependencyInjection;
using OpenIddict.Server;
using YesSql;
using static OpenIddict.Server.OpenIddictServerEvents;
using static OpenIddict.Server.OpenIddictServerHandlers.Authentication;

namespace OrchardCore.OpenId.Handlers;

/// <summary>
/// Ensures that changes to any of the OpenId Stores are committed to the database before generated tokens
/// are transmitted to the client.
/// </summary>
public sealed class PersistStoresHandler(ISession session) : IOpenIddictServerHandler<ProcessSignInContext>
{
    /// <summary>
    /// Gets the default descriptor definition assigned to this handler.
    /// </summary>
    public static OpenIddictServerHandlerDescriptor Descriptor { get; }
        = OpenIddictServerHandlerDescriptor.CreateBuilder<ProcessSignInContext>()
            .UseScopedHandler(static provider => new PersistStoresHandler(provider.GetRequiredService<ISession>()))
            .SetOrder(ApplyAuthorizationResponse<ProcessSignInContext>.Descriptor.Order - 1_000)
            .SetType(OpenIddictServerHandlerType.Custom)
            .Build();

    /// <inheritdoc/>
    public async ValueTask HandleAsync(ProcessSignInContext context)
    {
        if (context.IsRejected)
        {
            return;
        }
        // Persist changes made to the OpenId stores by forcing a database commit on the YesSql session.
        await session.SaveChangesAsync();
    }
}
