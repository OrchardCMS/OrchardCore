using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Localization;
using Moq;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Descriptors.ShapeTemplateStrategy;
using OrchardCore.DisplayManagement.Manifest;
using OrchardCore.Data.Documents;
using OrchardCore.Documents;
using OrchardCore.Environment.Extensions;
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
    private readonly Dictionary<string, ShapeBinding> _bindings = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<IExtensionInfo> _extensions = [];

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

    [Fact]
    public void ComputeHash_DifferentLineEndings_ReturnsSameHash()
    {
        Assert.Equal(SourceTemplatesService.ComputeHash("a\r\nb"), SourceTemplatesService.ComputeHash("a\nb"));
        Assert.NotEqual(SourceTemplatesService.ComputeHash("a\nb"), SourceTemplatesService.ComputeHash("a\nc"));
    }

    [Fact]
    public async Task GetTemplatesAsync_SiteTheme_ListsTemplatesOfThemeAndBaseTheme()
    {
        AddExtension("BaseTheme");
        AddExtension("SiteTheme", baseTheme: "BaseTheme");
        AddExtension("OrchardCore.Contents");
        AddFile("SiteTheme", "Views/Layout.liquid", "{{ Model.Content | shape_render }}");
        AddFile("BaseTheme", "Views/Menu.cshtml", "@Model");
        AddFile("OrchardCore.Contents", "Views/Content.cshtml", "@Model");

        var templates = await CreateSource().GetTemplatesAsync(adminTemplates: false);

        Assert.Equal(["Layout", "Menu"], templates.Select(template => template.Name).Order());
        Assert.Contains(templates, template => template.Name == "Layout" && template.IsLiquid && template.ExtensionId == "SiteTheme");
        Assert.Contains(templates, template => template.Name == "Menu" && !template.IsLiquid && template.RelativePath == "Views/Menu.cshtml");
    }

    [Fact]
    public async Task GetTemplatesAsync_AdminTemplates_ListsTemplatesOfAdminTheme()
    {
        AddExtension("SiteTheme");
        AddExtension("AdminTheme");
        AddFile("SiteTheme", "Views/Layout.liquid", "site");
        AddFile("AdminTheme", "Views/Layout.cshtml", "admin");

        var template = Assert.Single(await CreateSource().GetTemplatesAsync(adminTemplates: true));

        Assert.Equal("AdminTheme", template.ExtensionId);
    }

    [Fact]
    public async Task CloneAsync_LiquidTemplate_IsMonitoredUntilDeleted()
    {
        AddExtension("SiteTheme");
        AddFile("SiteTheme", "Views/Layout.liquid", "original");
        var service = CreateService();
        var source = service.GetSource(ThemeTemplateSource.SourceName);

        var entry = Assert.Single(await service.GetEntriesAsync(source, adminTemplates: false));
        Assert.False(entry.IsOverridden);

        Assert.True(await service.CloneAsync(source, entry.Template, adminTemplates: false, overwrite: false));
        Assert.Equal("original", _templates.Templates["Layout"].Content);
        entry = Assert.Single(await service.GetEntriesAsync(source, adminTemplates: false));
        Assert.True(entry.IsOverridden && entry.IsCloned);
        Assert.False(entry.IsModified || entry.IsSourceChanged);

        // A second clone would lose the local template.
        Assert.False(await service.CloneAsync(source, entry.Template, adminTemplates: false, overwrite: false));

        _templates.Templates["Layout"].Content = "edited";
        AddFile("SiteTheme", "Views/Layout.liquid", "updated");
        entry = Assert.Single(await service.GetEntriesAsync(source, adminTemplates: false));
        Assert.True(entry.IsModified && entry.IsSourceChanged);

        Assert.True(await service.CloneAsync(source, entry.Template, adminTemplates: false, overwrite: true));
        Assert.Equal("updated", _templates.Templates["Layout"].Content);
        entry = Assert.Single(await service.GetEntriesAsync(source, adminTemplates: false));
        Assert.False(entry.IsModified || entry.IsSourceChanged);

        Assert.True(await service.DeleteAsync("Layout", adminTemplates: false));
        Assert.Empty(_templates.Templates);
        Assert.Empty(_overrides.Overrides);
    }

    [Fact]
    public async Task CloneAsync_RazorTemplate_ReturnsFalse()
    {
        AddExtension("AdminTheme");
        AddFile("AdminTheme", "Views/Layout.cshtml", "@Model");
        var service = CreateService();
        var source = service.GetSource(ThemeTemplateSource.SourceName);

        var entry = Assert.Single(await service.GetEntriesAsync(source, adminTemplates: true));

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

        var entry = Assert.Single(await service.GetEntriesAsync(service.GetSource(ThemeTemplateSource.SourceName), adminTemplates: false));

        Assert.True(entry.IsOverridden);
        Assert.False(entry.IsCloned || entry.IsModified || entry.IsSourceChanged);
    }

    private void AddExtension(string id, string baseTheme = null)
    {
        var manifest = new Mock<IManifestInfo>();
        manifest.SetupGet(manifest => manifest.ModuleInfo).Returns(new ThemeAttribute { BaseTheme = baseTheme });

        var extension = new Mock<IExtensionInfo>();
        extension.SetupGet(extension => extension.Id).Returns(id);
        extension.SetupGet(extension => extension.SubPath).Returns(Application.ModulesRoot + id);
        extension.SetupGet(extension => extension.Exists).Returns(true);
        extension.SetupGet(extension => extension.Manifest).Returns(manifest.Object);

        _extensions.Add(extension.Object);
    }

    private void AddFile(string extensionId, string relativePath, string content)
    {
        var filePath = $"{Application.ModulesRoot}{extensionId}/{relativePath}";
        var fullPath = Path.Combine(_root.FullName, filePath);

        Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
        File.WriteAllText(fullPath, content);

        Assert.True(SourceTemplateNames.TryGetTemplateName(relativePath, out var name));
        _bindings[filePath] = new ShapeBinding { BindingName = name.ToLowerInvariant(), BindingSource = filePath };
    }

    private ThemeTemplateSource CreateSource()
    {
        var shapeTableManager = new Mock<IShapeTableManager>();
        shapeTableManager
            .Setup(manager => manager.GetShapeTableAsync(It.IsAny<string>()))
            .ReturnsAsync((string themeId) => new ShapeTable(
                new Dictionary<string, ShapeDescriptor>(),
                _bindings
                    .Where(binding => !IsOtherTheme(binding.Value.BindingSource, themeId))
                    .ToDictionary(binding => binding.Value.BindingName, binding => binding.Value, StringComparer.OrdinalIgnoreCase)));

        var extensionManager = new Mock<IExtensionManager>();
        extensionManager.Setup(manager => manager.GetExtensions()).Returns(() => _extensions);
        extensionManager.Setup(manager => manager.GetExtension(It.IsAny<string>()))
            .Returns((string id) => _extensions.FirstOrDefault(extension => extension.Id == id));

        var fileProviderAccessor = new Mock<IShapeTemplateFileProviderAccessor>();
        fileProviderAccessor.SetupGet(accessor => accessor.FileProvider).Returns(new PhysicalFileProvider(_root.FullName));

        var siteThemeService = new Mock<ISiteThemeService>();
        siteThemeService.Setup(service => service.GetSiteThemeNameAsync()).ReturnsAsync("SiteTheme");

        var adminThemeService = new Mock<IAdminThemeService>();
        adminThemeService.Setup(service => service.GetAdminThemeNameAsync()).ReturnsAsync("AdminTheme");

        var localizer = new Mock<IStringLocalizer<ThemeTemplateSource>>();
        localizer.Setup(localizer => localizer[It.IsAny<string>()]).Returns((string name) => new LocalizedString(name, name));

        return new ThemeTemplateSource(
            new ShapeTemplateFiles(shapeTableManager.Object, extensionManager.Object, fileProviderAccessor.Object),
            extensionManager.Object,
            [siteThemeService.Object],
            [adminThemeService.Object],
            localizer.Object);
    }

    // The shape table of a theme only has the templates of the theme, of the themes it is based on, and of the modules.
    private bool IsOtherTheme(string filePath, string themeId)
    {
        var themeIds = new List<string> { themeId };
        var baseTheme = (_extensions.FirstOrDefault(extension => extension.Id == themeId)?.Manifest.ModuleInfo as ThemeAttribute)?.BaseTheme;

        if (!string.IsNullOrEmpty(baseTheme))
        {
            themeIds.Add(baseTheme);
        }

        return filePath.Contains("Theme/", StringComparison.Ordinal)
            && !themeIds.Any(id => filePath.StartsWith(Application.ModulesRoot + id + "/", StringComparison.Ordinal));
    }

    private SourceTemplatesService CreateService()
    {
        var source = CreateSource();
        var fileProviderAccessor = new Mock<IShapeTemplateFileProviderAccessor>();
        fileProviderAccessor.SetupGet(accessor => accessor.FileProvider).Returns(new PhysicalFileProvider(_root.FullName));

        var clock = new Mock<IClock>();
        clock.SetupGet(clock => clock.UtcNow).Returns(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));

        return new SourceTemplatesService(
            [source],
            new ShapeTemplateFiles(Mock.Of<IShapeTableManager>(), Mock.Of<IExtensionManager>(), fileProviderAccessor.Object),
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
