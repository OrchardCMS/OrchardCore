using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace OrchardCore.Tests.Apis;

public class ApiProblemCodesTests
{
    [Theory]
    [InlineData(StatusCodes.Status400BadRequest, "validation")]
    [InlineData(StatusCodes.Status401Unauthorized, "unauthorized")]
    [InlineData(StatusCodes.Status403Forbidden, "forbidden")]
    [InlineData(StatusCodes.Status404NotFound, "not_found")]
    [InlineData(StatusCodes.Status409Conflict, "conflict")]
    [InlineData(StatusCodes.Status412PreconditionFailed, "precondition_failed")]
    [InlineData(StatusCodes.Status429TooManyRequests, "rate_limited")]
    public void Apply_SharedFailureKind_AddsItsCode(int statusCode, string code)
    {
        var problem = new ProblemDetails { Status = statusCode };

        ApiProblemCodes.Apply(new ProblemDetailsContext { HttpContext = new DefaultHttpContext(), ProblemDetails = problem });

        Assert.Equal(code, problem.Extensions[ApiProblemCodes.ExtensionName]);
    }

    [Fact]
    public void Apply_OtherStatusCode_AddsNoCode()
    {
        var problem = new ProblemDetails { Status = StatusCodes.Status500InternalServerError };

        ApiProblemCodes.Apply(new ProblemDetailsContext { HttpContext = new DefaultHttpContext(), ProblemDetails = problem });

        Assert.False(problem.Extensions.ContainsKey(ApiProblemCodes.ExtensionName));
    }

    [Fact]
    public void Apply_CodeChosenByTheEndpoint_IsKept()
    {
        var problem = new ProblemDetails { Status = StatusCodes.Status409Conflict };
        problem.Extensions[ApiProblemCodes.ExtensionName] = "layer_referenced";

        ApiProblemCodes.Apply(new ProblemDetailsContext { HttpContext = new DefaultHttpContext(), ProblemDetails = problem });

        Assert.Equal("layer_referenced", problem.Extensions[ApiProblemCodes.ExtensionName]);
    }

    [Fact]
    public void Apply_NoStatusOnTheProblem_UsesTheOneOfTheResponse()
    {
        var context = new DefaultHttpContext();
        context.Response.StatusCode = StatusCodes.Status404NotFound;
        var problem = new ProblemDetails();

        ApiProblemCodes.Apply(new ProblemDetailsContext { HttpContext = context, ProblemDetails = problem });

        Assert.Equal("not_found", problem.Extensions[ApiProblemCodes.ExtensionName]);
    }
}
