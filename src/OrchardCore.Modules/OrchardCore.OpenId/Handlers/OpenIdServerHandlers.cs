using System.Collections.Immutable;
using Microsoft.Extensions.DependencyInjection;
using OpenIddict.Server;
using YesSql;
using static OpenIddict.Server.OpenIddictServerEvents;

namespace OrchardCore.OpenId.Handlers;

internal static class OpenIdServerHandlers
{
    public static ImmutableArray<OpenIddictServerHandlerDescriptor> DefaultHandlers { get; } =
    [
        PersistStores.Descriptor
    ];

    /// <summary>
    /// Ensures that changes to any of the OpenId Stores are committed to the database before the response is
    /// transmitted to the client.
    /// </summary>
    public sealed class PersistStores(ISession session) : IOpenIddictServerHandler<ApplyAuthorizationResponseContext>
    {
        /// <summary>
        /// Gets the default descriptor definition assigned to this handler.
        /// </summary>
        public static OpenIddictServerHandlerDescriptor Descriptor { get; }
            = OpenIddictServerHandlerDescriptor.CreateBuilder<ApplyAuthorizationResponseContext>()
                .UseScopedHandler(static provider => new PersistStores(provider.GetRequiredService<ISession>()))
                .SetOrder(0)
                .SetType(OpenIddictServerHandlerType.Custom)
                .Build();

        /// <inheritdoc/>
        public async ValueTask HandleAsync(ApplyAuthorizationResponseContext context) => await session.SaveChangesAsync();
    }
}
