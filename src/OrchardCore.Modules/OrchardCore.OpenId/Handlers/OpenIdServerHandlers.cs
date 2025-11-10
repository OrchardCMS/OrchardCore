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
    /// Ensures that changes to any of the OpenId Stores are committed to the database before generated tokens
    /// are transmitted to the client.
    /// </summary>
    public sealed class PersistStores(ISession session) : IOpenIddictServerHandler<GenerateTokenContext>
    {
        /// <summary>
        /// Gets the default descriptor definition assigned to this handler.
        /// </summary>
        public static OpenIddictServerHandlerDescriptor Descriptor { get; }
            = OpenIddictServerHandlerDescriptor.CreateBuilder<GenerateTokenContext>()
                .UseScopedHandler(static provider => new PersistStores(provider.GetRequiredService<ISession>()))
                .SetOrder(int.MaxValue)
                .SetType(OpenIddictServerHandlerType.Custom)
                .Build();

        /// <inheritdoc/>
        public async ValueTask HandleAsync(GenerateTokenContext context)
        {
            if (context.IsRejected)
            {
                return;
            }
            await session.SaveChangesAsync();
        }
    }
}
