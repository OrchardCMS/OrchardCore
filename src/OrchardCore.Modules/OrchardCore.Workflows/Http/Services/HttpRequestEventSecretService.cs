using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using OrchardCore.DisplayManagement;
using OrchardCore.Secrets;
using OrchardCore.Workflows.Http.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Http.Services;

public class HttpRequestEventSecretService : IHttpRequestEventSecretService
{
    private readonly ISecretService _secretService;
    private readonly ISecurityTokenService _securityTokenService;
    private readonly ViewContextAccessor _viewContextAccessor;
    private readonly IUrlHelperFactory _urlHelperFactory;

    public HttpRequestEventSecretService(
        ISecretService secretService,
        ISecurityTokenService securityTokenService,
        ViewContextAccessor viewContextAccessor,
        IUrlHelperFactory urlHelperFactory)
    {
        _secretService = secretService;
        _securityTokenService = securityTokenService;
        _viewContextAccessor = viewContextAccessor;
        _urlHelperFactory = urlHelperFactory;
    }

    public async Task<string> GetUrlAsync(string secretName)
    {
        var secret = await _secretService.GetSecretAsync<HttpRequestEventSecret>(secretName);
        if (secret is null || secret.WorkflowTypeId is null || secret.ActivityId is null)
        {
            return null;
        }

        var urlHelper = _urlHelperFactory.GetUrlHelper(_viewContextAccessor.ViewContext);

        var token = _securityTokenService.CreateToken(
            new WorkflowPayload(
                secret.WorkflowTypeId,
                secret.ActivityId),
            TimeSpan.FromDays(1));

        return urlHelper.Action("Invoke", "HttpWorkflow", new { area = "OrchardCore.Workflows", token });
    }
}
