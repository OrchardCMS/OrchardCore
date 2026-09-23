using System.Net;

namespace OrchardCore.Workflows.Http.Services;

internal interface IHttpRequestDestinationValidator
{
    bool IsAllowed(string host, IPAddress address);
}
