using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace OrchardCore.Tests.Apis;

public class ApiResourceVersionTests
{
    [Fact]
    public void ComputeApiETag_SameStateInAnotherPropertyOrder_ReturnsTheSameVersion()
    {
        var first = JsonNode.Parse("""{"a":1,"b":{"c":"x","d":[1,2]}}""");
        var second = JsonNode.Parse("""{"b":{"d":[1,2],"c":"x"},"a":1}""");

        Assert.Equal(ApiResourceVersionExtensions.ComputeApiETag(first), ApiResourceVersionExtensions.ComputeApiETag(second));
    }

    [Theory]
    [InlineData("""{"a":2}""")]
    [InlineData("""{"a":1,"b":1}""")]
    [InlineData("""{"a":[1]}""")]
    public void ComputeApiETag_AnotherState_ReturnsAnotherVersion(string other)
    {
        Assert.NotEqual(ApiResourceVersionExtensions.ComputeApiETag(JsonNode.Parse("""{"a":1}""")), ApiResourceVersionExtensions.ComputeApiETag(JsonNode.Parse(other)));
    }

    [Fact]
    public void ComputeApiETag_ArrayOrder_IsPartOfTheVersion()
    {
        Assert.NotEqual(ApiResourceVersionExtensions.ComputeApiETag(JsonNode.Parse("""[1,2]""")), ApiResourceVersionExtensions.ComputeApiETag(JsonNode.Parse("""[2,1]""")));
    }

    [Fact]
    public void ComputeApiETag_NullProperty_IsTheSameAsAMissingOne()
    {
        Assert.Equal(ApiResourceVersionExtensions.ComputeApiETag(new { A = 1, B = (string)null }), ApiResourceVersionExtensions.ComputeApiETag(new { A = 1 }));
        Assert.Equal(ApiResourceVersionExtensions.ComputeApiETag(JsonNode.Parse("""{"a":1,"b":null}""")), ApiResourceVersionExtensions.ComputeApiETag(JsonNode.Parse("""{"a":1}""")));
    }

    [Fact]
    public void ComputeApiETag_AnyState_ReturnsAQuotedStrongTag()
    {
        Assert.Matches("^\"[0-9a-f]{32}\"$", ApiResourceVersionExtensions.ComputeApiETag(new { A = 1 }));
    }

    [Fact]
    public void WithApiETag_AnyState_SetsTheHeaderAndReturnsTheState()
    {
        var context = new DefaultHttpContext();
        var state = new { A = 1 };

        Assert.Same(state, context.WithApiETag(state));
        Assert.Equal(ApiResourceVersionExtensions.ComputeApiETag(state), context.Response.Headers.ETag);
    }

    [Fact]
    public void ApiIfMatchProblem_NoHeader_ReturnsNull()
    {
        Assert.Null(new DefaultHttpContext().ApiIfMatchProblem(new { A = 1 }));
    }

    [Theory]
    [InlineData("current")]
    [InlineData("*")]
    [InlineData("\"other\", current")]
    public void ApiIfMatchProblem_MatchingHeader_ReturnsNull(string header)
    {
        var state = new { A = 1 };
        var context = new DefaultHttpContext();
        context.Request.Headers.IfMatch = header.Replace("current", ApiResourceVersionExtensions.ComputeApiETag(state), StringComparison.Ordinal);

        Assert.Null(context.ApiIfMatchProblem(state));
    }

    [Theory]
    [InlineData("\"stale\"")]
    [InlineData("W/current")]
    [InlineData("unquoted")]
    [InlineData("")]
    public void ApiIfMatchProblem_OtherHeader_ReturnsPreconditionFailedWithTheCurrentVersion(string header)
    {
        var state = new { A = 1 };
        var etag = ApiResourceVersionExtensions.ComputeApiETag(state);
        var context = CreateContext();
        context.Request.Headers.IfMatch = header.Replace("current", etag, StringComparison.Ordinal);

        var result = context.ApiIfMatchProblem(state);

        Assert.Equal(StatusCodes.Status412PreconditionFailed, Assert.IsAssignableFrom<IStatusCodeHttpResult>(result).StatusCode);
        Assert.Equal(etag, context.Response.Headers.ETag);
    }

    private static DefaultHttpContext CreateContext() => new()
    {
        RequestServices = new ServiceCollection()
            .AddSingleton(Mock.Of<IStringLocalizer<ProblemDetailsApiLocalization>>())
            .BuildServiceProvider(),
    };
}
