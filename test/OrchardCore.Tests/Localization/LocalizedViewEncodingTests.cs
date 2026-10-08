using System.Security.Cryptography.X509Certificates;
using AngleSharp.Html.Parser;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Compliance.Redaction;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.Environment.Extensions.Features;
using OrchardCore.Features.Models;
using OrchardCore.Features.ViewModels;
using OrchardCore.Navigation;
using OrchardCore.OpenId.ViewModels;
using OrchardCore.Roles.ViewModels;
using OrchardCore.Search.ViewModels;
using OrchardCore.Security.Permissions;
using OrchardCore.Tests.Apis.Context;
using OrchardCore.Users.AuditTrail.Services;
using OrchardCore.Users.AuditTrail.ViewModels;
using FeaturesAdminController = OrchardCore.Features.Controllers.AdminController;

namespace OrchardCore.Tests.Localization;

public class LocalizedViewEncodingTests
{
    [Theory]
    [InlineData("Author & name")]
    [InlineData("Author \"name\" <label>")]
    public async Task ContentsMeta_LocalizedAuthorTitle_EncodesOnce(string translation)
    {
        using var context = new SiteContext();
        await context.InitializeAsync();

        var model = new Shape();
        model.Properties["ContentItem"] = new ContentItem { Author = "admin" };
        model.Metadata.DisplayType = "SummaryAdmin";

        var html = await RenderAsync(context, "OrchardCore.Contents", "ContentsMeta_SummaryAdmin", model, "Author", translation);
        using var document = new HtmlParser().ParseDocument(html);

        var tooltip = Assert.Single(document.QuerySelectorAll("[data-bs-toggle='tooltip']"));
        Assert.Equal(translation, tooltip.GetAttribute("title"));
    }

    [Theory]
    [InlineData("Search \"words\" & <terms>")]
    [InlineData("Literal &amp; entity")]
    public async Task SearchForm_LocalizedPlaceholder_EncodesAttribute(string translation)
    {
        using var context = new SiteContext();
        await context.InitializeAsync();

        var html = await RenderAsync(context, "OrchardCore.Search", "Search-Form", new SearchFormViewModel(), "Enter your search term(s)", translation);
        using var document = new HtmlParser().ParseDocument(html);

        var input = Assert.Single(document.QuerySelectorAll("input[name='Terms']"));
        Assert.Equal(translation, input.GetAttribute("placeholder"));
        Assert.False(input.HasAttribute("words\""));
    }

    [Fact]
    public void Features_NonInternedCoreCategory_DoesNotOfferDisable()
    {
        var category = new string("Core".ToCharArray());
        var model = CreateFeaturesModel(category);

        // The rows of the features list are built by the controller, which decides what a row offers.
        var entry = FeaturesAdminController.CreateEntry(model, Assert.Single(model.Features), category, tenant: null);

        Assert.False(entry.CanDisable);
        Assert.False(entry.IsSelectable);
    }

    [Theory]
    [InlineData("Other & miscellaneous")]
    [InlineData("Category \"quoted\" <tag>")]
    public async Task Features_CategoryName_EncodesOnce(string category)
    {
        using var context = new SiteContext();
        await context.InitializeAsync();

        // The controller names the categories, "Uncategorized" with its string localizer, so they are plain text.
        var model = CreateFeaturesModel(category);
        model.Groups.Add(new FeatureGroupViewModel { Category = category });

        var html = await RenderAsync(context, "OrchardCore.Features", "Admin/Features", model);
        using var document = new HtmlParser().ParseDocument(html);

        Assert.Equal(category, Assert.Single(document.QuerySelectorAll(".feature-group h3")).TextContent);

        // The select-all checkbox of the category passes its name to the view localizer as an argument.
        var toolbar = new Shape();
        toolbar.Properties["CategoryName"] = category;
        toolbar.Properties["CheckboxId"] = "select-all-category";

        html = await RenderAsync(context, "OrchardCore.Features", "FeaturesGroupToolbar", toolbar);
        using var toolbarDocument = new HtmlParser().ParseDocument(html);

        var label = Assert.Single(toolbarDocument.QuerySelectorAll(".list-group-select-all-label"));
        Assert.Equal($"Select all __COUNT__ features grouped under {category} category", label.GetAttribute("data-select-all-text"));
        Assert.Equal($"Select all 0 features grouped under {category} category", label.TextContent.Trim());
    }

    [Theory]
    [InlineData("Store \"value\" & <text>")]
    [InlineData("Literal &amp; entity")]
    public async Task UsersAuditTrail_LocalizedSelectLabel_EncodesOnce(string translation)
    {
        using var context = new SiteContext();
        await context.InitializeAsync();

        var model = new AuditTrailUserEventSettingsViewModel
        {
            UserSnapshotProperties = [new() { Name = "UserName", Redactor = nameof(NullRedactor) }],
            RedactorNames = [nameof(RemoveRedactor), nameof(NullRedactor)],
        };
        var html = await RenderAsync(context, "OrchardCore.Users", "AuditTrailAdmin/Index", model, "Store", translation);
        using var document = new HtmlParser().ParseDocument(html);

        Assert.Equal(translation, Assert.Single(document.QuerySelectorAll("option[value='NullRedactor']")).TextContent);
    }

    [Theory]
    [InlineData("Certificate \"name\" & <text>")]
    [InlineData("Literal &amp; entity")]
    public async Task OpenId_LocalizedCertificateFallback_EncodesOptionTextOnce(string translation)
    {
        using var context = new SiteContext();
        await context.InitializeAsync();

        var model = new OpenIdServerSettingsViewModel();
        model.AvailableCertificates.Add(new()
        {
            ThumbPrint = "TestCertificate",
            StoreLocation = StoreLocation.CurrentUser,
            StoreName = StoreName.My,
            NotBefore = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            NotAfter = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        });
        var html = await RenderAsync(context, "OrchardCore.OpenId", "OpenIdServerSettings.Edit", model, "No Friendly Name", translation);
        using var document = new HtmlParser().ParseDocument(html);

        var options = document.QuerySelectorAll("option[value='TestCertificate']");
        Assert.Equal(2, options.Length);
        Assert.All(options, option => Assert.StartsWith(translation + " [", option.TextContent.Trim()));
    }

    [Theory]
    [InlineData("Anonymous")]
    [InlineData("Authenticated")]
    public async Task RolePermissions_LocalizedGrantingRole_EncodesTooltipArgumentOnce(string resource)
    {
        using var context = new SiteContext();
        await context.InitializeAsync();

        var permission = new Permission("TestPermission");
        var model = new RolePermissionsViewModel
        {
            RoleCategoryPermissions = new Dictionary<PermissionGroupKey, IEnumerable<Permission>>
            {
                [new("Test", "Test")] = [permission],
            },
            EffectivePermissions = new Dictionary<string, Permission> { [permission.Name] = permission },
            AssignedPermissions = new HashSet<string>(),
            AnonymousGrantedPermissions = new HashSet<string>(),
            AuthenticatedGrantedPermissions = new HashSet<string>(),
        };
        (resource == "Anonymous" ? model.AnonymousGrantedPermissions : model.AuthenticatedGrantedPermissions).Add(permission.Name);
        var translation = "Role \"name\" & <members>";
        var html = await RenderAsync(context, "OrchardCore.Roles", "Admin/_RolePermissions", model, resource, translation);
        using var document = new HtmlParser().ParseDocument(html);

        var tooltip = Assert.Single(document.QuerySelectorAll(".form-check[data-bs-toggle='tooltip']"));
        Assert.Equal("This permission is granted by role: " + translation, tooltip.GetAttribute("title"));
    }

    [Fact]
    public async Task AdminNavigation_LocalizedToggleLabel_FormatsAndEncodesOnce()
    {
        using var context = new SiteContext();
        await context.InitializeAsync();

        var text = "Menu \"name\" & <items>";
        var model = new NavigationItemViewModel
        {
            Text = new LocalizedString("Menu", text),
            Href = "/menu",
            Level = 1,
            Hash = "TestMenu",
        };
        await model.AddAsync(new NavigationItemViewModel
        {
            Text = new LocalizedString("Child", "Child"),
            Href = "/child",
            Level = 2,
        }, "0");
        var html = await RenderAsync(context, "TheAdmin", "NavigationItem-admin", model, "Toggle {0}", "Expand {0}");
        using var document = new HtmlParser().ParseDocument(html);

        var toggle = Assert.Single(document.QuerySelectorAll("button.nav-toggle"));
        Assert.Equal("Expand " + text, toggle.GetAttribute("aria-label"));
    }

    private static FeaturesViewModel CreateFeaturesModel(string category)
        => new()
        {
            Features =
            [
                new ModuleFeature
                {
                    Descriptor = new FeatureInfo("TestFeature", "Test Feature", 0, category, "Test description", null, [], false, false, false),
                    IsEnabled = true,
                    FeatureDependencies = [],
                    EnabledDependentFeatures = [],
                },
            ],
        };

    private static async Task<string> RenderAsync(SiteContext context, string module, string viewName, object model, string resource = null, string translation = null)
    {
        var html = string.Empty;
        await context.UsingTenantScopeAsync(async scope =>
        {
            var localizer = new Mock<IViewLocalizer>();
            localizer.As<IViewContextAware>();
            localizer.Setup(l => l[It.IsAny<string>()])
                .Returns((string name) => new LocalizedHtmlString(name, name == resource ? translation : name));
            localizer.Setup(l => l[It.IsAny<string>(), It.IsAny<object[]>()])
                .Returns((string name, object[] arguments) => new LocalizedHtmlString(name, name == resource ? translation : name, false, arguments));
            localizer.Setup(l => l.GetString(It.IsAny<string>()))
                .Returns((string name) => new LocalizedString(name, name == resource ? translation : name));
            localizer.Setup(l => l.GetString(It.IsAny<string>(), It.IsAny<object[]>()))
                .Returns((string name, object[] arguments) => new LocalizedString(name, string.Format(name == resource ? translation : name, arguments)));

            var httpContext = SiteContext.HttpContextAccessor.HttpContext;
            var services = httpContext.RequestServices;
            var endpoint = httpContext.GetEndpoint();
            httpContext.RequestServices = new LocalizedViewServiceProvider(scope.ServiceProvider, localizer.Object);
            httpContext.SetEndpoint(new Endpoint(_ => Task.CompletedTask, new EndpointMetadataCollection(), "Localized view test"));

            try
            {
                var routeData = new RouteData();
                routeData.Values["area"] = module;
                routeData.Values["controller"] = "Admin";
                routeData.Values["action"] = "Features";
                var actionContext = new ActionContext(httpContext, routeData, new ActionDescriptor());
                var viewEngine = scope.ServiceProvider.GetRequiredService<IRazorViewEngine>();
                var result = viewEngine.GetView(null, $"/Areas/{module}/Views/{viewName}.cshtml", isMainPage: false);
                Assert.True(result.Success, string.Join(System.Environment.NewLine, result.SearchedLocations ?? []));

                using var writer = new StringWriter();
                var viewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary()) { Model = model };
                var tempData = new TempDataDictionary(httpContext, scope.ServiceProvider.GetRequiredService<ITempDataProvider>());
                var viewContext = new ViewContext(actionContext, result.View, viewData, tempData, writer, new HtmlHelperOptions());
                await result.View.RenderAsync(viewContext);
                html = writer.ToString();
            }
            finally
            {
                httpContext.RequestServices = services;
                httpContext.SetEndpoint(endpoint);
            }
        });

        return html;
    }

    private sealed class LocalizedViewServiceProvider(IServiceProvider services, IViewLocalizer localizer) : IServiceProvider
    {
        public object GetService(Type serviceType)
            => serviceType == typeof(IViewLocalizer) ? localizer : services.GetService(serviceType);
    }
}
