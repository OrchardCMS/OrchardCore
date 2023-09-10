using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Secrets;
using OrchardCore.Workflows.Http.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Http;

public static class UrlHelperExtensions
{
    public static async Task<string> GetHttpRequestEventUrlAsync(this IUrlHelper urlHelper, string secretName)
    {
        var services = urlHelper.ActionContext?.HttpContext.RequestServices;
        if (services is null)
        {
            return null;
        }

        var secretService = services.GetService<ISecretService>();
        if (secretService is null)
        {
            return null;
        }

        var securityTokenService = services.GetService<ISecurityTokenService>();
        if (securityTokenService is null)
        {
            return null;
        }

        var secret = await secretService.GetSecretAsync<HttpRequestEventSecret>(secretName);
        if (secret is null || secret.WorkflowTypeId is null || secret.ActivityId is null)
        {
            return null;
        }

        var token = securityTokenService.CreateToken(new WorkflowPayload(
            secret.WorkflowTypeId,
            secret.ActivityId),
            TimeSpan.FromDays(1));

        return urlHelper.Action("Invoke", "HttpWorkflow", new { area = "OrchardCore.Workflows", token });
    }
}
