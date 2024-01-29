namespace OrchardCore.Email.Services;

public interface IEmailDeliveryServiceResolver
{
    IEmailDeliveryService Resolve(string name);
}
