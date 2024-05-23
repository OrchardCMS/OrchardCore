using System.Threading.Tasks;
using OpenIddict.Client;
using OrchardCore.OpenId.Settings;
using static OpenIddict.Client.OpenIddictClientEvents;

namespace OrchardCore.OpenId.Services.Handlers;

public class OpenIdClientCustomParametersEventHandler : IOpenIddictClientHandler<ProcessChallengeContext>
{
    public static OpenIddictClientHandlerDescriptor Descriptor { get; }
        = OpenIddictClientHandlerDescriptor.CreateBuilder<ProcessChallengeContext>()
            .UseSingletonHandler<OpenIdClientCustomParametersEventHandler>()
            .SetOrder(OpenIddictClientHandlers.AttachCustomChallengeParameters.Descriptor.Order - 1)
            .SetType(OpenIddictClientHandlerType.BuiltIn)
            .Build();

    public ValueTask HandleAsync(ProcessChallengeContext context)
    {
        // If the client registration is managed by Orchard, attach the custom parameters set by the user.
        if (context.Registration.Properties.TryGetValue(nameof(OpenIdClientSettings), out var value) &&
            value is OpenIdClientSettings settings && settings.Parameters is { Length: > 0 } parameters)
        {
            foreach (var parameter in parameters)
            {
                context.Parameters[parameter.Name] = parameter.Value;
            }
        }

        return default;
    }
}
