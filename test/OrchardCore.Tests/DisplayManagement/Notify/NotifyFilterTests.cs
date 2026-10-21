using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Net.Http.Headers;
using OrchardCore.DisplayManagement.Notify;

namespace OrchardCore.Tests.DisplayManagement.Notify;

public class NotifyFilterTests
{
    [Theory]
    [InlineData("https", true)]
    [InlineData("http", false)]
    public async Task OnActionExecuted_Redirect_SetsSecureCookieOnHttps(string scheme, bool expectedSecure)
    {
        var notifier = new Notifier(NullLogger<Notifier>.Instance);
        await notifier.SuccessAsync(new LocalizedHtmlString("Saved", "Saved"));

        var filter = CreateFilter(notifier);
        var httpContext = CreateHttpContext(scheme);
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());

        filter.OnActionExecuting(new ActionExecutingContext(actionContext, [], new Dictionary<string, object>(), controller: null));
        filter.OnActionExecuted(new ActionExecutedContext(actionContext, [], controller: null)
        {
            Result = new RedirectResult("/"),
        });

        var setCookie = GetNotifySetCookie(httpContext);

        Assert.True(setCookie.HttpOnly);
        Assert.Equal(expectedSecure, setCookie.Secure);
    }

    [Theory]
    [InlineData("https", true)]
    [InlineData("http", false)]
    public async Task OnResultExecutionAsync_InvalidCookie_DeletesSecureCookieOnHttps(string scheme, bool expectedSecure)
    {
        var filter = CreateFilter(new Notifier(NullLogger<Notifier>.Instance));
        var httpContext = CreateHttpContext(scheme);
        httpContext.Request.Headers.Cookie = $"{NotifyFilter.CookiePrefix}=invalid";
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());

        filter.OnActionExecuting(new ActionExecutingContext(actionContext, [], new Dictionary<string, object>(), controller: null));

        var resultContext = new ResultExecutingContext(actionContext, [], new RedirectResult("/"), controller: null);
        await filter.OnResultExecutionAsync(resultContext, () => Task.FromResult<ResultExecutedContext>(null));

        var setCookie = GetNotifySetCookie(httpContext);

        Assert.Equal(DateTimeOffset.UnixEpoch, setCookie.Expires);
        Assert.True(setCookie.HttpOnly);
        Assert.Equal(expectedSecure, setCookie.Secure);
    }

    private static NotifyFilter CreateFilter(INotifier notifier)
    {
        var options = new NotifyJsonSerializerOptions();
        options.SerializerOptions.Converters.Add(new NotifyEntryConverter(HtmlEncoder.Default));

        return new NotifyFilter(
            notifier,
            shapeFactory: null,
            layoutAccessor: null,
            new EphemeralDataProtectionProvider(),
            HtmlEncoder.Default,
            Options.Create(options),
            NullLogger<NotifyFilter>.Instance);
    }

    private static DefaultHttpContext CreateHttpContext(string scheme)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Scheme = scheme;
        httpContext.Request.Host = new HostString("localhost");

        return httpContext;
    }

    private static SetCookieHeaderValue GetNotifySetCookie(HttpContext httpContext)
        => SetCookieHeaderValue.Parse(Assert.Single(httpContext.Response.Headers.SetCookie, cookie => cookie.StartsWith($"{NotifyFilter.CookiePrefix}=", StringComparison.Ordinal)));
}
