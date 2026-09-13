using System.Net;
using OrchardCore.Deployment.Remote.Models;
using OrchardCore.Deployment.Remote.ViewModels;

namespace OrchardCore.Deployment.Remote.Services;

/// <summary>Sends existing deployment archives through the shared remote import protocol.</summary>
public sealed class RemoteDeploymentSender
{
    /// <summary>Gets the HTTP client name with redirects disabled to protect deployment keys.</summary>
    public const string HttpClientName = "OrchardCore.RemoteDeployment";
    private readonly IHttpClientFactory _clients;

    /// <summary>Creates a sender using the configured tenant HTTP client factory.</summary>
    public RemoteDeploymentSender(IHttpClientFactory clients) => _clients = clients;

    /// <summary>Sends an archive once; a failure or cancellation may occur after the target started importing.</summary>
    public async Task<HttpStatusCode> SendAsync(RemoteInstance destination, Stream archive, string fileName, CancellationToken cancellationToken = default)
    {
        if (!RemoteDeploymentValidation.IsSafeUrl(destination.Url))
        {
            throw new InvalidOperationException("The remote deployment URL does not meet transport requirements.");
        }
        using var content = new MultipartFormDataContent();
        content.Add(new StreamContent(archive), nameof(ImportViewModel.Content), fileName);
        content.Add(new StringContent(destination.ClientName), nameof(ImportViewModel.ClientName));
        content.Add(new StringContent(destination.ApiKey), nameof(ImportViewModel.ApiKey));
        using var response = await _clients.CreateClient(HttpClientName).PostAsync(destination.Url, content, cancellationToken);
        return response.StatusCode;
    }
}
