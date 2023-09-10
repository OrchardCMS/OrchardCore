using System;
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
}
