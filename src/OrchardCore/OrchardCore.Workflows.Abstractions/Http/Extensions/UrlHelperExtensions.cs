using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Workflows.Http.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Http;

public static class UrlHelperExtensions
{
    public static string GenerateHttpRequestEventUrl(this IUrlHelper urlHelper, WorkflowPayload payload)
    {
        var securityTokenService = urlHelper.ActionContext.HttpContext.RequestServices.GetService<ISecurityTokenService>();
        if (securityTokenService is null)
        {
            return null;
        }

        var token = securityTokenService.CreateToken(payload, TimeSpan.FromDays(2));

        return urlHelper.Action("Invoke", "HttpWorkflow", new { area = "OrchardCore.Workflows", token });
    }

    public static async Task<string> GenerateHttpRequestEventUrlAsync(this IUrlHelper urlHelper, WorkflowPayload payload)
    {
        var secretTokenService = urlHelper.ActionContext.HttpContext.RequestServices.GetService<ISecretTokenService>();
        if (secretTokenService is null)
        {
            return null;
        }

        var token = await secretTokenService.CreateTokenAsync(payload, TimeSpan.FromDays(2));

        return urlHelper.Action("Invoke", "HttpWorkflow", new { area = "OrchardCore.Workflows", token });
    }
}
