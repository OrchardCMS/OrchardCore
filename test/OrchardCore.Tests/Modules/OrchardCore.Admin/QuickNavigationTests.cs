using System.Globalization;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging.Abstractions;
using OrchardCore.Admin.Controllers;
using OrchardCore.Admin.Models;
using OrchardCore.Admin.QuickNavigation;
using OrchardCore.Entities;
using OrchardCore.Environment.Shell;
using OrchardCore.Navigation;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;

namespace OrchardCore.Tests.Modules.OrchardCore.Admin;

public class QuickNavigationTests
{
    [Fact]
    public void Replace_EntriesChanged_PreservesSnapshotsAndSourceIsolation()
    {
        var index = new QuickNavigationIndex();
        var ids = new[] { "one", "one", "two" };
        index.Replace("first", ids);
        index.Replace("second", ["other"]);
        var snapshot = index.GetEntryIds("first");
        ids[0] = "mutated";

        index.Replace("first", ["new"]);

        Assert.Equal(["one", "two"], snapshot);
        Assert.Equal(["new"], index.GetEntryIds("first"));
        Assert.Equal(["other"], index.GetEntryIds("second"));

        index.Replace("first", []);

        Assert.Empty(index.GetEntryIds("first"));
        Assert.Empty(index.GetEntryIds("missing"));
    }

    [Fact]
    public void Replace_InvalidEntry_DoesNotReplaceExistingSnapshot()
    {
        var index = new QuickNavigationIndex();
        index.Replace("source", ["original"]);

        Assert.Throws<ArgumentException>(() => index.Replace("source", ["valid", " "]));
        Assert.Equal(["original"], index.GetEntryIds("source"));
    }

    [Fact]
    public async Task Index_RegisteredSources_RoutesCachedAndRequestEntriesAndOmitsUnauthorizedEntries()
    {
        var index = new QuickNavigationIndex();
        index.Replace("first", ["cached", "hidden"]);
        index.Replace("second", ["cached"]);
        index.Replace("disabled-feature", ["never-displayed"]);

        var first = new TestSource("first", ["live", "cached"]);
        var second = new TestSource("second");
        var controller = CreateController(index, [first, second]);

        var results = ReadResults(await controller.Index());

        Assert.Equal(["cached", "live", "cached"], results.Select(result => result.Id));
        Assert.Equal(["first", "first", "second"], results.Select(result => result.Source));
        Assert.Equal(["cached", "hidden", "live"], first.Displayed);
        Assert.Equal(["cached"], second.Displayed);
        Assert.All(results, result =>
        {
            Assert.Equal("/tenant/manage/" + result.Id, result.Href);
            Assert.Equal(["en"], result.Path);
        });
        Assert.Equal("no-store", controller.Response.Headers.CacheControl);
        Assert.False(controller.Response.Headers.ContainsKey("ETag"));
    }

    [Theory]
    [InlineData("\"previous-document\"")]
    [InlineData("W/\"previous-document\"")]
    [InlineData("*")]
    public async Task Index_ConditionalRequest_ExecutesSourcesAndReturnsFullDocument(string ifNoneMatch)
    {
        var index = new QuickNavigationIndex();
        index.Replace("source", ["one"]);
        var source = new TestSource("source");
        var first = CreateController(index, [source]);
        var original = Assert.IsType<FileContentResult>(await first.Index());
        var second = CreateController(index, [source]);
        second.Request.Headers.IfNoneMatch = ifNoneMatch;

        var response = Assert.IsType<FileContentResult>(await second.Index());

        Assert.Equal(original.FileContents, response.FileContents);
        Assert.Equal("one", Assert.Single(ReadResults(response)).Id);
        Assert.Equal(2, source.GetEntryIdsCalls);
        Assert.Equal(2, source.Displayed.Count);
        Assert.Equal("no-store", second.Response.Headers.CacheControl);
        Assert.False(second.Response.Headers.ContainsKey("ETag"));
    }

    [Fact]
    public async Task Index_SourcePopulatesIndexLazily_IncludesEntriesInFirstResponse()
    {
        var index = new QuickNavigationIndex();
        var source = new TestSource("lazy", populateIndex: () => index.Replace("lazy", ["one"]));
        var controller = CreateController(index, [source]);

        var result = Assert.Single(ReadResults(await controller.Index()));

        Assert.Equal("lazy", result.Source);
        Assert.Equal("one", result.Id);
    }

    [Fact]
    public async Task Index_UpdatedIndex_ChangesDocument()
    {
        var index = new QuickNavigationIndex();
        index.Replace("source", ["one"]);
        var first = CreateController(index, [new TestSource("source")]);
        Assert.Equal("one", Assert.Single(ReadResults(await first.Index())).Id);
        index.Replace("source", ["two"]);
        var second = CreateController(index, [new TestSource("source")]);
        var result = Assert.Single(ReadResults(await second.Index()));

        Assert.Equal("two", result.Id);
    }

    [Fact]
    public async Task Index_DifferentCulture_RendersSameIndexedIdentifierInCurrentCulture()
    {
        var index = new QuickNavigationIndex();
        index.Replace("source", ["one"]);
        var originalCulture = CultureInfo.CurrentUICulture;

        try
        {
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("en");
            var first = CreateController(index, [new TestSource("source", useCurrentCulture: true)]);
            var english = Assert.Single(ReadResults(await first.Index()));
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("fr");
            var second = CreateController(index, [new TestSource("source", useCurrentCulture: true)]);
            var french = Assert.Single(ReadResults(await second.Index()));

            Assert.Equal(english.Id, french.Id);
            Assert.Equal("en:one", english.Title);
            Assert.Equal("fr:one", french.Title);
            Assert.Equal(["fr"], french.Path);
        }
        finally
        {
            CultureInfo.CurrentUICulture = originalCulture;
        }
    }

    [Fact]
    public async Task Index_AccessRevoked_DoesNotReuseAuthorizedDocument()
    {
        var index = new QuickNavigationIndex();
        index.Replace("source", ["one"]);
        var first = CreateController(index, [new TestSource("source")]);
        await first.Index();
        var second = CreateController(index, [new TestSource("source")]);
        second.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity());
        Assert.Empty(ReadResults(await second.Index()));
    }

    [Fact]
    public async Task Index_DisabledSetting_DoesNotInvokeSources()
    {
        var source = new TestSource("source", ["one"]);
        var controller = CreateController(new QuickNavigationIndex(), [source], enabled: false);
        controller.Request.Headers.IfNoneMatch = "*";

        Assert.IsType<NotFoundResult>(await controller.Index());
        Assert.Empty(source.Displayed);
        Assert.Equal("no-store", controller.Response.Headers.CacheControl);
    }

    [Fact]
    public async Task AdminMenuSource_LocalizedMenu_PreservesOrderBreadcrumbsTargetsAndNeutralIds()
    {
        var manager = new Mock<INavigationManager>();
        using var services = new ServiceCollection().BuildServiceProvider();
        var httpContext = new DefaultHttpContext { RequestServices = services };
        httpContext.Request.RouteValues = new RouteValueDictionary { ["area"] = "OrchardCore.Admin" };
        var actionDescriptor = new ActionDescriptor();
        httpContext.SetEndpoint(new Endpoint(null, new EndpointMetadataCollection(actionDescriptor), "Quick navigation"));
        var culture = "en";
        manager.Setup(manager => manager.BuildMenuAsync(NavigationConstants.AdminId,
            It.Is<ActionContext>(context => context.HttpContext == httpContext
                && context.ActionDescriptor == actionDescriptor
                && (string)context.RouteData.Values["area"] == "OrchardCore.Admin")))
            .ReturnsAsync(() =>
            [
                new MenuItem
                {
                    Text = new LocalizedString("Tools", culture == "en" ? "Tools" : "Outils"),
                    Href = "#",
                    Items =
                    [
                        new MenuItem { Text = new LocalizedString("Later", "Later"), Href = "/tenant/manage/later", Position = "10" },
                        new MenuItem
                        {
                            Text = new LocalizedString("Features", culture == "en" ? "Features" : "Fonctionnalites"),
                            Href = "/tenant/manage/features",
                            Position = "2",
                            Target = "_blank",
                        },
                    ],
                },
            ]);
        var source = new AdminMenuItemNavigationSource(manager.Object,
            Mock.Of<IHttpContextAccessor>(accessor => accessor.HttpContext == httpContext));
        var ids = (await source.GetEntryIdsAsync()).ToArray();
        var result = await source.DisplayAsync(ids[0]);

        Assert.Equal(2, ids.Length);
        Assert.Equal("Features", result.Title);
        Assert.Equal(["Tools"], result.Path);
        Assert.Equal("/tenant/manage/features", result.Href);
        Assert.Equal("_blank", result.Target);
        Assert.Null(await source.DisplayAsync("missing"));

        culture = "fr";
        var localizedIds = (await source.GetEntryIdsAsync()).ToArray();
        var localized = await source.DisplayAsync(localizedIds[0]);

        Assert.Equal(ids, localizedIds);
        Assert.Equal("Fonctionnalites", localized.Title);
        Assert.Equal(["Outils"], localized.Path);
    }

    [Fact]
    public async Task AdminMenuSource_DeniedPermission_ExcludesItemAndItsChildren()
    {
        var provider = new Mock<INavigationProvider>();
        provider.Setup(provider => provider.BuildNavigationAsync(NavigationConstants.AdminId, It.IsAny<NavigationBuilder>()))
            .Callback<string, NavigationBuilder>((_, builder) =>
            {
                builder.Add(new LocalizedString("Allowed", "Allowed"), item => item.Url("allowed"));
                builder.Add(new LocalizedString("Denied", "Denied"), item => item
                    .Permission(new Permission("Denied"))
                    .Add(new LocalizedString("Child", "Child"), child => child.Url("denied-child")));
            })
            .Returns(ValueTask.CompletedTask);
        var authorization = new Mock<IAuthorizationService>();
        authorization.Setup(service => service.AuthorizeAsync(
            It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
            .ReturnsAsync(AuthorizationResult.Failed());
        var manager = new NavigationManager(
            [provider.Object], NullLogger<NavigationManager>.Instance, new ShellSettings(),
            Mock.Of<IUrlHelperFactory>(), authorization.Object);
        using var services = new ServiceCollection().BuildServiceProvider();
        var httpContext = new DefaultHttpContext
        {
            RequestServices = services,
            User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, "user")], "Test")),
        };
        httpContext.Request.PathBase = "/tenant";
        var source = new AdminMenuItemNavigationSource(manager,
            Mock.Of<IHttpContextAccessor>(accessor => accessor.HttpContext == httpContext));

        var id = Assert.Single(await source.GetEntryIdsAsync());
        var result = await source.DisplayAsync(id);

        Assert.Equal("Allowed", result.Title);
        Assert.Equal("/tenant/allowed", result.Href);
        authorization.Verify(service => service.AuthorizeAsync(
            httpContext.User, It.IsAny<object>(), It.IsAny<IEnumerable<IAuthorizationRequirement>>()), Times.Once);
    }

    [Fact]
    public async Task AdminMenuSource_WithoutHttpContext_ReportsMissingRequest()
    {
        var manager = new Mock<INavigationManager>();
        var source = new AdminMenuItemNavigationSource(manager.Object, Mock.Of<IHttpContextAccessor>());

        await Assert.ThrowsAsync<InvalidOperationException>(async () => await source.GetEntryIdsAsync());
        manager.Verify(manager => manager.BuildMenuAsync(It.IsAny<string>(), It.IsAny<ActionContext>()), Times.Never);
    }

    private static QuickNavigationController CreateController(
        IQuickNavigationIndex index, IEnumerable<QuickNavigationSource> sources, bool enabled = true)
    {
        var site = new SiteSettings();
        site.Put(new AdminSettings { DisplayQuickNavigation = enabled });
        var siteService = new Mock<ISiteService>();
        siteService.Setup(service => service.GetSiteSettingsAsync()).ReturnsAsync(site);
        var context = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, "user")], "Test")),
        };
        sources = sources.ToArray();
        foreach (var source in sources.OfType<TestSource>())
        {
            source.HttpContextAccessor = Mock.Of<IHttpContextAccessor>(accessor => accessor.HttpContext == context);
        }

        return new QuickNavigationController(index, sources, siteService.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = context },
        };
    }

    private static QuickNavigationResult[] ReadResults(IActionResult result)
    {
        var file = Assert.IsType<FileContentResult>(result);
        Assert.Equal("application/json; charset=utf-8", file.ContentType);
        return JsonSerializer.Deserialize<QuickNavigationResult[]>(file.FileContents, JsonSerializerOptions.Web);
    }

    private sealed class TestSource : QuickNavigationSource
    {
        private readonly string[] _entryIds;
        private readonly bool _useCurrentCulture;
        private readonly Action _populateIndex;

        public TestSource(string name, string[] entryIds = null, bool useCurrentCulture = false, Action populateIndex = null)
        {
            Name = name;
            _entryIds = entryIds ?? [];
            _useCurrentCulture = useCurrentCulture;
            _populateIndex = populateIndex;
        }

        public override string Name { get; }

        public List<string> Displayed { get; } = [];

        public int GetEntryIdsCalls { get; private set; }

        public IHttpContextAccessor HttpContextAccessor { get; set; }

        public override ValueTask<IEnumerable<string>> GetEntryIdsAsync()
        {
            GetEntryIdsCalls++;
            _populateIndex?.Invoke();
            return ValueTask.FromResult<IEnumerable<string>>(_entryIds);
        }

        public override ValueTask<QuickNavigationResult> DisplayAsync(string entryId)
        {
            Displayed.Add(entryId);
            if (entryId == "hidden" || HttpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated != true)
            {
                return ValueTask.FromResult<QuickNavigationResult>(null);
            }

            var culture = _useCurrentCulture ? CultureInfo.CurrentUICulture.Name : "en";
            return ValueTask.FromResult(new QuickNavigationResult
            {
                Title = culture + ":" + entryId,
                Path = [culture],
                Href = "/tenant/manage/" + entryId,
            });
        }
    }
}
