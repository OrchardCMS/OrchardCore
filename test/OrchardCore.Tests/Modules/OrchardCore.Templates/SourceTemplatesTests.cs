using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Localization;
using Moq;
using OrchardCore.Admin;
using OrchardCore.Data.Documents;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Descriptors.ShapeTemplateStrategy;
using OrchardCore.DisplayManagement.Manifest;
using OrchardCore.Documents;
using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Extensions.Features;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Extensions.Manifests;
using OrchardCore.Modules;
using OrchardCore.Modules.Manifest;
using OrchardCore.Templates.Models;
using OrchardCore.Templates.Services;
using OrchardCore.Themes.Services;

namespace OrchardCore.Tests.Modules.OrchardCore.Templates;

public sealed class SourceTemplatesTests : IDisposable
{
    private readonly DirectoryInfo _root = Directory.CreateTempSubdirectory("source-templates-");
    private readonly TemplatesDocument _templates = new();
    private readonly AdminTemplatesDocument _adminTemplates = new();
    private readonly TemplateOverridesDocument _overrides = new();
    private readonly AdminTemplateOverridesDocument _adminOverrides = new();
    private readonly List<IExtensionInfo> _extensions = [];
    private readonly Dictionary<string, ShapeBinding> _bindings = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _disabled = [];
    private string _siteThemeId = "SiteTheme";

    public void Dispose() => _root.Delete(recursive: true);

    [Theory]
    [InlineData("Views/Layout.liquid", "Layout")]
    [InlineData("Views/Content-BlogPost.Summary.liquid", "Content_Summary__BlogPost")]
    [InlineData("Views/MenuItemLink-ContentMenuItem.liquid", "MenuItemLink__ContentMenuItem")]
    [InlineData("Views/Parts/Foo-Bar.liquid", "Parts_Foo__Bar")]
    [InlineData("Views/Items/Content.Summary.cshtml", "Content_Summary")]
    [InlineData("Views/Layout.cshtml", "Layout")]
    public void TryGetTemplateName_ShapeTemplate_ReturnsName(string relativePath, string expectedName)
    {
        Assert.True(SourceTemplateNames.TryGetTemplateName(relativePath, out var name));
        Assert.Equal(expectedName, name);
    }

    [Theory]
    [InlineData("Views/_ViewImports.cshtml")]
    [InlineData("Views/Shared/Layout.liquid")]
    [InlineData("Recipes/Snippets/Content__ComingSoon.liquid")]
    [InlineData("Views/Layout")]
    [InlineData("")]
    public void TryGetTemplateName_OtherFile_ReturnsFalse(string relativePath)
    {
        Assert.False(SourceTemplateNames.TryGetTemplateName(relativePath, out _));
    }

    [Theory]
    [InlineData("TextField_Edit", true)]
    [InlineData("Content_SummaryAdmin", true)]
    [InlineData("ContentPartSettings_Edit", true)]
    [InlineData("NavigationItemText__multitenancy_Id", true)]
    [InlineData("ScriptTask_Fields_Design", true)]
    [InlineData("Content_Summary__BlogPost", false)]
    [InlineData("TextField", false)]
    [InlineData("MenuItemLink__ContentMenuItem", false)]
    [InlineData("Credits", false)]
    public void IsAdminTemplate_TemplateName_GuessesAdmin(string templateName, bool expected)
    {
        Assert.Equal(expected, SourceTemplateNames.IsAdminTemplate(templateName));
    }

    [Fact]
    public void ComputeHash_DifferentLineEndings_ReturnsSameHash()
    {
        Assert.Equal(SourceTemplatesService.ComputeHash("a\r\nb"), SourceTemplatesService.ComputeHash("a\nb"));
        Assert.NotEqual(SourceTemplatesService.ComputeHash("a\nb"), SourceTemplatesService.ComputeHash("a\nc"));
    }

    [Fact]
    public async Task GetTemplatesAsync_ActiveSiteTheme_ListsTemplatesOfThemeAndBaseTheme()
    {
        AddExtension("BaseTheme");
        AddExtension("SiteTheme", baseTheme: "BaseTheme");
        AddExtension("OrchardCore.Contents", isTheme: false);
        AddFile("SiteTheme", "Views/Layout.liquid", "site");
        AddFile("BaseTheme", "Views/Layout.liquid", "base");
        AddFile("BaseTheme", "Views/Menu.cshtml", "@Model");
        AddFile("OrchardCore.Contents", "Views/Content.cshtml", "@Model");

        var templates = (await CreateSource().GetTemplatesAsync(adminTemplates: false, scope: null)).ToList();

        Assert.Equal(["Layout", "Menu"], templates.Select(template => template.Name).Order());
        Assert.All(templates, template => Assert.True(template.IsInUse));

        // The template of the theme wins over the one of the theme it is based on.
        Assert.Contains(templates, template => template.Name == "Layout" && template.CanClone && template.ExtensionId == "SiteTheme");
        Assert.Contains(templates, template => template.Name == "Menu" && !template.CanClone && template.RelativePath == "Views/Menu.cshtml");
    }

    [Fact]
    public async Task GetScopesAsync_SiteTemplates_ListsSiteThemesActiveFirst()
    {
        AddExtension("AnotherTheme");
        AddExtension("SiteTheme");
        AddExtension("AdminTheme", admin: true);
        AddExtension("OrchardCore.Contents", isTheme: false);

        var scopes = (await CreateSource().GetScopesAsync(adminTemplates: false)).ToList();

        Assert.Equal(["SiteTheme", "AnotherTheme"], scopes.Select(scope => scope.Name));
        Assert.True(scopes[0].IsInUse);
        Assert.False(scopes[1].IsInUse);
    }

    [Fact]
    public async Task GetScopesAsync_AdminTemplates_ListsAdminThemes()
    {
        AddExtension("SiteTheme");
        AddExtension("AdminTheme", admin: true);

        var scope = Assert.Single(await CreateSource().GetScopesAsync(adminTemplates: true));

        Assert.Equal("AdminTheme", scope.Name);
        Assert.True(scope.IsInUse);
    }

    [Fact]
    public async Task GetTemplatesAsync_InactiveTheme_CanOnlyBeViewed()
    {
        AddExtension("SiteTheme");
        AddExtension("AnotherTheme");
        AddFile("AnotherTheme", "Views/Layout.liquid", "another");
        var service = CreateService();
        var source = service.GetSource(ThemeTemplateSource.SourceName);

        var entry = Assert.Single(await service.GetEntriesAsync(source, adminTemplates: false, scope: "AnotherTheme"));

        Assert.False(entry.Template.IsInUse);
        Assert.False(entry.Template.CanClone);
        Assert.False(await service.CloneAsync(source, entry.Template, adminTemplates: false, overwrite: false));
        Assert.Empty(_templates.Templates);
    }

    [Fact]
    public async Task GetTemplatesAsync_ThemeOfTheOtherList_ReturnsNothing()
    {
        AddExtension("SiteTheme");
        AddExtension("AdminTheme", admin: true);
        AddFile("AdminTheme", "Views/Layout.cshtml", "admin");

        Assert.Empty(await CreateSource().GetTemplatesAsync(adminTemplates: false, scope: "AdminTheme"));
    }

    [Fact]
    public async Task CloneAsync_LiquidTemplate_IsMonitoredUntilDeleted()
    {
        AddExtension("SiteTheme");
        AddFile("SiteTheme", "Views/Layout.liquid", "original");
        var service = CreateService();
        var source = service.GetSource(ThemeTemplateSource.SourceName);

        var entry = Assert.Single(await service.GetEntriesAsync(source, adminTemplates: false, scope: null));
        Assert.False(entry.IsOverridden);

        Assert.True(await service.CloneAsync(source, entry.Template, adminTemplates: false, overwrite: false));
        Assert.Equal("original", _templates.Templates["Layout"].Content);
        entry = Assert.Single(await service.GetEntriesAsync(source, adminTemplates: false, scope: null));
        Assert.True(entry.IsOverridden && entry.IsCloned);
        Assert.False(entry.IsModified || entry.IsSourceChanged);

        // A second clone would lose the local template.
        Assert.False(await service.CloneAsync(source, entry.Template, adminTemplates: false, overwrite: false));

        _templates.Templates["Layout"].Content = "edited";
        AddFile("SiteTheme", "Views/Layout.liquid", "updated");
        entry = Assert.Single(await service.GetEntriesAsync(source, adminTemplates: false, scope: null));
        Assert.True(entry.IsModified && entry.IsSourceChanged);

        Assert.True(await service.CloneAsync(source, entry.Template, adminTemplates: false, overwrite: true));
        Assert.Equal("updated", _templates.Templates["Layout"].Content);
        entry = Assert.Single(await service.GetEntriesAsync(source, adminTemplates: false, scope: null));
        Assert.False(entry.IsModified || entry.IsSourceChanged);

        Assert.True(await service.DeleteAsync("Layout", adminTemplates: false));
        Assert.Empty(_templates.Templates);
        Assert.Empty(_overrides.Overrides);
    }

    [Fact]
    public async Task GetEntriesAsync_ClonedFromThemeActiveBefore_IsNotComparedToTheActiveTheme()
    {
        AddExtension("SiteTheme");
        AddExtension("NewTheme");
        AddFile("SiteTheme", "Views/Layout.liquid", "site");
        AddFile("NewTheme", "Views/Layout.liquid", "new");
        var service = CreateService();
        var source = service.GetSource(ThemeTemplateSource.SourceName);
        var entry = Assert.Single(await service.GetEntriesAsync(source, adminTemplates: false, scope: null));
        Assert.True(await service.CloneAsync(source, entry.Template, adminTemplates: false, overwrite: false));

        _siteThemeId = "NewTheme";
        entry = Assert.Single(await service.GetEntriesAsync(source, adminTemplates: false, scope: null));

        Assert.True(entry.IsOverridden);
        Assert.Equal("SiteTheme", entry.ClonedFrom);
        Assert.False(entry.IsCloned || entry.IsModified || entry.IsSourceChanged);
    }

    [Fact]
    public async Task CloneAsync_RazorTemplate_ReturnsFalse()
    {
        AddExtension("AdminTheme", admin: true);
        AddFile("AdminTheme", "Views/Layout.cshtml", "@Model");
        var service = CreateService();
        var source = service.GetSource(ThemeTemplateSource.SourceName);

        var entry = Assert.Single(await service.GetEntriesAsync(source, adminTemplates: true, scope: null));

        Assert.False(await service.CloneAsync(source, entry.Template, adminTemplates: true, overwrite: false));
        Assert.Empty(_adminTemplates.Templates);
    }

    [Fact]
    public async Task GetEntriesAsync_TemplateWrittenByHand_IsOverriddenButNotCloned()
    {
        AddExtension("SiteTheme");
        AddFile("SiteTheme", "Views/Layout.liquid", "original");
        _templates.Templates["layout"] = new Template { Content = "by hand" };
        var service = CreateService();

        var entry = Assert.Single(await service.GetEntriesAsync(service.GetSource(ThemeTemplateSource.SourceName), adminTemplates: false, scope: null));

        Assert.True(entry.IsOverridden);
        Assert.False(entry.IsCloned || entry.IsModified || entry.IsSourceChanged);
        Assert.Null(entry.ClonedFrom);
    }

    [Fact]
    public async Task GetTemplatesAsync_AllFeatures_ListsTemplateOfLastEnabledModuleByShape()
    {
        AddExtension("SiteTheme");
        AddExtension("Base.Module", isTheme: false);
        AddExtension("Dependent.Module", isTheme: false);
        AddExtension("Disabled.Module", isTheme: false, enabled: false);
        AddFile("Base.Module", "Views/TextField.cshtml", "base");
        AddFile("Base.Module", "Views/TextField.Edit.cshtml", "editor");
        AddFile("Dependent.Module", "Views/TextField.cshtml", "dependent");
        AddFile("Disabled.Module", "Views/Disabled.cshtml", "disabled");
        AddFile("SiteTheme", "Views/Layout.liquid", "theme");

        var templates = (await CreateFeatureSource().GetTemplatesAsync(adminTemplates: false, scope: null)).ToList();

        Assert.Equal(["TextField", "TextField_Edit"], templates.Select(template => template.Name).Order());
        Assert.Contains(templates, template => template.Name == "TextField" && template.ExtensionId == "Dependent.Module" && template.OverriddenBy == null && template.IsAdmin == false);
        Assert.Contains(templates, template => template.Name == "TextField_Edit" && template.IsAdmin == true);
    }

    [Fact]
    public async Task GetTemplatesAsync_OneFeature_MarksTemplatesOverriddenByThemeOrModule()
    {
        AddExtension("SiteTheme");
        AddExtension("Base.Module", isTheme: false);
        AddExtension("Dependent.Module", isTheme: false);
        AddFile("Base.Module", "Views/TextField.cshtml", "base");
        AddFile("Base.Module", "Views/Menu.cshtml", "menu");
        AddFile("Base.Module", "Views/Content.cshtml", "content");
        AddFile("Dependent.Module", "Views/TextField.cshtml", "dependent");
        AddFile("SiteTheme", "Views/Menu.liquid", "theme menu");
        Bind("Menu", "SiteTheme", "Views/Menu.liquid");
        Bind("TextField", "Dependent.Module", "Views/TextField.cshtml");
        Bind("Content", "Base.Module", "Views/Content.cshtml");

        var source = CreateFeatureSource();
        var templates = (await source.GetTemplatesAsync(adminTemplates: false, scope: "Base.Module")).ToDictionary(template => template.Name);

        Assert.Equal("SiteTheme", templates["Menu"].OverriddenBy);
        Assert.Equal("Dependent.Module", templates["TextField"].OverriddenBy);
        Assert.Null(templates["Content"].OverriddenBy);

        var scopes = (await source.GetScopesAsync(adminTemplates: false)).Select(scope => scope.Name).ToList();
        Assert.Equal([FeatureTemplateSource.AllFeaturesScope, "Base.Module", "Dependent.Module"], scopes);
    }

    private void Bind(string shapeType, string extensionId, string relativePath)
    {
        var filePath = $"{Application.ModulesRoot}{extensionId}/{relativePath}";
        _bindings[shapeType] = new ShapeBinding { BindingName = shapeType.ToLowerInvariant(), BindingSource = filePath };
    }

    private FeatureTemplateSource CreateFeatureSource()
    {
        var shellFeaturesManager = new Mock<IShellFeaturesManager>();
        shellFeaturesManager.Setup(manager => manager.GetEnabledFeaturesAsync()).ReturnsAsync(() => _extensions
            .Where(extension => !_disabled.Contains(extension.Id))
            .Select(extension =>
            {
                var feature = new Mock<IFeatureInfo>();
                feature.SetupGet(feature => feature.Id).Returns(extension.Id);
                feature.SetupGet(feature => feature.Extension).Returns(extension);

                return feature.Object;
            })
            .ToList());

        var siteThemeService = new Mock<ISiteThemeService>();
        siteThemeService.Setup(service => service.GetSiteThemeNameAsync()).ReturnsAsync(() => _siteThemeId);

        var localizer = new Mock<IStringLocalizer<FeatureTemplateSource>>();
        localizer.Setup(localizer => localizer[It.IsAny<string>()]).Returns((string name) => new LocalizedString(name, name));

        return new FeatureTemplateSource(
            CreateShapeTemplateFiles(),
            shellFeaturesManager.Object,
            new ActiveThemes([siteThemeService.Object], []),
            localizer.Object);
    }

    private void AddExtension(string id, string baseTheme = null, bool isTheme = true, bool admin = false, bool enabled = true)
    {
        var manifest = new Mock<IManifestInfo>();
        manifest.SetupGet(manifest => manifest.ModuleInfo).Returns(isTheme ? new ThemeAttribute { BaseTheme = baseTheme } : new ModuleAttribute());
        manifest.SetupGet(manifest => manifest.Name).Returns(id);
        manifest.SetupGet(manifest => manifest.Tags).Returns(admin ? [ManifestConstants.AdminTag] : []);

        var extension = new Mock<IExtensionInfo>();
        extension.SetupGet(extension => extension.Id).Returns(id);
        extension.SetupGet(extension => extension.SubPath).Returns(Application.ModulesRoot + id);
        extension.SetupGet(extension => extension.Exists).Returns(true);
        extension.SetupGet(extension => extension.Manifest).Returns(manifest.Object);
        extension.SetupGet(extension => extension.Features).Returns([]);

        _extensions.Add(extension.Object);

        if (!enabled)
        {
            _disabled.Add(id);
        }
    }

    private void AddFile(string extensionId, string relativePath, string content)
    {
        var fullPath = Path.Combine(_root.FullName, $"{Application.ModulesRoot}{extensionId}/{relativePath}");

        Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
        File.WriteAllText(fullPath, content);
    }

    private IExtensionManager CreateExtensionManager()
    {
        var extensionManager = new Mock<IExtensionManager>();
        extensionManager.Setup(manager => manager.GetExtensions()).Returns(() => _extensions);
        extensionManager.Setup(manager => manager.GetExtension(It.IsAny<string>()))
            .Returns((string id) => _extensions.FirstOrDefault(extension => extension.Id == id));

        return extensionManager.Object;
    }

    private ShapeTemplateFiles CreateShapeTemplateFiles()
    {
        var fileProviderAccessor = new Mock<IShapeTemplateFileProviderAccessor>();
        fileProviderAccessor.SetupGet(accessor => accessor.FileProvider).Returns(new PhysicalFileProvider(_root.FullName));

        var liquid = new Mock<IShapeTemplateViewEngine>();
        liquid.SetupGet(engine => engine.TemplateFileExtensions).Returns([".liquid"]);

        var razor = new Mock<IShapeTemplateViewEngine>();
        razor.SetupGet(engine => engine.TemplateFileExtensions).Returns([".cshtml"]);

        var shapeTableManager = new Mock<IShapeTableManager>();
        shapeTableManager.Setup(manager => manager.GetShapeTableAsync(It.IsAny<string>()))
            .ReturnsAsync(() => new ShapeTable(new Dictionary<string, ShapeDescriptor>(), _bindings.Values.ToDictionary(binding => binding.BindingName, StringComparer.OrdinalIgnoreCase)));

        return new ShapeTemplateFiles(shapeTableManager.Object, CreateExtensionManager(), fileProviderAccessor.Object, [liquid.Object, razor.Object]);
    }

    private ThemeTemplateSource CreateSource()
    {
        var siteThemeService = new Mock<ISiteThemeService>();
        siteThemeService.Setup(service => service.GetSiteThemeNameAsync()).ReturnsAsync(() => _siteThemeId);

        var adminThemeService = new Mock<IAdminThemeService>();
        adminThemeService.Setup(service => service.GetAdminThemeNameAsync()).ReturnsAsync("AdminTheme");

        var localizer = new Mock<IStringLocalizer<ThemeTemplateSource>>();
        localizer.Setup(localizer => localizer[It.IsAny<string>()]).Returns((string name) => new LocalizedString(name, name));

        return new ThemeTemplateSource(
            CreateShapeTemplateFiles(),
            CreateExtensionManager(),
            new ActiveThemes([siteThemeService.Object], [adminThemeService.Object]),
            localizer.Object);
    }

    private SourceTemplatesService CreateService()
    {
        var clock = new Mock<IClock>();
        clock.SetupGet(clock => clock.UtcNow).Returns(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));

        return new SourceTemplatesService(
            [CreateSource()],
            CreateShapeTemplateFiles(),
            new TemplatesManager(CreateDocumentManager(_templates)),
            new AdminTemplatesManager(CreateDocumentManager(_adminTemplates)),
            CreateDocumentManager(_overrides),
            CreateDocumentManager(_adminOverrides),
            clock.Object);
    }

    // The tests change and read the same document, as a store would after it is committed.
    private static IDocumentManager<TDocument> CreateDocumentManager<TDocument>(TDocument document)
        where TDocument : class, IDocument, new()
    {
        var documentManager = new Mock<IDocumentManager<TDocument>>();
        documentManager.Setup(manager => manager.GetOrCreateMutableAsync(It.IsAny<Func<Task<TDocument>>>())).ReturnsAsync(document);
        documentManager.Setup(manager => manager.GetOrCreateImmutableAsync(It.IsAny<Func<Task<TDocument>>>())).ReturnsAsync(document);
        documentManager.Setup(manager => manager.UpdateAsync(It.IsAny<TDocument>(), It.IsAny<Func<TDocument, Task>>())).Returns(Task.CompletedTask);

        return documentManager.Object;
    }
}
