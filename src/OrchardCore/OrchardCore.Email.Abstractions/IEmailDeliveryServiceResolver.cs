using OrchardCore.Email.Services;

namespace OrchardCore.Email;

public interface IEmailDeliveryServiceResolver
{
    IEmailDeliveryService Resolve(string name);
}
