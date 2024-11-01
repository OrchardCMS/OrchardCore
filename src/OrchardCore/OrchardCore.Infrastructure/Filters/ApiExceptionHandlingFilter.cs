using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.Json;
using Json.More;
namespace OrchardCore.Infrastructure.Filters;

public class ApiExceptionHandlingFilter : IAsyncExceptionFilter
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public ApiExceptionHandlingFilter()
    {

    }

    public async Task OnExceptionAsync(ExceptionContext context)
    {
        var response = new ProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
            Title = "An error occurred while processing your request.",
            Status = StatusCodes.Status500InternalServerError,
            Detail = context.Exception.Message,
            Instance = context.HttpContext.Request.Path
        };

        context.HttpContext.Response.StatusCode = response.Status.Value;
        context.Result = new JsonResult(response, _jsonOptions);
        context.ExceptionHandled = true;
    }
}

