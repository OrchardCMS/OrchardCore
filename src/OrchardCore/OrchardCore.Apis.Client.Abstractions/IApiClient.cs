namespace OrchardCore.Apis.Client.Abstractions
{
    public interface IApiClient
    {
        ITenantResource Tenants { get; }

        IContentResource Content { get; }
    }
}
